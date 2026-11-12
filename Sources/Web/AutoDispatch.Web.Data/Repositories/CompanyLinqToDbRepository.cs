using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Repositories;

/// <summary>
///     Репозиторий арендатора: CompanyId во всех запросах и при вставке </summary>
public class CompanyLinqToDbRepository<T> : LinqToDbRepository<T> where T : BaseCompanyEntity {

    protected readonly int CompanyId;

    public CompanyLinqToDbRepository(LinqToDbContext dbContext, int companyId) : base(dbContext) {
        this.CompanyId = companyId;
    }

    protected override IQueryable<T> Table() {
        return base.Table().Where(e => e.CompanyId == this.CompanyId);
    }

    public override async Task InsertAsync(T model, int userId) {
        model.CompanyId = this.CompanyId;
        await base.InsertAsync(model, userId);
    }

    public override async Task InsertAsync(IEnumerable<T> models, int userId) {
        foreach (var model in models) {
            model.CompanyId = this.CompanyId;
        }
        await base.InsertAsync(models, userId);
    }
}
