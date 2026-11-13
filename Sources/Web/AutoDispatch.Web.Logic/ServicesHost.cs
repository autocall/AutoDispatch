using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using AutoDispatch.Web.Data;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Logic;

/// <summary>
///     Scoped-хост сервисов: UnitOfWork, Mapper, Identity-менеджеры; сервисы создаются лениво через GetService </summary>
public class ServicesHost {

    private readonly Dictionary<Type, IService> services = new Dictionary<Type, IService>();

    private RoleManager<AdRole> _roleManager;
    public RoleManager<AdRole> RoleManager {
        get {
            if (_roleManager == null) {
                _roleManager = this.ServiceProvider.GetRequiredService<RoleManager<AdRole>>();
            }
            return _roleManager;
        }
    }

    private UserManager<AdUser> _userManager;
    public UserManager<AdUser> UserManager {
        get {
            if (_userManager == null) {
                _userManager = this.ServiceProvider.GetRequiredService<UserManager<AdUser>>();
            }
            return _userManager;
        }
    }

    public UnitOfWork UnitOfWork { get; }
    public IMapper Mapper { get; }
    public IServiceProvider ServiceProvider { get; }

    public ServicesHost(UnitOfWork unitOfWork, IMapper mapper, IServiceProvider serviceProvider) {
        this.ServiceProvider = serviceProvider;
        this.UnitOfWork = unitOfWork;
        this.Mapper = mapper;
    }

    /// <summary>
    ///     Сервис по типу; единственный конструктор сервиса — (ServicesHost host) </summary>
    public T GetService<T>() where T : IService {
        if (services.ContainsKey(typeof(T)) == false) {
            services[typeof(T)] = (T)Activator.CreateInstance(typeof(T), this);
        }
        return (T)services[typeof(T)];
    }
}
