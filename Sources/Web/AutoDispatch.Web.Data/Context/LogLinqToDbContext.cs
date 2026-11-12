using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities.Log;

namespace AutoDispatch.Web.Data.Context;

/// <summary>
///     LinqToDB для БД логов: UserLogin, UserActivity, Metric и чтение таблицы Log Serilog </summary>
public class LogLinqToDbContext : DataConnection {
    // DataOptions<T>: своя строка подключения, не общая с LinqToDbContext
    public LogLinqToDbContext(DataOptions<LogLinqToDbContext> options) : base(options.Options) {
        var mappingSchema = new MappingSchema();
        var builder = new FluentMappingBuilder(mappingSchema);
        builder.Entity<UserLogin>().HasTableName("UserLogin").HasPrimaryKey(e => e.Id);
        builder.Entity<UserActivity>().HasTableName("UserActivity").HasPrimaryKey(e => e.Id);
        builder.Entity<Metric>().HasTableName("Metric").HasPrimaryKey(e => new { e.DateTime, e.MetricId });
        builder.Entity<LogEntry>().HasTableName(AppSettings.LogTableName).HasPrimaryKey(e => e.Id);
        builder.Build();
        this.AddMappingSchema(mappingSchema);
    }
}
