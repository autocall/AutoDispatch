using LinqToDB;
using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Repositories;

namespace AutoDispatch.Web.Data;

public class UnitOfWork : IDisposable {

    /// <summary>
    ///     Основная БД (DataBaseName) </summary>
    public LinqToDbContext Context { get; }

    /// <summary>
    ///     БД логов (LogDataBaseName): журнал входов, активность, таблица Log Serilog </summary>
    public LogLinqToDbContext LogContext { get; }

    /// <summary>
    ///     Кэш репозиториев по типу и CompanyId (default — без фильтра) </summary>
    private readonly Dictionary<(Type, Guid), IRepository> repositories = new();

    public UnitOfWork(LinqToDbContext context, LogLinqToDbContext logContext) {
        this.Context = context;
        this.LogContext = logContext;
    }

    private List<AdRole> roles = null;
    public List<AdRole> Roles {
        get {
            if (roles == null) {
                roles = this.Context.GetTable<AdRole>().ToList();
            }
            return roles;
        }
    }

    public IRepository<T> GetRepository<T>() where T : BaseEntity {
        if (repositories.ContainsKey((typeof(T), default))) {
            return repositories[(typeof(T), default)] as LinqToDbRepository<T>;
        }
        var repository = new LinqToDbRepository<T>(this.Context);
        repositories.Add((typeof(T), default), repository);
        return repository;
    }

    public IRepository<T> GetRepository<T>(Guid companyId) where T : BaseCompanyEntity {
        if (repositories.ContainsKey((typeof(T), companyId))) {
            return repositories[(typeof(T), companyId)] as CompanyLinqToDbRepository<T>;
        }
        var repository = new CompanyLinqToDbRepository<T>(this.Context, companyId);
        repositories.Add((typeof(T), companyId), repository);
        return repository;
    }

    public void Dispose() {
        this.Context.Dispose();
        this.LogContext.Dispose();
    }
}
