using AutoMapper;
using Newtonsoft.Json;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Dtos.Dispatch;
using AutoDispatch.Web.Logic.Mapping;
using AutoDispatch.Web.Logic.Mapping.Dispatch;
using AutoDispatch.Web.Logic.Services.Dispatch;

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
        config.CreateMapper(); // профиль собирается
        var load = new Load { EntityId = Guid.NewGuid(), DispatchCompanyId = Guid.NewGuid(), OriginLatitude = 41.5m, OriginLongitude = -87.25m };
        var dto = LoadConverter.ToDto(load);
        Assert.Equal(load.DispatchCompanyId, dto.ActualCompanyId);
        Assert.Equal(41.5f, dto.OriginLocation?.Latitude);
        var back = LoadConverter.ToModel(dto);
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
        // как V1 в d_Load.EmailInvoiceSent: "System.Collections.Generic.List`1[[System.Guid, mscorlib]], mscorlib"
        Assert.Contains("mscorlib", json);
        Assert.DoesNotContain("System.Private.CoreLib", json);
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

    /// <summary>
    ///     RTF→PDF шаблонов setup packet (Telerik .NET Standard): шрифты берутся из Resources/Fonts, а не подменяются стандартными PDF-шрифтами </summary>
    [Theory]
    [InlineData("000102040506071314.rtf")]
    [InlineData("000102040506071314_02.rtf")]
    [InlineData("05_02.rtf")]
    public void Test09_RtfToPdf(string template) {
        // логотип как в FillRtfTemplate: hex PNG вместо [logo] (1x1 прозрачный PNG — проверяет и конвертацию в JPEG)
        var logo = Convert.ToHexString(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="));
        var rtf = File.ReadAllText(Path.Combine(AppSettings.DocumentsPath, template)).Replace("[logo]", logo);
        var pdf = TelerikPdfHelper.RtfToPdf(rtf);
        using var document = UglyToad.PdfPig.PdfDocument.Open(pdf);
        var fonts = document.GetPages().SelectMany(p => p.Letters).Select(l => l.FontName).Distinct().ToList();
        Assert.Contains(fonts, f => f.EndsWith("Carlito")); // Calibri → Carlito (OFL), шрифты Microsoft в репозитории нет
        Assert.DoesNotContain(fonts, f => f.Contains("Helvetica"));
    }

    /// <summary>
    ///     Gains/Expenses из БД — массив с $type клиента (TypeNameHandling.All, AutoDispatch.Logic.Contracts): читается без разрешения
    ///     типов, иначе whitelist MscorlibBinder (этап 4) отбивал бы серверные типы и доплаты терялись (найдено на /Confirmation) </summary>
    [Fact]
    public void Test10_GainsFromV1Json() {
        const string json = "{\"$type\":\"AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses.GainViewModel[], AutoDispatch.Logic.Contracts\"," +
            "\"$values\":[{\"$type\":\"AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses.GainViewModel, AutoDispatch.Logic.Contracts\"," +
            "\"Cost\":150.0,\"Comment\":null,\"Paid\":false,\"PaidDate\":null,\"Type\":1}," +
            "{\"$type\":\"AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses.GainViewModel, AutoDispatch.Logic.Contracts\"," +
            "\"Cost\":20.5,\"Comment\":\"Tolls\",\"Paid\":true,\"PaidDate\":null,\"Type\":6}]}";
        var gains = ExpensesConverter.ToGains(json);
        Assert.Equal(Enum.GetValues(typeof(GainType)).Length, gains.Count);
        var lumper = gains.Single(g => g.Type == GainType.Lumper);
        Assert.Equal(150m, lumper.Cost);
        Assert.Equal("Lumper", lumper.TypeDescription());
        var other = gains.Single(g => g.Type == GainType.Other1);
        Assert.Equal(20.5m, other.Cost);
        Assert.Equal("Tolls", other.TypeDescription());
        // null и мусор — список всех типов без доплат, как в V1
        Assert.True(ExpensesConverter.ToGains(null).All(g => g.Cost == null));
        Assert.True(ExpensesConverter.ToGains("not json").All(g => g.Cost == null));
    }

    /// <summary>
    ///     Обобщённые методы DispatchService (SaveWithHistoryAsync, BaseDeleteAsync, Undo) выбирают конвертер по типу объекта:
    ///     каждый тип туда и обратно приходит своим XConverter, SetupPacket — с вложенными автоматическими маппингами </summary>
    [Fact]
    public void Test11_ObjectConverterByType() {
        var mapper = new MapperConfiguration(conf => conf.AddProfile(new DispatchMappingProfile())).CreateMapper();
        var entities = new BaseObjectEntity[] {
            new Load(), new Driver(), new Truck(), new Trailer(), new DatCompany(), new DatDriver(), new Warehouse(), new Yard(), new SetupPacket()
        };
        foreach (var entity in entities) {
            entity.EntityId = Guid.NewGuid();
            entity.CompanyId = Guid.NewGuid();
            var dto = ObjectConverter.ToDto<BaseObjectDto>(entity, mapper);
            Assert.Equal(entity.GetType().Name + "Dto", dto.GetType().Name);
            Assert.Equal(entity.CompanyId, dto.CompanyId);
            var back = ObjectConverter.ToModel<BaseObjectEntity>(dto);
            Assert.IsType(entity.GetType(), back);
            Assert.Equal(entity.EntityId, back.EntityId);
        }
        Assert.Null(ObjectConverter.ToDto<LoadDto>(null, mapper));
    }

    /// <summary>
    ///     Имя документа от клиента — только имя файла: «..», каталоги и абсолютные пути не выходят из каталога сущности (audit A1) </summary>
    [Theory]
    [InlineData("../../app/appsettings.Production.json")]
    [InlineData("/app/appsettings.Production.json")]
    [InlineData("sub/file.pdf")]
    [InlineData("..")]
    [InlineData(".")]
    [InlineData(" ")]
    public void Test13_FileNameWithoutPath(string name) {
        var id = Guid.NewGuid();
        Assert.Throws<AutoDispatch.Web.Common.Exceptions.InfoException>(() => DispatchFileHelper.FileGetFullPath(id, name));
        Assert.Throws<AutoDispatch.Web.Common.Exceptions.InfoException>(() => DispatchFileHelper.InsuranceGetFullPath(id, name));
        Assert.EndsWith("BOL (1).pdf", DispatchFileHelper.FileGetFullPath(id, "BOL (1).pdf"));
        Assert.EndsWith("rate *.pdf", DispatchFileHelper.FileGetFullPath(id, "rate *.pdf"));
        Assert.EndsWith(id.ToString(), DispatchFileHelper.FileGetFullPath(id));
    }

    /// <summary>
    ///     Секции контактов setup packet по MARKER1..4: заголовки целиком, пустая секция вырезается, маркеров не остаётся.
    ///     В V1 (и в V3 до 2026-11-23) маркеры удалялись до вырезания — съедалось по 7 символов: «ontact Information» и т.п. </summary>
    [Theory]
    [InlineData("000102040506071314.rtf")]
    [InlineData("000102040506071314_02.rtf")]
    public void Test12_SetupPacketContactSections(string template) {
        var fields = new Dictionary<string, string> {
            // 1x1 PNG вместо логотипа компании ([logo] → hex)
            ["ActualCompany.LogoBig"] = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=",
            ["SetupPacket.SalesContact.Contact"] = "Sales Person",
            ["SetupPacket.AfterHourContact.Contact"] = "Accounting Person"
            // DispatchContact нет — секция вырезается
        };
        var rtf = ReportService.FillRtfTemplate(Path.Combine(AppSettings.DocumentsPath, template), new SetupPacketDto(), fields);
        Assert.DoesNotContain("MARKER", rtf);
        Assert.Contains("Sales Contact Information", rtf);
        Assert.Contains("Accounting Contact Information", rtf);
        Assert.DoesNotContain("No Accounting Contact Information", rtf); // это подпись галочки шага 2, не заголовок секции
        Assert.DoesNotMatch(@"\\trbrdr[tlbrhv]\s*\\brdrs", rtf); // рамки стиля таблиц — как в V1, без них
        Assert.DoesNotContain("Dispatch Contact Information", rtf);
    }

    /// <summary>
    ///     Шаблоны по версии packet'а: версия 3 — W-9 Rev. March 2024 (fw9_03.pdf), setup packet — _02 (своего _03 нет);
    ///     до 3 — W-9 Rev. November 2017 V1 (fw9.pdf). Обе формы W-9 заполняются без ошибок (решено 2026-11-23) </summary>
    [Theory]
    [InlineData(1, "fw9.pdf", "000102040506071314.rtf")]
    [InlineData(2, "fw9.pdf", "000102040506071314_02.rtf")]
    [InlineData(3, "fw9_03.pdf", "000102040506071314_02.rtf")]
    public void Test13_SetupPacketTemplatesByVersion(byte version, string w9, string setupPacket) {
        var w9Path = DispatchFileHelper.GetFileByVersion(Path.Combine(AppSettings.DocumentsPath, "fw9.pdf"), version);
        Assert.Equal(w9, Path.GetFileName(w9Path));
        Assert.Equal(setupPacket, Path.GetFileName(DispatchFileHelper.GetFileByVersion(Path.Combine(AppSettings.DocumentsPath, "000102040506071314.rtf"), version)));

        var fields = new[] { "W9Name", "W9BusinessName", "W9Address", "W9City", "W9StateId", "W9ZipCode", "W9NamePerson", "W9AccountNumber", "W9ExemptPayeeCode",
            "W9FATCACode", "W9SocialSecurityNumber", "W9EmployerIdentificationNumber", "W9CompanyType", "W9TaxClassification", "W9OtherTaxClassification",
            "W9Signature", "W9AgreeDate" }.ToDictionary(e => "SetupPacket." + e, e => (string)null);
        fields["SetupPacket.W9Name"] = "Carpathian Eagle Inc";
        fields["SetupPacket.W9EmployerIdentificationNumber"] = "12-3456789";
        fields["SetupPacket.W9CompanyType"] = "6";
        // ReportService без ServicesHost: заполнение W-9 сервисы не использует
        var reportService = (ReportService)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ReportService));
        var pdf = reportService.GenerateW9Pdf(w9Path, new SetupPacketDto { Version = version, W9CompanyType = 6, W9ForeignPartnersHas = true }, fields);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
