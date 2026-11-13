using LinqToDB.Async;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Repositories;
using AutoDispatch.Web.Logic.Dtos;

namespace AutoDispatch.Web.Logic.Services;

public class CompanyService : BaseService {

    private IRepository<Company> Rep => base.Rep<Company>();

    public CompanyService(ServicesHost host) : base(host) { }

    /// <summary>
    ///     deleted — только удалённые, иначе только активные; name — подстрока названия, пусто — все </summary>
    public async Task<List<CompanyDto>> GetAllAsync(bool deleted, string name = null) {
        name = String.IsNullOrWhiteSpace(name) ? null : name.Trim();
        var models = await Rep.GetAll(true).Where(e => e.IsDeleted == deleted && (name == null || e.Name.Contains(name)))
            .OrderBy(e => e.Name).ToListAsync();
        return Mapper.Map<List<CompanyDto>>(models);
    }

    public async Task<CompanyDto> GetAsync(Guid id) {
        var model = await Rep.GetByIdAsync(id);
        return Mapper.Map<CompanyDto>(model);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto dto, Guid userId) {
        var model = Mapper.Map<Company>(dto);
        await Rep.InsertAsync(model, userId);
        return await GetAsync(model.Id);
    }

    public async Task<CompanyDto> UpdateAsync(UpdateCompanyDto dto, Guid userId) {
        var model = Mapper.Map<Company>(dto);
        await Rep.UpdateAsync<ICompanyUpdateMap>(model, userId);
        return await GetAsync(dto.Id);
    }

    public async Task DeleteAsync(Guid id, Guid userId) {
        if (id == Company.SystemCompanyId) {
            throw new InfoException("System company cannot be deleted");
        }
        await Rep.UpdateIsDeletedAsync(id, true, userId);
    }

    public async Task RestoreAsync(Guid id, Guid userId) {
        await Rep.UpdateIsDeletedAsync(id, false, userId);
    }
}
