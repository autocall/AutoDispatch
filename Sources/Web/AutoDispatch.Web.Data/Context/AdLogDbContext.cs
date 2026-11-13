using Microsoft.EntityFrameworkCore;
using AutoDispatch.Web.Data.Entities.Log;

namespace AutoDispatch.Web.Data.Context;

/// <summary>
///     EF Core для БД логов (LogDataBaseName): только миграции таблиц UserLogin, UserActivity и Metric.
///     Таблицу Log создаёт Serilog sink, запросы — через LogLinqToDbContext </summary>
public class AdLogDbContext : DbContext {

    public AdLogDbContext(DbContextOptions<AdLogDbContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<UserLogin>(e => {
            e.ToTable("UserLogin");
            e.HasIndex(x => x.DateTime);
            e.HasIndex(x => new { x.UserId, x.DateTime });
        });
        modelBuilder.Entity<UserActivity>(e => {
            e.ToTable("UserActivity");
            e.HasIndex(x => x.SeenAt);
            e.HasIndex(x => new { x.UserId, x.SeenAt });
        });
        modelBuilder.Entity<Metric>(e => {
            e.ToTable("Metric");
            e.HasKey(x => new { x.DateTime, x.MetricId });
        });
    }
}
