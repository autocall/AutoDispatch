using System.Text;
using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Компании dispatch — ActualCompany V1 (V1 ActualCompanyService): контракт ActualCompanyDto, таблица DispatchCompany.
///     Шаблоны confirmation/BOL/invoice (RTF) — файлы в каталоге компании через DispatchService.FileSave </summary>
public class DispatchCompanyService : DispatchCrudService<ActualCompanyDto, DispatchCompany> {

    public DispatchCompanyService(ServicesHost host) : base(host) { }

    // Data doesn't use in Web-Page in Grid
    private static ActualCompanyDto SetEmptyData(ActualCompanyDto dto) {
        dto.ConfirmationData = null;
        dto.BOLData = null;
        dto.InvoiceData = null;
        return dto;
    }

    /// <summary>
    ///     Список без шаблонов и большого логотипа; companyId — только компании арендатора </summary>
    public async Task<List<ActualCompanyDto>> GetAllAsync(Guid? companyId) {
        var query = Alive<DispatchCompany>();
        if (companyId != null) {
            query = query.Where(e => e.CompanyId == companyId);
        }
        var dtos = Mapper.Map<List<ActualCompanyDto>>(await query.ToListAsync());
        foreach (var dto in dtos) {
            SetEmptyData(dto);
            dto.LogoBig = null;
        }
        return dtos;
    }

    public override Task<List<ActualCompanyDto>> GetAllAsync() => GetAllAsync(null);

    /// <summary>
    ///     С кастомизациями нумерации (Customizes) и без шаблонов </summary>
    public override async Task<ActualCompanyDto> GetByIdAsync(Guid id) {
        var store = await GetByIdAsync<DispatchCompany>(id);
        if (store == null) {
            return null;
        }
        store.Customizes = await Alive<DispatchCompanyCustom>().Where(e => e.DispatchCompanyId == id).ToListAsync();
        return SetEmptyData(Mapper.Map<ActualCompanyDto>(store));
    }

    /// <summary>
    ///     Полная карточка с шаблонами (ConfirmationData/BOLData/InvoiceData) для писем и печати </summary>
    public async Task<ActualCompanyDto> GetWithDataAsync(Guid id) {
        var store = await GetByIdAsync<DispatchCompany>(id);
        return store == null ? null : Mapper.Map<ActualCompanyDto>(store);
    }

    public async Task<List<ActualCompanyDto>> SearchFor2Async(System.Linq.Expressions.Expression<Func<DispatchCompany, bool>> predicate) {
        var stores = await Alive<DispatchCompany>().Where(predicate).ToListAsync();
        var ids = stores.Select(e => e.EntityId).ToList();
        var customizes = await Alive<DispatchCompanyCustom>().Where(e => ids.Contains(e.DispatchCompanyId)).ToListAsync();
        foreach (var store in stores) {
            store.Customizes = customizes.Where(e => e.DispatchCompanyId == store.EntityId).ToList();
        }
        return Mapper.Map<List<ActualCompanyDto>>(stores);
    }

    protected override void OnUpdate(DispatchCompany old, DispatchCompany @new) {
        @new.SMTP_Password = @new.SMTP_Password ?? old.SMTP_Password; // NOTE: sets if changed
        @new.SMTP_SchemePassword = @new.SMTP_SchemePassword ?? old.SMTP_SchemePassword;
        @new.BnsfPassword = @new.BnsfPassword ?? old.BnsfPassword; // NOTE: sets if changed
        @new.BnsfCookies = @new.BnsfCookies ?? old.BnsfCookies; // NOTE: sets if changed
    }

    public override async Task SaveAsync(ActualCompanyDto dto, Guid userId) {
        await base.SaveAsync(dto, userId);

        #region [ files ]
        var dispatchService = Host.GetService<DispatchService>();

        if (dto.ConfirmationData != null) {
            dispatchService.FileDelete(new FileDeleteRequestData(userId) { File = new FileDto() { EntityId = dto.EntityId, Name = dto.ConfirmationFile } });
            await dispatchService.FileSave(new FileSaveRequestData(userId) {
                File = new FileDto() { EntityId = dto.EntityId, Data = Encoding.ASCII.GetBytes(dto.ConfirmationData), Name = dto.ConfirmationFile }
            });
        }
        if (dto.BOLData != null) {
            dispatchService.FileDelete(new FileDeleteRequestData(userId) { File = new FileDto() { EntityId = dto.EntityId, Name = dto.BOLFile } });
            await dispatchService.FileSave(new FileSaveRequestData(userId) {
                File = new FileDto() { EntityId = dto.EntityId, Data = Encoding.ASCII.GetBytes(dto.BOLData), Name = dto.BOLFile }
            });
        }
        if (dto.InvoiceData != null) {
            dispatchService.FileDelete(new FileDeleteRequestData(userId) { File = new FileDto() { EntityId = dto.EntityId, Name = dto.InvoiceFile } });
            await dispatchService.FileSave(new FileSaveRequestData(userId) {
                File = new FileDto() { EntityId = dto.EntityId, Data = Encoding.ASCII.GetBytes(dto.InvoiceData), Name = dto.InvoiceFile }
            });
        }
        #endregion [ files ]

        #region [ customizes ]
        if (dto.Customizes != null) {
            // Adds / Updates customizes
            foreach (var dto2 in dto.Customizes) {
                var model = await GetByIdAsync<DispatchCompanyCustom>(dto2.EntityId);
                if (model == null) {
                    model = Mapper.Map<DispatchCompanyCustom>(dto2);
                    if (model.EntityId == Guid.Empty) {
                        dto2.EntityId = model.EntityId = Guid.NewGuid();
                    }
                    model.DispatchCompanyId = dto.EntityId;
                    model.CreatedById = model.LastModifiedById = userId;
                    model.CreatedDate = model.LastModifiedDate = DateTime.UtcNow;
                    await InsertAsync(model);
                } else {
                    var newModel = Mapper.Map<DispatchCompanyCustom>(dto2);
                    newModel.CreatedById = model.CreatedById;
                    newModel.CreatedDate = model.CreatedDate;
                    newModel.LastModifiedById = userId;
                    newModel.LastModifiedDate = DateTime.UtcNow;
                    await UpdateAsync(newModel);
                }
            }
        } else {
            // removes
            await Table<DispatchCompanyCustom>().Where(e => e.DispatchCompanyId == dto.EntityId).DeleteAsync();
        }
        #endregion [ customizes ]
    }

    public async Task<DateTime?> GetLastDoneAsync() {
        var company = await Alive<DispatchCompany>().Where(e => e.BnsfLastDone != null).OrderByDescending(e => e.BnsfLastDone).FirstOrDefaultAsync();
        return company?.BnsfLastDone;
    }
}
