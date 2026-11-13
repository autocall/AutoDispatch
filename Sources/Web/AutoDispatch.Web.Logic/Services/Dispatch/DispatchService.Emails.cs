using LinqToDB;
using LinqToDB.Async;
using AutoCall.Common.Attributes;
using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Email;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Utils;
using AutoDispatch.Web.Logic.WebServices;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

public partial class DispatchService {

    private static SmtpSettings Smtp(DispatchCompany company) =>
        new SmtpSettings(company.SMTP_Email, company.SMTP_Password, company.SMTP_Host, company.SMTP_Port);

    private static void CheckEmails(IEnumerable<string> emails) {
        foreach (var email in emails) {
            if (EmailUtils.IsValidEmail(email) == false) {
                throw new InfoException("Email is not valid");
            }
        }
    }

    #region [ setup packet ]

    /// <summary>
    ///     Карриер принял setup packet: письма с Insurance Instructions и Setup Packet + W9 (PDF), файлы сохраняются в каталог карриера.
    ///     RTF→PDF — ReportService.RtfToPdf (Telerik, решение не принято — AI/wiki/plan.md) </summary>
    private async Task SendSetupPacketAceeptEmailAsync(DatCompanyDto carrier) {
        var actualCompanyService = Host.GetService<DispatchCompanyService>();
        var setupPacketService = Host.GetService<SetupPacketService>();
        var reportService = Host.GetService<ReportService>();

        var company = await actualCompanyService.GetByIdAsync(carrier.ActualCompanyId);
        SetupPacketDto setupPacketDto = await setupPacketService.GetByIdAsync(carrier.SetupPacketId);

        var fields = reportService.GetFields(setupPacketDto, carrier, company);

        var destinDir = DispatchFileHelper.FileGetFullPath(carrier.EntityId);

        #region [ Insurance Instructions Email ]
        {
            var sourceFilePathInsuranceInstructions = Path.Combine(AppSettings.DocumentsPath, "05_02.rtf");
            var binaryInsuranceInstructions = reportService.GenerateInsuranceInstructionsPdf(sourceFilePathInsuranceInstructions, setupPacketDto, fields);

            // Creates and sends letter
            var email = await DispatchEmailGenerator.GenerateInsuranceInstructionsCompletedAsync(setupPacketDto, carrier, company,
                new EmailAttachment() {
                    Content = Convert.ToBase64String(binaryInsuranceInstructions),
                    FileName = "Insurance_Instructions.pdf"
                });

#if !DEBUG
            await setupPacketService.SendAsync(carrier, email);
#endif
            Directory.CreateDirectory(destinDir);
        }
        #endregion [ Insurance Instructions Email ]

        #region [ Setup Packet Email ]
        {
            var destinFilePathW9 = Path.Combine(destinDir, $"fw9_{DateTime.Now:yyyy_MM_dd}.pdf");
            var sourceFilePathW9 = Path.Combine(AppSettings.DocumentsPath, "fw9.pdf");
            var binaryW9 = reportService.GenerateW9Pdf(sourceFilePathW9, setupPacketDto, fields);

            var destinFilePathSP = DispatchFileHelper.GetFileByVersion(Path.Combine(destinDir, $"setup_packet_{DateTime.Now:yyyy_MM_dd}.pdf"), setupPacketDto.Version);
            var sourceFilePathSP = DispatchFileHelper.GetFileByVersion(Path.Combine(AppSettings.DocumentsPath, "000102040506071314.rtf"), setupPacketDto.Version);
            var binarySP = reportService.GenerateSetupPacketPdf(sourceFilePathSP, setupPacketDto, fields);

            carrier.W9File = Path.GetFileName(destinFilePathW9);
            carrier.SetupPacketFile = Path.GetFileName(destinFilePathSP);

            // Creates and sends letter
            var email = await DispatchEmailGenerator.GenerateSetupPacketCompletedAsync(setupPacketDto, carrier, company,
                new EmailAttachment() { Content = Convert.ToBase64String(binaryW9), FileName = "fw9.pdf" },
                new EmailAttachment() { Content = Convert.ToBase64String(binarySP), FileName = "setup_packet.pdf" });

            // adds phone to contacts
            try {
                if (DatCompanyContactHelper.CheckPhone(setupPacketDto.ConfidantContact?.Phone)) {
                    var phone = DatCompanyContactHelper.OptimizePhone(setupPacketDto.ConfidantContact.Phone);
                    var contact = setupPacketDto.ConfidantContact.Contact;
                    if (DatCompanyContactHelper.CheckNewPhone(carrier.Contacts, phone) == false) {
                        carrier.Contacts = DatCompanyContactHelper.AddAndGetPhone(carrier.Contacts, phone, contact);
                    }
                }
            } catch (Exception e) {
                _l.e("DatCompanyContactHelper", e);
            }

            await setupPacketService.SendAsync(carrier, email);

            // saves files after emails
            Directory.CreateDirectory(destinDir);
            await File.WriteAllBytesAsync(destinFilePathW9, binaryW9);
            await File.WriteAllBytesAsync(destinFilePathSP, binarySP);
        }
        #endregion [ Setup Packet Email ]

        // sets finished
        setupPacketDto.Step = 14;
        setupPacketDto.EmailSent = DateTime.UtcNow;
        await setupPacketService.SaveAsync(setupPacketDto, carrier.CreatedById);
        await DatCompanies.SaveAsync(carrier, carrier.CreatedById);
    }

    public async Task<SetupPacketInviteSendResponseData> SetupPacketInviteSend(SetupPacketInviteSendRequestData request) {
        CheckEmails(request.Emails);

        DatCompany datCompany = await GetByIdAsync<DatCompany>(request.DatCompanyId);
        if (datCompany == null) {
            throw new InfoException("can not send email");
        }
        if (datCompany.DispatchCompanyId == null) {
            throw new InfoException("Company was not selected");
        }
        var company = await GetByIdAsync<DispatchCompany>(datCompany.DispatchCompanyId);

        var datCompnayDto = Mapper.Map<DatCompanyDto>(datCompany);
        var companyDto = Mapper.Map<ActualCompanyDto>(company);

        var email = await DispatchEmailGenerator.GenerateSetupPacketAsync(datCompnayDto, companyDto, request.Emails);
        await EmailUtils.SendEmailAsync(Smtp(company), email);

        if (datCompnayDto.SetupPacketEmailSent == null) {
            datCompnayDto.SetupPacketEmailSent = request.Emails;
        } else {
            datCompnayDto.SetupPacketEmailSent.AddRange(request.Emails);
            datCompnayDto.SetupPacketEmailSent = datCompnayDto.SetupPacketEmailSent.Distinct().ToList();
        }
        datCompnayDto.SetupPacketSent = true;

        // Sets Version
        if (datCompany.SetupPacketId != null) {
            SetupPacket setupPacket = await GetByIdAsync<SetupPacket>(datCompany.SetupPacketId);
            var version = AppSettings.SetupPacketVersion;
            if (setupPacket != null && setupPacket.Version != version) {
                setupPacket.Step = 4;
                setupPacket.Version = (byte)version;
                datCompnayDto.W9File = null;
                datCompnayDto.SetupPacketFile = null;
                datCompnayDto.SetupPacketAccept = false;
                await UpdateAsync(setupPacket);
                await this._saveEventSetupPacketAsync(setupPacket, request.UserId);
            }
            if (request.Resend) {
                datCompnayDto.SetupPacketCompleted = false;
            }
        }

        datCompany = await BaseSaveAsync<DatCompanyDto, DatCompany>(request.UserId, datCompnayDto);
        await this._saveEventDatCompanyAsync(datCompany, request.UserId);

        return new SetupPacketInviteSendResponseData() { DatCompany = datCompnayDto };
    }

    public async Task<SetupPacketEditSendResponseData> SetupPacketEditSend(SetupPacketEditSendRequestData request) {
        var datCompany = await GetByIdAsync<DatCompany>(request.DatCompanyId);
        if (datCompany == null) {
            throw new InfoException("can not send email");
        }
        if (datCompany.DispatchCompanyId == null) {
            throw new InfoException("Company was not selected");
        }
        var company = await GetByIdAsync<DispatchCompany>(datCompany.DispatchCompanyId);

        var datCompnayDto = Mapper.Map<DatCompanyDto>(datCompany);
        var companyDto = Mapper.Map<ActualCompanyDto>(company);

        var email = await DispatchEmailGenerator.GenerateEditSetupPacketAsync(datCompnayDto, companyDto, datCompnayDto.SetupPacketEmailSent);
        await EmailUtils.SendEmailAsync(Smtp(company), email);

        // Sets Version
        if (datCompany.SetupPacketId != null) {
            SetupPacket setupPacket = await GetByIdAsync<SetupPacket>(datCompany.SetupPacketId);
            setupPacket.Step = 6;
            if (setupPacket.WorkersCompensationInsuranceAgencyId != null) {
                await Table<SetupPacketInsuranceAgency>().Where(e => e.EntityId == setupPacket.WorkersCompensationInsuranceAgencyId)
                    .Set(e => e.Phone, (string)null)
                    .Set(e => e.CompanyName, (string)null)
                    .Set(e => e.Number, (string)null)
                    .Set(e => e.Email, (string)null)
                    .Set(e => e.Fax, (string)null)
                    .UpdateAsync();
            }
            await UpdateAsync(setupPacket);
            await this._saveEventSetupPacketAsync(setupPacket, request.UserId);

            datCompnayDto.SetupPacketCompleted = datCompany.SetupPacketCompleted = false;
            await this._saveEventDatCompanyAsync(datCompany, request.UserId);
        } else {
            throw new InfoException("Setup Packet was not found");
        }

        datCompany = await BaseSaveAsync<DatCompanyDto, DatCompany>(request.UserId, datCompnayDto);
        await this._saveEventDatCompanyAsync(datCompany, request.UserId);

        return new SetupPacketEditSendResponseData() { DatCompany = datCompnayDto };
    }

    #endregion [ setup packet ]

    #region [ Bind Loads ]

    public async Task<BindLoadsResponseData> BindLoads(BindLoadsRequestData request) {
        Guid? groupId = null;
        var loads = await Alive<Load>().Where(l => request.LoadIds.Contains(l.EntityId)).ToListAsync();
        int? historyId = null;
        switch (request.ParentType) {
            case LoadParentType.Warehouse: {
                    var warehouse = await GetByIdAsync<Warehouse>(request.ParentId);
                    historyId = warehouse.HistoryId;
                    foreach (var load in loads) {
                        load.WarehouseId = request.ParentId;
                        load.TrailerId = null;
                        load.DatCompanyId = null;
                        load.DatDriverId = null;
                        if (DisplayAttribute.GetOrder(LoadStatus.Assigned) > DisplayAttribute.GetOrder((LoadStatus)load.Status)) {
                            load.Status = (byte)LoadStatus.Assigned;
                        }
                        load.CurrentLatitude = warehouse.Latitude;
                        load.CurrentLongitude = warehouse.Longitude;
                    }
                    break;
                }
            case LoadParentType.Trailer: {
                    var trailer = await GetByIdAsync<Trailer>(request.ParentId);
                    var history = await Table<HistoryLocationTrailer>().Where(h => h.TrailerId == request.ParentId)
                        .OrderByDescending(h => h.Id).FirstOrDefaultAsync();
                    historyId = history?.Id;

                    var list = await Alive<Load>().Where(l => l.TrailerId == request.ParentId)
                        .Select(l => new { l.PickupOrder, l.DeliveryOrder }).ToListAsync();
                    byte maxOrder = 0;
                    if (list.Any()) {
                        maxOrder = list.Max(l => Math.Max(l.PickupOrder ?? 0, l.DeliveryOrder ?? 0));
                    }
                    foreach (var load in loads) {
                        load.WarehouseId = null;
                        load.TrailerId = request.ParentId;
                        if (load.HistoryAssignedTrailerId == null) {
                            load.HistoryAssignedTrailerId = request.ParentId;
                            load.AssignedTrailerDate = DateTime.UtcNow;
                        }
                        load.DatCompanyId = null;
                        load.DatDriverId = null;
                        load.PickupOrder = ++maxOrder;
                        load.DeliveryOrder = ++maxOrder;
                        if (DisplayAttribute.GetOrder(LoadStatus.Assigned) > DisplayAttribute.GetOrder((LoadStatus)load.Status)) {
                            load.Status = (byte)LoadStatus.Assigned;
                        }
                        load.CurrentLatitude = trailer.CurrentLatitude;
                        load.CurrentLongitude = trailer.CurrentLongitude;
                        // схема трейлера прикрепляется к грузу
                        load.TrailerSchemeDate = trailer.SchemeDate;
                        load.TrailerSchemeFile = trailer.SchemeFile;
                        load.TrailerSchemeId = trailer.EntityId;
                    }
                    break;
                }
            case LoadParentType.DatCompany: {
                    historyId = (await GetByIdAsync<DatCompany>(request.ParentId)).HistoryId;
                    foreach (var load in loads) {
                        load.WarehouseId = null;
                        load.TrailerId = null;
                        load.DatCompanyId = request.ParentId;
                        load.DatDriverId = null;
                        if (DisplayAttribute.GetOrder(LoadStatus.Assigned) > DisplayAttribute.GetOrder((LoadStatus)load.Status)) {
                            load.Status = (byte)LoadStatus.Assigned;
                        }
                    }
                    break;
                }
            case LoadParentType.DatDriver: {
                    var history = await Table<HistoryLocationDatDriver>().Where(h => h.DatDriverId == request.ParentId)
                        .OrderByDescending(h => h.Id).FirstOrDefaultAsync();
                    historyId = history?.Id;
                    foreach (var load in loads) {
                        load.WarehouseId = null;
                        load.TrailerId = null;
                        // load.DatCompanyId = null; оставляем текущий Id
                        load.DatDriverId = request.ParentId;
                    }
                    break;
                }
            case LoadParentType.Shipment: {
                    var carrierLoad = await GetByIdAsync<Load>(request.ParentId);
                    if (carrierLoad.ContainerType != (byte)ContainerType.Shipment) {
                        groupId = Guid.NewGuid();
                        carrierLoad.ContainerType = (byte)ContainerType.Shipment;
                        var dto = Mapper.Map<LoadDto>(carrierLoad);
                        Load entity = await this.BaseSaveAsync<LoadDto, Load>(request.UserId, dto, groupId);
                        await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, entity, entity.HistoryId, request.ParentType, request.UserId);
                    }
                    var history = await Table<HistoryLocationLoad>().Where(h => h.LoadId == request.ParentId)
                        .OrderByDescending(h => h.Id).FirstOrDefaultAsync();
                    historyId = history?.Id;

                    var list = await Alive<Load>().Where(l => l.ShipmentId == request.ParentId)
                        .Select(l => new { l.PickupOrder, l.DeliveryOrder }).ToListAsync();
                    byte maxOrder = 0;
                    if (list.Any()) {
                        maxOrder = list.Max(l => Math.Max(l.PickupOrder ?? 0, l.DeliveryOrder ?? 0));
                    }
                    foreach (var load in loads) {
                        load.WarehouseId = null;
                        load.ShipmentId = request.ParentId;
                        load.TrailerId = null;
                        load.DatCompanyId = null;
                        load.DatDriverId = null;
                        load.PickupOrder = ++maxOrder;
                        load.DeliveryOrder = ++maxOrder;
                        load.Status = (byte)LoadStatus.Assigned;
                    }
                    break;
                }
            case LoadParentType.None:
                foreach (var load in loads) {
                    load.WarehouseId = null;
                    load.TrailerId = null;
                    load.DatCompanyId = null;
                    load.DatDriverId = null;
                    // NOTE: Sets status as waiting for none
                    load.Status = (byte)LoadStatus.None;
                }
                break;
            default: throw new NotImplementedException();
        }
        foreach (var load in loads) {
            load.BindDate = DateTime.UtcNow;
            load.LastStatusChangedDate = DateTime.UtcNow;
        }

        var dtos = Mapper.Map<List<LoadDto>>(loads);
        // Saves of datas
        List<Load> entities = await this.BaseSavesAsync<LoadDto, Load>(request.UserId, dtos, groupId);
        await this._saveEventHistoriesLoadsAsync(EventTypeModel.BookedLoad,
            entities.ToDictionary(e => e, e => historyId), request.ParentType, request.UserId);
        return new BindLoadsResponseData() { Loads = this.Mapper.Map<List<LoadDto>>(entities) };
    }

    #endregion [ Bind Loads ]

    #region [ Invoice ]

    public async Task<InvoiceSendResponseData> InvoiceSend(InvoiceSendRequestData request) {
        try {
            var emails = request.Emails.ToList();
            if (request.FactoringCompanyEmail != null) {
                emails.Add(request.FactoringCompanyEmail);
            }
            CheckEmails(emails);

            var load = await GetByIdAsync<Load>(request.LoadId);
            var company = await GetByIdAsync<DispatchCompany>(load.DispatchCompanyId);

            // ## find broker ##
            DatCompany datCarrier = null;
            if (load.DatCompanyId != null) {
                datCarrier = await GetByIdAsync<DatCompany>(load.DatCompanyId);
            } else if (load.DatBrokerId != null) {
                datCarrier = await GetByIdAsync<DatCompany>(load.DatBrokerId);
            }

            var loadDto = Mapper.Map<LoadDto>(load);
            await loadDto.IncludeCompaniesAsync(Db);
            var companyDto = Mapper.Map<ActualCompanyDto>(company);
            var datBrokerDto = Mapper.Map<DatCompanyDto>(datCarrier);

            // Creates letter
            var email = await DispatchEmailGenerator.GenerateInvoiceAsync(datBrokerDto, loadDto, companyDto, emails,
                request.SendInvoice, request.SendPOD, request.SendConfirmation, request.SendLumper, request.Merged, request.Text);
            await EmailUtils.SendEmailAsync(Smtp(company), email);

            if (loadDto.EmailInvoiceSentDate == null) {
                loadDto.EmailInvoiceSentDate = DateTime.UtcNow;
            }
            // NOTES: CHANGES STATUS TO BILLED
            loadDto.Status = LoadStatus.Billed;
            if (loadDto.EmailInvoiceSent == null) {
                loadDto.EmailInvoiceSent = emails;
            } else {
                loadDto.EmailInvoiceSent.AddRange(emails);
                loadDto.EmailInvoiceSent = loadDto.EmailInvoiceSent.Distinct().ToList();
            }
            loadDto.ConfirmedDate = null;
            loadDto.ConfirmedFile = null;

            var groupId = Guid.NewGuid();
            load = await BaseSaveAsync<LoadDto, Load>(request.UserId, loadDto, groupId);
            await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, load, null, LoadParentType.None, request.UserId);

            if (loadDto.ContainerType != ContainerType.None) {
                await SaveChildrenAsync(loadDto, request.UserId, groupId);
            }

            return new InvoiceSendResponseData() { Load = loadDto };
        } catch (Exception e) {
            _l.e("InvoiceSend", e);
            throw;
        }
    }

    public async Task<InvoiceMergeResponseData> InvoiceMerge(InvoiceMergeRequestData request) {
        try {
            var load = await GetByIdAsync<Load>(request.LoadId);
            var loadDto = Mapper.Map<LoadDto>(load);
            var file = DispatchEmailGenerator.GenerateMergedInvoice(loadDto, request.SendInvoice, request.SendPOD, request.SendConfirmation, request.SendLumper);
            return new InvoiceMergeResponseData() { File = file };
        } catch (Exception e) {
            _l.e("InvoiceMerge", e);
            throw;
        }
    }

    #endregion [ Invoice ]

    #region [ Confirmation ]

    public async Task<ConfirmationSendResponseData> ConfirmationSend(ConfirmationSendRequestData request) {
        try {
            _l.i($"ConfirmationSend\n:{DispatchJson.Serialize(request)}");
            CheckEmails(request.Emails);

            var load = await GetByIdAsync<Load>(request.LoadId);
            var children = await Alive<Load>().Where(e => e.ParentId == load.EntityId).OrderBy(e => e.IndexExtra).ToListAsync();
            var company = await GetByIdAsync<DispatchCompany>(load.DispatchCompanyId);

            // ## find carrier ##
            if (load.DatCompanyId == null) {
                throw new InfoException("Undefined Carrier");
            }
            var carrier = await GetByIdAsync<DatCompany>(load.DatCompanyId);

            var loadDto = Mapper.Map<LoadDto>(load);
            var childrenDtos = Mapper.Map<List<LoadDto>>(children);
            await loadDto.IncludeCompaniesAsync(Db);
            await childrenDtos.IncludeCompaniesAsync(Db);
            var companyDto = Mapper.Map<ActualCompanyDto>(company);
            var carrierDto = Mapper.Map<DatCompanyDto>(carrier);

            carrierDto.Fields = await DatCompanies.GetFieldsAsync(carrierDto.DotNumber, carrierDto.McNumber);

            // Creates letter
            var email = await DispatchEmailGenerator.GenerateConfirmationAsync(Mapper, carrierDto, loadDto, childrenDtos, companyDto, request.Emails);
            await EmailUtils.SendEmailAsync(Smtp(company), email);

            loadDto.EmailConfirmationSentDate = DateTime.UtcNow;
            loadDto.EmailConfirmationSent = request.Emails;
            loadDto.ConfirmedDate = null;
            loadDto.ConfirmedFile = null;

            load = await BaseSaveAsync<LoadDto, Load>(request.UserId, loadDto);
            await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, load, null, LoadParentType.None, request.UserId);

            return new ConfirmationSendResponseData() { Load = loadDto };
        } catch (Exception e) {
            _l.e("ConfirmationSend", e);
            throw;
        }
    }

    /// <summary>
    ///     Карриер подписал confirmation (страница /Confirmation): подпись агента — AdUser.Signature </summary>
    public async Task<ConfirmedSendResponseData> ConfirmedSend(ConfirmedSendRequestData request) {
        var load = await GetByIdAsync<Load>(request.LoadId);
        var company = await GetByIdAsync<DispatchCompany>(load.DispatchCompanyId);
        var agentSignature = await Table<AdUser>().Where(e => e.Id == load.CreatedById).Select(e => e.Signature).FirstOrDefaultAsync();

        // ## find carrier ##
        if (load.DatCompanyId == null) {
            throw new InfoException("Undefined Carrier");
        }
        var carrier = await GetByIdAsync<DatCompany>(load.DatCompanyId);

        var loadDto = Mapper.Map<LoadDto>(load);
        var companyDto = Mapper.Map<ActualCompanyDto>(company);
        var carrierDto = Mapper.Map<DatCompanyDto>(carrier);

        if (load.ConfirmationFile == null) {
            throw new InfoException("Confirmation doesn't exists");
        }

        loadDto.ConfirmedDate = load.ConfirmedDate = DateTime.UtcNow;
        loadDto.ConfirmedFile = DispatchPdfGenerator.SignConfirmation(loadDto, request.Signature, agentSignature);

        if (String.IsNullOrEmpty(load.EmailConfirmationSent) == false) {
            var email = await DispatchEmailGenerator.GenerateConfirmedAsync(carrierDto, loadDto, companyDto, request.Emails);
            await EmailUtils.SendEmailAsync(Smtp(company), email);
        } else {
            _l.w($"No email is specified. LoadId: '{loadDto.EntityId}'");
        }

        load = await BaseSaveAsync<LoadDto, Load>(load.CreatedById, loadDto);
        await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, load, null, LoadParentType.None, load.CreatedById);

        return new ConfirmedSendResponseData() { Load = loadDto };
    }

    #endregion [ Confirmation ]

    #region [ SMS ]

    public async Task<SendSmsResponseData> SendSms(SendSmsRequestData request) {
        var company = await GetByIdAsync<DispatchCompany>(request.ActualCompanyId);
        var result = await Host.GetService<SmsWebService>().SendSmsAsync(company.SMS_Phone, request.ToPhone, request.Message);
        if (result.Success) {
            return new SendSmsResponseData() { Json = result.Data };
        }
        throw new InfoException(result.ErrorMessage);
    }

    /// <summary>
    ///     LoadSendSmsJob V1: SMS складу о грузах с SendSms </summary>
    public async Task LoadSendSms_Process() {
        var sms = Host.GetService<SmsWebService>();
        var loads = await Alive<Load>().Where(e => e.SendSms).ToListAsync();
        foreach (var load in loads) {
            var company = await GetByIdAsync<DispatchCompany>(load.DispatchCompanyId);
            await Table<Load>().Where(e => e.EntityId == load.EntityId).Set(e => e.SendSms, false).UpdateAsync();
            var message = $"Pick {load.PickupFrom:d h:mm tt} {load.OriginCity} to {load.DestinationCity} check AutoCall for more details";
            await sms.SendSmsAsync(company?.SMS_Phone, AppSettings.LoadSendSmsPhone, message);
        }
    }

    #endregion [ SMS ]

    #region [ Scheme Email ]

    public async Task<SendSchemeEmailResponseData> SendSchemeEmail(SendSchemeEmailRequestData request) {
        var company = await GetByIdAsync<DispatchCompany>(request.ActualCompanyId);
        var companyDto = Mapper.Map<ActualCompanyDto>(company);
        var emails = request.Emails.Split(',', '\n').Select(e => e.Trim()).Where(e => String.IsNullOrEmpty(e) == false).ToList();
        if (emails.Any() == false) throw new InfoException("email error");
        var smtpSettings = new SmtpSettings(company.SMTP_SchemeEmail, company.SMTP_SchemePassword, company.SMTP_SchemeHost, company.SMTP_SchemePort);
        foreach (var email in emails) {
            var emailObj = await DispatchEmailGenerator.GenerateSchemeEmailAsync(email, request.TrailerNumber, request.Data, companyDto);
            await EmailUtils.SendEmailAsync(smtpSettings, emailObj);
        }
        return new SendSchemeEmailResponseData() { Result = true };
    }

    #endregion [ Scheme Email ]
}
