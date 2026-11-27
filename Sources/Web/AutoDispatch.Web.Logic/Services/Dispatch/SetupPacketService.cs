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
using AutoDispatch.Web.Logic.Mapping.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Setup packet карриера (V1 SetupPacketService): контакты, страховые агентства и references — отдельные таблицы,
///     сохраняются здесь; само сохранение setup packet с событием — DispatchService.SaveSetupPacket </summary>
public class SetupPacketService : DispatchCrudService<SetupPacketDto, SetupPacket> {

    public SetupPacketService(ServicesHost host) : base(host) { }

    protected override SetupPacketDto ToDto(SetupPacket model) => SetupPacketConverter.ToDto(model, Mapper);

    protected override SetupPacket ToModel(SetupPacketDto dto) => SetupPacketConverter.ToModel(dto);

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
            // компания карриера, а не создателя: регистрация сохраняет от CreatedById, которого могло не быть в переносе (AI/wiki/app-testing.md 6.20)
            CompanyId = carrier.CompanyId,
            Version = (byte)AppSettings.SetupPacketVersion,
            FileMC = certificate.Success ? certificate.Data.FileName : null,
            PhysicalCountryId = 0,
            MailingCountryId = 0
        };

        // карточка FMCSA недоступна (Safer 502, карриер не найден) — packet без предзаполнения, карриер вводит данные на шаге 2 сам;
        // в V1 исключение прерывало регистрацию (AI/wiki/public-site.md)
        Dictionary<string, string> fields;
        try {
            fields = await datCompanyService.GetFieldsAsync(carrier.DotNumber, carrier.McNumber) ?? new Dictionary<string, string>();
        } catch (Exception e) {
            _l.w($"Create setup packet: FMCSA fields for {carrier.Name} (MC {carrier.McNumber}, DOT {carrier.DotNumber}) unavailable: {e.Message}");
            fields = new Dictionary<string, string>();
        }

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
            if (dto.CompanyId == Guid.Empty) {
                dto.CompanyId = await GetCompanyIdByUserIdAsync(userId);
            }
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
    ///     Файл MC или COI шага 13 (V1 Step13_ViewModelToDto): в каталог карриера под именем MC_yyyy_MM_dd.ext / COI_…,
    ///     занятое имя — с номером; старый файл с тем же именем удаляется. Возвращает имя для SetupPacketDto.FileMC/FileCOI </summary>
    public async Task<string> SaveStepFileAsync(Guid carrierId, string prefix, string fileName, Stream content, string currentName) {
        var directory = DispatchFileHelper.FileGetFullPath(carrierId);
        Directory.CreateDirectory(directory);
        var name = $"{prefix}_{DateTime.Now:yyyy_MM_dd}{Path.GetExtension(fileName)}";
        if (name == currentName && File.Exists(Path.Combine(directory, name))) {
            File.Delete(Path.Combine(directory, name));
        }
        var fullPath = DispatchFileHelper.UniqueFileFullPath(Path.Combine(directory, name));
        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file);
        return Path.GetFileName(fullPath);
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
        return SetupPacketConverter.ToDto(store, Mapper);
    }

    public override async Task<List<SetupPacketDto>> GetAllAsync() {
        var stores = await Alive<SetupPacket>().ToListAsync();
        await LoadRelationsAsync(stores);
        return stores.Select(e => SetupPacketConverter.ToDto(e, Mapper)).ToList();
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
