using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Data.Mappings.Dispatch;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Модель dispatch (этап 2): EF-модель собирается без БД (связи по атрибутам однозначны, FK без каскада,
///     таблицы без префикса d_), LinqToDB-маппинг не принимает навигации за колонки </summary>
public class DispatchModel_Test {

    private static AdDbContext CreateContext() {
        var options = new DbContextOptionsBuilder<AdDbContext>().UseSqlServer("Server=.;Database=model_only").Options;
        return new AdDbContext(options);
    }

    [Fact]
    public void Test01_EfModelBuilds() {
        using var context = CreateContext();
        var model = context.Model;
        foreach (var type in DispatchMap.EntityTypes) {
            var entity = model.FindEntityType(type);
            Assert.NotNull(entity);
            Assert.Equal(type.Name, entity.GetTableName());
            Assert.All(entity.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.NoAction, fk.DeleteBehavior));
        }
        Assert.Equal(28, DispatchMap.EntityTypes.Length);
    }

    [Fact]
    public void Test02_EfLoadRelations() {
        using var context = CreateContext();
        var load = context.Model.FindEntityType(typeof(Load));
        // у Trailer одна коллекция Loads — она должна быть парой к Load.Trailer, а не к HistoryTrailer
        var trailer = load.FindNavigation(nameof(Load.Trailer));
        Assert.Equal(nameof(Trailer.Loads), trailer.Inverse?.Name);
        Assert.Null(load.FindNavigation(nameof(Load.HistoryTrailer)).Inverse);
        Assert.Equal(nameof(Load.Children1), load.FindNavigation(nameof(Load.Parent)).Inverse?.Name);
        Assert.Equal(nameof(Driver.Histories), load.FindNavigation(nameof(Load.HistoryDriver)).Inverse?.Name);
        Assert.Equal("decimal(9,6)", load.FindProperty(nameof(Load.OriginLatitude)).GetColumnType());
        Assert.Equal("binary(4)", load.FindProperty(nameof(Load.Background)).GetColumnType());
        Assert.Equal("varchar(128)", load.FindProperty(nameof(Load.OriginCity)).GetColumnType());
        Assert.Equal("varchar(max)", load.FindProperty(nameof(Load.Comments)).GetColumnType());
    }

    [Fact]
    public void Test03_LinqToDbColumns() {
        var schema = new MappingSchema();
        var builder = new FluentMappingBuilder(schema);
        DispatchMap.LinqToDbConfigure(builder);
        builder.Build();
        var load = schema.GetEntityDescriptor(typeof(Load));
        Assert.Equal("Load", load.TableName);
        var columns = load.Columns.Select(c => c.MemberName).ToList();
        Assert.Contains(nameof(Load.OriginLatitude), columns);
        Assert.Contains(nameof(Load.CompanyId), columns);
        Assert.DoesNotContain(nameof(Load.Trailer), columns);
        Assert.DoesNotContain(nameof(Load.Children1), columns);
        Assert.True(load.Columns.Single(c => c.MemberName == nameof(Load.EntityId)).IsPrimaryKey);
        var history = schema.GetEntityDescriptor(typeof(HistoryObject));
        Assert.True(history.Columns.Single(c => c.MemberName == nameof(HistoryObject.Id)).IsIdentity);
    }
}
