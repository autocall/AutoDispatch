using AutoMapper;
using AutoDispatch.Web.Data;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Repositories;

namespace AutoDispatch.Web.Logic.Services;

public interface IService { }

public abstract class BaseService : IService {

    protected readonly UnitOfWork UnitOfWork;
    protected readonly IMapper Mapper;
    protected readonly ServicesHost Host;

    public BaseService(ServicesHost host) {
        this.UnitOfWork = host.UnitOfWork;
        this.Mapper = host.Mapper;
        this.Host = host;
    }

    /// <summary>
    ///     Репозиторий арендатора: все запросы и вставки с CompanyId </summary>
    public IRepository<T> Rep<T>(Guid companyId) where T : BaseCompanyEntity => this.UnitOfWork.GetRepository<T>(companyId);

    /// <summary>
    ///     Репозиторий без фильтра по компании (справочники, сами компании) </summary>
    public IRepository<T> Rep<T>() where T : BaseEntity => this.UnitOfWork.GetRepository<T>();
}
