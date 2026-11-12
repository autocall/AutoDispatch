using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AutoDispatch.Web.Data.Context;

/// <summary>
///     Design-time для dotnet ef: строка подключения из appsettings.Development.json проекта Server </summary>
public class AcDbContextFactory : IDesignTimeDbContextFactory<AcDbContext> {
    public AcDbContext CreateDbContext(string[] args) {
        var serverDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "AutoDispatch.Web.Server"));
        var basePath = Directory.Exists(serverDir) ? serverDir : Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
#if DEBUG
            .AddJsonFile("appsettings.Development.json")
#else
            .AddJsonFile("appsettings.json")
#endif
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AcDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        return new AcDbContext(optionsBuilder.Options);
    }
}
