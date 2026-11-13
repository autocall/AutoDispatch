using AutoMapper;
using Newtonsoft.Json;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Mapping;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Серверная логика dispatch (этап 3), без БД и сервера: маппинг собирается, снимки истории совместимы с клиентом .NET Framework,
///     хелперы V1 ведут себя как в V1 </summary>
public class DispatchLogic_Test {

    [Fact]
    public void Test01_MappingConfigurationIsValid() {
        var config = new MapperConfiguration(conf => {
            conf.AddProfile(new DispatchMappingProfile());
            conf.AllowNullCollections = true;
        });
        // AssertConfigurationIsValid не вызывается: как в V1 и V3, CreatedByName/Files заполняют конвертеры и сервисы, не профиль
        var mapper = config.CreateMapper();
        var load = new Load { EntityId = Guid.NewGuid(), DispatchCompanyId = Guid.NewGuid(), OriginLatitude = 41.5m, OriginLongitude = -87.25m };
        var dto = mapper.Map<LoadDto>(load);
        Assert.Equal(load.DispatchCompanyId, dto.ActualCompanyId);
        Assert.Equal(41.5f, dto.OriginLocation?.Latitude);
        var back = mapper.Map<Load>(dto);
        Assert.Equal(load.DispatchCompanyId, back.DispatchCompanyId);
        Assert.Equal(-87.25m, back.OriginLongitude);
    }

    /// <summary>
    ///     HistoryObject.Data — gzip + UTF-16 JSON с $type; типы ядра должны писаться как mscorlib (клиент на .NET Framework) </summary>
    [Fact]
    public void Test02_ZipSnapshotRoundTripWithMscorlib() {
        var dto = new DriverDto { EntityId = Guid.NewGuid(), Name = "Driver 1", Phone = "123" };
        byte[] data = ZipExtension.SerializeObject(dto);
        string json = BinaryHelper.GetString(ZipExtension.Decompress(data));
        Assert.Contains("\"$type\":\"AutoDispatch.Dtos.Dtos.DriverDto, AutoDispatch.Dtos\"", json);
        Assert.DoesNotContain("System.Private.CoreLib", json);
        var back = ZipExtension.DeserializeObject<DriverDto>(data);
        Assert.Equal(dto.EntityId, back.EntityId);
        Assert.Equal("Driver 1", back.Name);
    }

    [Fact]
    public void Test03_JsonMscorlibBinderReadsBothNames() {
        var list = new List<Guid> { Guid.NewGuid() };
        string json = DispatchJson.Serialize(list);
        Assert.Contains("mscorlib", json);
        Assert.Equal(list[0], DispatchJson.Deserialize<List<Guid>>(json)[0]);
        Assert.Equal(list[0], DispatchJson.Deserialize<List<Guid>>(json.Replace("mscorlib", "System.Private.CoreLib"))[0]);
    }

    [Fact]
    public void Test04_NextLoadNumber() {
        var company = new DispatchCompany { NextLoadNumber = 100 };
        var load = new LoadDto();
        Assert.Equal(101, DispatchHelper.NextLoadNumber(load, company, null));
        Assert.Equal(100, load.Index);
        Assert.Equal(101, company.NextLoadNumber);

        var prefix = new List<DispatchCompanyCustom> { new() { Type = (int)CompanyCustomizationEnum.PrefixIndex, Params = "TR" } };
        DispatchHelper.NextLoadNumber(load, company, prefix);
        Assert.Equal("TR", load.IndexPrefix);
        Assert.Equal(101, load.Index);

        var byLocation = new List<DispatchCompanyCustom> { new() { Type = (int)CompanyCustomizationEnum.ModifyIndexByLocation, Params = "il:C;wa:W" } };
        company.NextLoadExtra = null;
        var load2 = new LoadDto { OriginCity = "Chicago, IL", DestinationCity = "Seattle, WA" };
        Assert.Equal(1, DispatchHelper.NextLoadNumber(load2, company, byLocation));
        Assert.Equal("C", load2.IndexExtra);
        Assert.Equal(2, DispatchHelper.NextLoadNumber(load2, company, byLocation));
        var load3 = new LoadDto { OriginCity = "Miami, FL", DestinationCity = "Dallas, TX" };
        Assert.Equal(1, DispatchHelper.NextLoadNumber(load3, company, byLocation));
        Assert.Equal("?", load3.IndexExtra);
    }

    [Fact]
    public void Test05_LocationHelper() {
        string location = "  chicago,  il 60601 ";
        Assert.True(LocationHelper.CheckLocation(ref location));
        Assert.Equal("Chicago, IL 60601", location);
        string bad = "Chicago IL";
        Assert.False(LocationHelper.CheckLocation(ref bad));
        Assert.Equal("IL", LocationHelper.GetStateCode("Chicago, IL"));
        Assert.Null(LocationHelper.GetStateCode("Chicago"));
    }

    [Fact]
    public void Test06_SetupPacketBinaryHelper() {
        var bools = new List<bool> { true, false, true, true, false, false, false, false, true };
        byte[] bin = SetupPacketBinaryHelper.BackConvert(bools);
        Assert.Equal(2, bin.Length);
        Assert.Equal(bools, SetupPacketBinaryHelper.Convert<bool>(bin, bools.Count));

        var shorts = new List<short> { 1, 300, -5 };
        Assert.Equal(shorts, SetupPacketBinaryHelper.Convert<short>(SetupPacketBinaryHelper.BackConvert(shorts)));
    }

    [Fact]
    public void Test07_StateHelper() {
        Assert.Equal("Illinois", StateHelper.GetStateByCode("IL").Name);
        Assert.Equal("IL", StateHelper.GetState(StateHelper.GetStateByCode("IL").Id).Code);
        Assert.Null(StateHelper.GetState(null));
        Assert.Equal("Canada", StateHelper.GetCountry(1).Name);
        Assert.Contains(StateHelper.GetStateDtos(), s => s.Code == "TX");
    }

    [Fact]
    public void Test08_StringHelpers() {
        Assert.Equal("(203) 500-6917", "2035006917".ToPhone());
        Assert.Equal("8002279558", "(800) 227-9558".OnlyNumbers());
        Assert.Equal("1,234.50", "$1,234.50".OnlyDecimal());
        var sb = new System.Text.StringBuilder("abc MARKER1 def MARKER2");
        Assert.Equal(4, sb.IndexOf("MARKER1"));
        Assert.Equal(-1, sb.IndexOf("MARKER3"));
    }
}
