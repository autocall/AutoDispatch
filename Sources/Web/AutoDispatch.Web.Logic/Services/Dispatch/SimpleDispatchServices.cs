using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Груз, карриер, компания для страницы /Confirmation (V1 ConfirmationService) </summary>
public class ConfirmationService : DispatchBaseService {

    public ConfirmationService(ServicesHost host) : base(host) { }

    public async Task<LoadDto> GetLoadByIdAsync(Guid loadId) {
        return Mapper.Map<LoadDto>(await GetByIdAsync<Load>(loadId));
    }

    public async Task<DatCompanyDto> GetDatCompanyByIdAsync(Guid datCompanyId) {
        return Mapper.Map<DatCompanyDto>(await GetByIdAsync<DatCompany>(datCompanyId));
    }

    public async Task<ActualCompanyDto> GetActualCompanyByIdAsync(Guid actualCompanyId) {
        return Mapper.Map<ActualCompanyDto>(await GetByIdAsync<DispatchCompany>(actualCompanyId));
    }
}

/// <summary>
///     Свои статусы грузов компании (V1 CustomStatusService): CompanyId — из пользователя </summary>
public class CustomStatusService : DispatchBaseService {

    public CustomStatusService(ServicesHost host) : base(host) { }

    public async Task<List<CompanyCustomStatusDto>> GetStatusesAsync(Guid userId) {
        var companyId = await GetCompanyIdByUserIdAsync(userId);
        var models = await Alive<CompanyCustomStatus>().Where(e => e.CompanyId == companyId).OrderBy(e => e.CreatedDate).ToListAsync();
        return Mapper.Map<List<CompanyCustomStatusDto>>(models);
    }

    public async Task<CompanyCustomStatusDto> GetStatusAsync(Guid entityId, Guid userId) {
        var companyId = await GetCompanyIdByUserIdAsync(userId);
        var model = await GetByIdAsync<CompanyCustomStatus>(entityId);
        if (model == null) {
            throw new InfoException("Model was not found");
        }
        if (model.CompanyId != companyId) {
            throw new AccessDeniedException("Conflict Company");
        }
        return Mapper.Map<CompanyCustomStatusDto>(model);
    }

    public async Task<CompanyCustomStatusDto> SaveStatusAsync(CompanyCustomStatusDto dto, Guid userId) {
        var companyId = await GetCompanyIdByUserIdAsync(userId);
        if (dto.EntityId != Guid.Empty) {
            // Update
            var oldModel = await GetByIdAsync<CompanyCustomStatus>(dto.EntityId);
            if (oldModel == null) {
                throw new InfoException("Model was not found");
            }
            if (oldModel.CompanyId != companyId) {
                throw new AccessDeniedException("Conflict Company");
            }
            var newModel = Mapper.Map<CompanyCustomStatus>(dto);
            newModel.LastModifiedById = userId;
            newModel.LastModifiedDate = DateTime.UtcNow;
            newModel.CreatedById = oldModel.CreatedById;
            newModel.CreatedDate = oldModel.CreatedDate;
            newModel.CompanyId = companyId;
            await UpdateAsync(newModel);
            return Mapper.Map<CompanyCustomStatusDto>(newModel);
        } else {
            // Insert
            var newModel = Mapper.Map<CompanyCustomStatus>(dto);
            newModel.EntityId = Guid.NewGuid();
            newModel.LastModifiedById = newModel.CreatedById = userId;
            newModel.LastModifiedDate = newModel.CreatedDate = DateTime.UtcNow;
            newModel.CompanyId = companyId;
            await InsertAsync(newModel);
            return Mapper.Map<CompanyCustomStatusDto>(newModel);
        }
    }

    public async Task DeleteStatusAsync(Guid entityId, Guid userId) {
        var companyId = await GetCompanyIdByUserIdAsync(userId);
        var model = await GetByIdAsync<CompanyCustomStatus>(entityId);
        if (model == null) {
            throw new InfoException("Model was not found");
        }
        if (model.CompanyId != companyId) {
            throw new AccessDeniedException("Conflict Company");
        }
        await DeleteAsync(model);
    }
}

/// <summary>
///     Заказчики брокерской доски (V1 CustomerCompanyService) </summary>
public class CustomerCompanyService : DispatchCrudService<CustomerCompanyDto, CustomerCompany> {

    public CustomerCompanyService(ServicesHost host) : base(host) { }

    public async Task<List<CustomerCompanyDto>> SearchCustomersAsync(Guid userId, string text) {
        var companyId = await GetCompanyIdByUserIdAsync(userId);
        var dtos = await SearchForAsync(c => c.CompanyId == companyId && c.Name.Contains(text));
        return dtos.OrderBy(c => c.Name).ToList();
    }
}

/// <summary>
///     Грузоотправители и получатели (V1 ShipperReceiverCompanyService) </summary>
public class ShipperReceiverCompanyService : DispatchCrudService<ShipperReceiverCompanyDto, ShipperReceiverCompany> {

    public ShipperReceiverCompanyService(ServicesHost host) : base(host) { }
}

/// <summary>
///     Справочник факторинговых компаний setup packet (V1 FactoringCompanyService): int-ключ, штат — StateHelper (таблицы State нет) </summary>
public class FactoringCompanyService : DispatchBaseService {

    public FactoringCompanyService(ServicesHost host) : base(host) { }

    public async Task<Tuple<List<SetupPacketFactoringCompanyDto>, int>> LookupAsync(
        int pageIndex, int pageSize, string searchValue, string orderDir, int? orderBy) {
        var models = await Table<SetupPacketFactoringCompany>().ToListAsync();
        IEnumerable<SetupPacketFactoringCompany> query = models;
        if (searchValue != null) {
            query = query.Where(e =>
                Contains(e.Name, searchValue) ||
                Contains(e.Address, searchValue) ||
                Contains(e.City, searchValue) ||
                Contains(StateName(e.StateId), searchValue) ||
                Contains(StateCode(e.StateId), searchValue) ||
                Contains(e.ZipCode, searchValue) ||
                Contains(e.Email, searchValue));
        }
        var total = query.Count();
        if (orderBy != null) {
            Func<SetupPacketFactoringCompany, string> key = orderBy switch {
                0 => l => l.Name,
                1 => l => l.Address,
                2 => l => l.City,
                3 => l => StateCode(l.StateId),
                4 => l => l.ZipCode,
                5 => l => l.Email,
                _ => l => l.Name
            };
            query = orderDir == "asc" ? query.OrderBy(key) : query.OrderByDescending(key);
            query = query.Skip(pageIndex);
            if (pageSize > 0) {
                query = query.Take(pageSize);
            }
        }
        var dtos = Mapper.Map<List<SetupPacketFactoringCompanyDto>>(query.ToList());
        return new Tuple<List<SetupPacketFactoringCompanyDto>, int>(dtos, total);
    }

    private static bool Contains(string value, string search) => value != null && value.Contains(search, StringComparison.OrdinalIgnoreCase);
    private static string StateName(byte id) => Helpers.Dispatch.StateHelper.GetState(id)?.Name;
    private static string StateCode(byte id) => Helpers.Dispatch.StateHelper.GetState(id)?.Code;

    public async Task<SetupPacketFactoringCompanyDto> GetByIdAsync(int id) {
        var model = await Table<SetupPacketFactoringCompany>().FirstOrDefaultAsync(e => e.EntityId == id);
        return Mapper.Map<SetupPacketFactoringCompanyDto>(model);
    }

    public async Task<SetupPacketFactoringCompanyDto> SaveAsync(SetupPacketFactoringCompanyDto dto) {
        var model = await Table<SetupPacketFactoringCompany>().FirstOrDefaultAsync(e => e.EntityId == dto.EntityId);
        if (model != null) {
            Mapper.Map(dto, model);
            await UpdateAsync(model);
        } else {
            model = Mapper.Map<SetupPacketFactoringCompany>(dto);
            model.EntityId = (await Table<SetupPacketFactoringCompany>().MaxAsync(e => (int?)e.EntityId) ?? 0) + 1;
            model.CreatedDate = DateTime.UtcNow;
            await InsertAsync(model);
        }
        return Mapper.Map<SetupPacketFactoringCompanyDto>(model);
    }

    public async Task DeleteAsync(int id) {
        await Table<SetupPacketFactoringCompany>().Where(e => e.EntityId == id).DeleteAsync();
    }
}
