using System.Text.RegularExpressions;
using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Email;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.WebServices;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Setup packet карриера (V1 SetupPacketService): контакты, страховые агентства и references — отдельные таблицы,
///     сохраняются здесь; само сохранение setup packet с событием — DispatchService.SaveSetupPacket </summary>
public class SetupPacketService : DispatchCrudService<SetupPacketDto, SetupPacket> {

    public SetupPacketService(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Get all Factoring Companies </summary>
    public async Task<List<SetupPacketFactoringCompanyDto>> GetFactoringCompaniesAsync() {
        var list = await Table<SetupPacketFactoringCompany>().ToListAsync();
        return Mapper.Map<List<SetupPacketFactoringCompanyDto>>(list);
    }

    private static GroupCollection GetFirstMatch(string input, string pattern) {
        var matches = new Regex(pattern).Matches(input);
        return matches.Count != 0 ? matches[0].Groups : null;
    }

    /// <summary>
    ///     Новый setup packet по карточке FMCSA карриера; сертификат MC сохраняется в файлы карриера </summary>
    public async Task<SetupPacketDto> CreateSetupPacketAsync(DatCompanyDto carrier) {
        var datCompanyService = Host.GetService<DatCompanyService>();
        var dispatchService = Host.GetService<DispatchService>();

        var certificate = await Host.GetService<BrokersnapshotWebService>().DownloadCertificateAsync("MC", carrier.McNumber, carrier.DotNumber);
        if (certificate.Success) {
            await dispatchService.FileSave(new FileSaveRequestData(AdUser.SystemUserId) {
                File = new FileDto() {
                    Data = certificate.Data.Data,
                    Name = certificate.Data.FileName,
                    EntityId = carrier.EntityId,
                }
            });
        }

        var setupPacket = new SetupPacketDto() {
            Version = (byte)AppSettings.SetupPacketVersion,
            FileMC = certificate.Success ? certificate.Data.FileName : null,
            PhysicalCountryId = 0,
            MailingCountryId = 0
        };

        Dictionary<string, string> fields = await datCompanyService.GetFieldsAsync(carrier.DotNumber, carrier.McNumber);

        if (fields.ContainsKey("Legal Name:")) {
            setupPacket.W9Name = setupPacket.LegalName = fields["Legal Name:"];
            setupPacket.W9BusinessName = setupPacket.DBAName = fields["DBA Name:"];
            setupPacket.MailingCompanyLegalName = fields["Legal Name:"];
        }

        // Input : LOS ANGELES, CA   90025-2156
        const string pattern = @"^(?<city>[a-zA-Z0-9\s]+)[,][\s]+(?<state>[A-Z]{2})[\s]+(?<zip>[0-9\s-]{1,})$";
        if (fields.ContainsKey("Physical Address:")) {
            var arr = fields["Physical Address:"].Split('\n');
            setupPacket.W9Address = setupPacket.PhysicalAddress = arr.First().Trim();
            var result = GetFirstMatch(arr.Last().Trim(), pattern);
            if (result != null) {
                string city = result["city"].Value;
                string state = result["state"].Value;
                string zip = result["zip"].Value;
                var stateDto = StateHelper.GetStateByCode(state);
                if (stateDto != null) {
                    setupPacket.W9City = setupPacket.PhysicalCity = city.Trim();
                    setupPacket.W9StateId = setupPacket.PhysicalStateId = stateDto.Id;
                } else {
                    setupPacket.PhysicalCity = $"{city}, {state}";
                }
                setupPacket.W9ZipCode = setupPacket.PhysicalZipCode = zip.Trim();
            } else {
                setupPacket.PhysicalCity = arr.Last().Trim();
            }
        }
        if (fields.ContainsKey("Mailing Address:")) {
            var arr = fields["Mailing Address:"].Split('\n');
            setupPacket.MailingAddress = arr.First().Trim();
            var result = GetFirstMatch(arr.Last().Trim(), pattern);
            if (result != null) {
                string city = result["city"].Value;
                string state = result["state"].Value;
                string zip = result["zip"].Value;
                var stateDto = StateHelper.GetStateByCode(state);
                if (stateDto != null) {
                    setupPacket.MailingCity = city.Trim();
                    setupPacket.MailingStateId = stateDto.Id;
                } else {
                    setupPacket.MailingCity = $"{city}, {state}";
                }
                setupPacket.MailingZipCode = zip.Trim();
            } else {
                setupPacket.MailingCity = arr.Last().Trim();
            }
        }

        return setupPacket;
    }

    /// <summary>
    ///     Save model to database: контакты и агентства — upsert, лишние — удалить, затем setup packet с событием (DispatchService) </summary>
    public override async Task SaveAsync(SetupPacketDto dto, Guid userId) {
        var dispatchService = Host.GetService<DispatchService>();
        var model = await GetByIdAsync<SetupPacket>(dto.EntityId);

        await CleanContactAsync(dto.ConfidantContact);
        await CleanContactAsync(dto.CorporateContact);
        await CleanContactAsync(dto.SalesContact);
        await CleanContactAsync(dto.DispatchContact);
        await CleanContactAsync(dto.AfterHourContact);

        await CleanInsuranceAgencyAsync(dto.GeneralLiabilityInsuranceAgency);
        await CleanInsuranceAgencyAsync(dto.AutoInsuranceAgency);
        await CleanInsuranceAgencyAsync(dto.CargoInsuranceAgency);
        await CleanInsuranceAgencyAsync(dto.WorkersCompensationInsuranceAgency);

        if (model == null) {
            if (dto.EntityId == Guid.Empty) {
                dto.EntityId = Guid.NewGuid();
            }
            dto.CompanyId = await GetCompanyIdByUserIdAsync(userId);
            dto.CreatedById = userId;
            dto.CreatedDate = DateTime.UtcNow;
        } else {
            await CleanContactAsync(model.ConfidantContactId, dto.ConfidantContact);
            await CleanContactAsync(model.CorporateContactId, dto.CorporateContact);
            await CleanContactAsync(model.SalesContactId, dto.SalesContact);
            await CleanContactAsync(model.DispatchContactId, dto.DispatchContact);
            await CleanContactAsync(model.AfterHourContactId, dto.AfterHourContact);

            await CleanInsuranceAgencyAsync(model.GeneralLiabilityInsuranceAgencyId, dto.GeneralLiabilityInsuranceAgency);
            await CleanInsuranceAgencyAsync(model.AutoInsuranceAgencyId, dto.AutoInsuranceAgency);
            await CleanInsuranceAgencyAsync(model.CargoInsuranceAgencyId, dto.CargoInsuranceAgency);
            await CleanInsuranceAgencyAsync(model.WorkersCompensationInsuranceAgencyId, dto.WorkersCompensationInsuranceAgency);

            await SaveReferencesAsync(model.EntityId, dto.References ?? new List<SetupPacketReferenceDto>());
            dto.CompanyId = model.CompanyId;
        }

        // saves events (и саму запись — SaveWithHistory)
        await dispatchService.SaveSetupPacket(new SaveSetupPacketRequestData(userId) { SetupPacket = dto });
    }

    /// <summary>
    ///     Сохранение без события и истории (шаги публичной формы) </summary>
    public async Task OnlySaveAsync(SetupPacketDto dto, Guid userId) {
        var model = await GetByIdAsync<SetupPacket>(dto.EntityId);
        if (model != null) {
            var newModel = Mapper.Map<SetupPacket>(dto);
            newModel.CreatedById = model.CreatedById;
            newModel.CreatedDate = model.CreatedDate;
            newModel.LastModifiedById = userId;
            newModel.LastModifiedDate = DateTime.UtcNow;
            await UpdateAsync(newModel);
        }
    }

    private async Task SaveReferencesAsync(Guid setupPacketId, List<SetupPacketReferenceDto> dtos) {
        var models = await Table<SetupPacketReference>().Where(e => e.SetupPacketId == setupPacketId).ToListAsync();
        // Adds
        foreach (var dto in dtos) {
            if (models.Any(r => r.EntityId == dto.EntityId) == false) {
                var newModel = Mapper.Map<SetupPacketReference>(dto);
                newModel.SetupPacketId = setupPacketId;
                newModel.CreatedDate = DateTime.UtcNow;
                await InsertAsync(newModel);
            }
        }
        // Deletes
        foreach (var model in models) {
            if (dtos.Any(r => r.EntityId == model.EntityId) == false) {
                await DeleteAsync(model);
            }
        }
    }

    /// <summary>
    ///     Inserts or Updates to db if new </summary>
    private async Task CleanContactAsync(SetupPacketContactDto dto) {
        if (dto == null || dto.HasContact == false) return;
        var model = await Table<SetupPacketContact>().FirstOrDefaultAsync(e => e.EntityId == dto.EntityId);
        var newModel = Mapper.Map<SetupPacketContact>(dto);
        if (model == null) {
            await InsertAsync(newModel);
        } else {
            await UpdateAsync(newModel);
        }
    }

    /// <summary>
    ///     Deletes if doesn't exist </summary>
    private async Task CleanContactAsync(Guid? modelId, SetupPacketContactDto dto) {
        if (modelId != null && (dto == null || dto.EntityId != modelId)) {
            await Table<SetupPacketContact>().Where(e => e.EntityId == modelId).DeleteAsync();
        }
    }

    private async Task CleanInsuranceAgencyAsync(SetupPacketInsuranceAgencyDto dto) {
        if (dto == null || dto.HasInsurance == false) return;
        var model = await Table<SetupPacketInsuranceAgency>().FirstOrDefaultAsync(e => e.EntityId == dto.EntityId);
        var newModel = Mapper.Map<SetupPacketInsuranceAgency>(dto);
        if (model == null) {
            await InsertAsync(newModel);
        } else {
            await UpdateAsync(newModel);
        }
    }

    private async Task CleanInsuranceAgencyAsync(Guid? modelId, SetupPacketInsuranceAgencyDto dto) {
        if (modelId != null && (dto == null || dto.EntityId != modelId)) {
            await Table<SetupPacketInsuranceAgency>().Where(e => e.EntityId == modelId).DeleteAsync();
        }
    }

    /// <summary>
    ///     Контакты, агентства, references setup packet (EF6 ReferenceLoad/CollectionLoad V1) </summary>
    public async Task LoadRelationsAsync(IList<SetupPacket> packets) {
        if (packets.Count == 0) return;
        var contactIds = packets.SelectMany(p => new[] { p.ConfidantContactId, p.CorporateContactId, p.SalesContactId, p.DispatchContactId, p.AfterHourContactId })
            .Where(e => e != null).Select(e => e.Value).Distinct().ToList();
        var agencyIds = packets.SelectMany(p => new[] { p.GeneralLiabilityInsuranceAgencyId, p.AutoInsuranceAgencyId, p.CargoInsuranceAgencyId, p.WorkersCompensationInsuranceAgencyId })
            .Where(e => e != null).Select(e => e.Value).Distinct().ToList();
        var packetIds = packets.Select(p => p.EntityId).ToList();
        var contacts = contactIds.Any() ? (await Table<SetupPacketContact>().Where(e => contactIds.Contains(e.EntityId)).ToListAsync()).ToDictionary(e => e.EntityId) : new Dictionary<Guid, SetupPacketContact>();
        var agencies = agencyIds.Any() ? (await Table<SetupPacketInsuranceAgency>().Where(e => agencyIds.Contains(e.EntityId)).ToListAsync()).ToDictionary(e => e.EntityId) : new Dictionary<Guid, SetupPacketInsuranceAgency>();
        var references = await Table<SetupPacketReference>().Where(e => packetIds.Contains(e.SetupPacketId)).ToListAsync();
        foreach (var p in packets) {
            p.ConfidantContact = Get(contacts, p.ConfidantContactId);
            p.CorporateContact = Get(contacts, p.CorporateContactId);
            p.SalesContact = Get(contacts, p.SalesContactId);
            p.DispatchContact = Get(contacts, p.DispatchContactId);
            p.AfterHourContact = Get(contacts, p.AfterHourContactId);
            p.GeneralLiabilityInsuranceAgency = Get(agencies, p.GeneralLiabilityInsuranceAgencyId);
            p.AutoInsuranceAgency = Get(agencies, p.AutoInsuranceAgencyId);
            p.CargoInsuranceAgency = Get(agencies, p.CargoInsuranceAgencyId);
            p.WorkersCompensationInsuranceAgency = Get(agencies, p.WorkersCompensationInsuranceAgencyId);
            p.References = references.Where(e => e.SetupPacketId == p.EntityId).ToList();
        }
    }

    private static T Get<T>(Dictionary<Guid, T> dic, Guid? id) where T : class {
        return id != null && dic.TryGetValue(id.Value, out var value) ? value : null;
    }

    public override async Task<SetupPacketDto> GetByIdAsync(Guid id) {
        var store = await GetByIdAsync<SetupPacket>(id);
        if (store == null) return null;
        await LoadRelationsAsync(new[] { store });
        return Mapper.Map<SetupPacketDto>(store);
    }

    public override async Task<List<SetupPacketDto>> GetAllAsync() {
        var stores = await Alive<SetupPacket>().ToListAsync();
        await LoadRelationsAsync(stores);
        return Mapper.Map<List<SetupPacketDto>>(stores);
    }

    /// <summary>
    ///     Письмо карриеру с SMTP его компании и отметка SetupPacketCompleted </summary>
    public async Task SendAsync(DatCompanyDto carrier, Email.Email email) {
        var company = await GetByIdAsync<DispatchCompany>(carrier.ActualCompanyId);
        var smtpSettings = new SmtpSettings(company.SMTP_Email, company.SMTP_Password, company.SMTP_Host, company.SMTP_Port);
        await EmailUtils.SendEmailAsync(smtpSettings, email);
        carrier.SetupPacketCompleted = true;
        await Host.GetService<DatCompanyService>().SaveAsync(carrier, carrier.CreatedById);
    }
}
