using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using AutoDispatch.Web.Data.Mappings;
using AutoDispatch.Web.Data.Mappings.Dispatch;

namespace AutoDispatch.Web.Data.Context;

/// <summary>
///     LinqToDB: все запросы, вставки и обновления. Имена таблиц — через XxxMap.LinqToDbConfigure </summary>
public class LinqToDbContext : DataConnection {
    // DataOptions<T>: у каждого контекста свои настройки (иначе второй контекст получает строку подключения первого)
    public LinqToDbContext(DataOptions<LinqToDbContext> options) : base(options.Options) {
        var mappingSchema = new MappingSchema();
        var builder = new FluentMappingBuilder(mappingSchema);
        CompanyMap.LinqToDbConfigure(builder);
        UserMap.LinqToDbConfigure(builder);
        UserSettingMap.LinqToDbConfigure(builder);
        DispatchMap.LinqToDbConfigure(builder);
        builder.Build();
        this.AddMappingSchema(mappingSchema);
    }
}
