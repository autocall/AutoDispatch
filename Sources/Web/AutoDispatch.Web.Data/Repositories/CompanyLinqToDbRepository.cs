using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Repositories;

/// <summary>
///     Репозиторий арендатора: CompanyId во всех запросах и при вставке </summary>
public class CompanyLinqToDbRepository<T> : LinqToDbRepository<T> where T : BaseCompanyEntity {

    protected readonly Guid CompanyId;

    public CompanyLinqToDbRepository(LinqToDbContext dbContext, Guid companyId) : base(dbContext) {
        this.CompanyId = companyId;
    }

    protected override IQueryable<T> Table() {
        return base.Table().Where(e => e.CompanyId == this.CompanyId);
    }

    public override async Task InsertAsync(T model, Guid userId) {
        model.CompanyId = this.CompanyId;
        await base.InsertAsync(model, userId);
    }

    public override async Task InsertAsync(IEnumerable<T> models, Guid userId) {
        foreach (var model in models) {
            model.CompanyId = this.CompanyId;
        }
        await base.InsertAsync(models, userId);
    }
}
