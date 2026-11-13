using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AutoDispatch.Web.Data.Context;

/// <summary>
///     Design-time для dotnet ef --context AdLogDbContext: строка LogConnection из appsettings проекта Server </summary>
public class AdLogDbContextFactory : IDesignTimeDbContextFactory<AdLogDbContext> {
    public AdLogDbContext CreateDbContext(string[] args) {
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

        var optionsBuilder = new DbContextOptionsBuilder<AdLogDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("LogConnection"));
        return new AdLogDbContext(optionsBuilder.Options);
    }
}
