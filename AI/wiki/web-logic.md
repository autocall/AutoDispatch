# AutoDispatch Web — серверная логика dispatch (этап 3)

Порт BLL V1 (`AutoCall.Logic.BLL` + `AutoDispatch.Logic.Contracts`) в `AutoDispatch.Web.Logic` на образце AutoCall V3: `ServicesHost`,
`BaseService(host)`, LinqToDB вместо EF6, `ApiResult<T>` для внешних API, `BaseBackgroundService` вместо Quartz. Состояние 2026-11-13:
код перенесён и собирается (~11 тыс. строк), офлайн-тесты `08_DispatchLogic` проходят; против БД не запускался (ждёт миграции `Dispatch`
и переноса данных, [[data-migration]]). Контракт — [[protocol]], модель — [[data-model]], что переносилось — [[v1-inventory]].

## Состав

| Где | Что | V1 |
|---|---|---|
| `Sources/Shared/AutoDispatch.Dtos` | контракт как есть (netstandard2.0, C# 7.3, Newtonsoft 13) + `Common/` — `BaseDto`, `DisplayAttribute`, `AutoCallEquipmentType`, `EnumExtensions` из `AutoCall.Common` с теми же namespace; имя сборки `AutoDispatch.Dtos` | `_Dtos/AutoDispatch.Dtos`, `_Common/AutoCall.Common` |
| `Services/Dispatch/Base` | `DispatchBaseService` (`Db`, `Table<T>`, `Alive<T>`, `GetByIdAsync`, Insert/Update/Delete целиком по PK, кэш user→company), `DispatchCrudService<TDto,TEntity>` | `HostService`, `EntityCrudService`, `EntityRepository` |
| `Services/Dispatch/DispatchService*.cs` | 9 partial-файлов: ядро и события компании, `Location`, `Load` (поиск истории, `LoadHistory2` сырой SQL с гаверсинусом, статистика), `Save` (все Save*/Delete*, история и снимки), `Files` (дефекты, файлы, страховка), `Emails` (SMTP компании, setup packet, инвойсы, confirmation, SMS), `Undo`, `Trackers` (CarrierView, BNSF, Waiting), `Events` (long-polling `EventNotificationAsync`) | `DispatchService.cs` 4651 строка |
| `Services/Dispatch/IDispatchService.cs` | 74 метода с `[MinAccess]` — таблица `Name → handler` для `Api/Dispatch/Request` (этап 4) | `IDispatchService` |
| `Services/Dispatch/*Service.cs` | `DatCompanyService`, `DispatchCompanyService` (=ActualCompany), `CensusService`, `SetupPacketService`, `ConfirmationService`, `CustomStatusService`, `CustomerCompanyService`, `ShipperReceiverCompanyService`, `FactoringCompanyService`, `HistoryCleanupService` + `LoadIndexCacheService`, `GeocodeService`, `FixLocationLoadsService`, `ReportService`, `OpenAIService` | те же имена |
| `Mapping/Dispatch/*`, `DispatchMappingProfile` | конвертеры `ITypeConverter` (регистрация по namespace через reflection) и простые `ReverseMap`; профиль добавлен в `Program.cs` | `Converters/Dispatch/**`, `BllMappingProfile` |
| `WebServices/` | `CarrierView`, `Bnsf` (HttpClient + cookies на запрос), `Safer` (FMCSA + `BrokersnapshotWebService.CompanyAsync/DownloadCertificateAsync`), `SmsWebService` (Bandwidth), `OpenAI` | `DataServices/*` |
| `Helpers/Dispatch/` | `StateHelper` (статика State/Country/Zone, снята с боевой БД 2026-11-13), `DispatchHelper` (номера грузов и инвойсов, rate), `DispatchFileHelper` (пути `FilesPath/{EntityId}`), `LoadCompanyNames`, `UserNameCache`, `SetupPacketBinaryHelper`, `DatCompanyContactHelper`, `LocationHelper`, `ImageHelper` (ImageSharp), `CompareEntities`, `ExpressionHelper` | `Helpers/*` |
| `Utils/` | `DispatchPdfGenerator` (iTextSharp: подпись confirmation, merge, схема), `DispatchEmailGenerator` (шаблоны `Resources/Emails`) | `PdfGenerator`, `EmailGenerator` |
| `Email/` | `Email*`, `EmailUtils.SendEmailAsync` (System.Net.Mail по SMTP компании) | `AutoCall.Common.Email` |
| `Common/Extensions` | `DispatchJson` (+`MscorlibBinder`), `ZipExtension`, `BinaryHelper`, `DispatchStringExtension`, `StringBuilderExtension`, `CollectionExtension`; `Helpers/MathHelper`, `DistanceHelper`; `AccessDeniedException` | `AutoCall.Common` |
| `Server/BackgroundServices` | `HistoryCleanupBackgroundService` (24 ч: чистка истории + индекс грузов за сутки), `LoadWaitingBackgroundService` (5 мин), `LoadSendSmsBackgroundService` (5 мин), `BnsfRefreshBackgroundService` (30 мин, выключен флагом) | Quartz `JobScheduler.cs` |
| `Server/Resources` | `Emails/*.html`, `Documents/*.rtf` + `fw9.pdf`, `Fonts/BRUSHSCI.TTF` — копируются в выход, `AppSettings.ResourcesPath` | `Web/Emails`, `Web/Documents`, `Content/fonts` |
| `AppSettings` | `#region Dispatch` в General (интервалы джобов, `HistoryExpiresDays`, `SetupPacketVersion`, OpenAI model/url, пути `Resources`, `TempPath`, `OpenAiLogsPath`), Secrets: `Sms:Bandwidth:*`, `OpenAI:ApiKey`, `Keeppingin:ApiKey`, `GoogleMap:Api`; `RecyclePath = FilesPath/_recycle` | `Web/AppSettings.cs`, `web.config` |

Пакеты Logic: AutoMapper 13, HtmlAgilityPack, iTextSharp.LGPLv2.Core (API iTextSharp 4.x), SixLabors.ImageSharp, PdfPig, PDFtoImage (pdfium).

## Как перенесено

- **EF6 → LinqToDB.** Сущности dispatch читаются `Table<T>()`/`Alive<T>()`, пишутся `InsertAsync/UpdateAsync` целиком (как `Update(entity)`
  V1), identity `Event`/`HistoryObject`/`HistoryLocation*` — `InsertWithInt32IdentityAsync`. Навигации не грузятся — связи достаются join'ами
  или вторым запросом (`DispatchCompanyService.GetByIdAsync` с Customizes, `SetupPacketService.LoadRelationsAsync`).
- **Снимки истории** — тот же формат `HistoryObject.Data`: gzip + UTF-16 JSON с `$type` (`ZipExtension`). `DispatchJson.MscorlibBinder` пишет
  `System.Private.CoreLib` как `mscorlib`, читает оба — иначе клиент на .NET Framework не десериализует `List<Guid>` и т.п.
- **Пользователи.** `UserNameCache` и `GetUsers` — по `AdUser`/`AdUserRole`; роль склада в `SaveLoads` — `RoleEnum.Warehouse.ToRoleId()`;
  системные действия (джобы, Undo) — `AdUser.SystemUserId` вместо V1 `SystemAdmin`/`AutoCallAdmin`. Подпись агента в письме
  confirmed — новая колонка `AdUser.Signature` (`varchar(64)`, из `autocall.dbo.Users.Signature`; миграция — пользователь).
- **Файлы.** `AppSettings.FilesPath/{EntityId}`, удалённые — `RecyclePath` (подкаталог `/files/_recycle`, второй volume не нужен),
  PNG для OpenAI — `TempPath` (`/files/_temp`), отдаётся по `/Temp` (этап 4).
- **Внешние API** — `ApiResult<T>`, исключения внутри. Таймзона груза — keeppingin `TimeZone_Get`, расстояние — Google Distance Matrix,
  геокодинг/автокомплит — Brokersnapshot (как V3). Кэши `GeocodeService` в памяти процесса — таблицы `Cache` нет.
- **Справочники** `State/Country/Zone` — `StateHelper` (статика). `GetStatistics` без `StateBorders`: штат берётся из `DestinationCity`.
- **Джобы.** Список снят с `Web/App_Start/JobScheduler.cs` V1 (не с прод-конфига): QuartzJob 01:00 будни → чистка истории,
  CacheLoadJob 00:10 → индекс грузов, LoadWaitingJob и LoadSendSmsJob по 5 мин, BnsfRailwayJob 30 мин (в V1 выключен системным флагом,
  ни у одной из 3 компаний с BnsfLogin нет `BnsfLastDone` за 30 дней → `AppSettings.BnsfRefreshEnabled = false`). `ClearExpiredCacheJob` — таблицы нет.
- **PDF.** W9 — AcroForm `fw9.pdf` через iTextSharp (LGPL-порт, тот же API 4.x). Подпись confirmation/merge — тот же iTextSharp.
  **RTF→PDF (`ReportService.RtfToPdf`) не реализован** — V1 Telerik Documents; бросает `NotImplementedException`, пока не решено ([[plan]]).
  Текст PDF для OpenAI — PdfPig с разделителями `----------- PageN -----------` (формат Telerik, на него завязан разбор страниц),
  PNG — PDFtoImage.

## Не перенесено (решения в [[plan]])

`ChatService`/`d_Message`; `ReportSimilarLoads` (решено 2026-11-13); Voxbone SMS и картинки в SMS (в V1 помечен нерабочим);
`DatRateDataService`, `Keeppingin SetPassword`, `GoogleMapDataService` (карту даёт `MapService` V3); обслуживающие методы
`Test`, `Set_Load_DeliveredDate`, `TestEvent`, `ReplaceExtraIndex`, `DeleteDuplicateDatCompanies`; `EntityCrudService` → `DispatchCrudService`;
`EnableCarrierViewTracker` не пишет `CarrierView*` в отсоединённую сущность (V1 их и так терял).

## Проверено 2026-11-13

`AutoDispatch.Web.sln` собирается; `08_DispatchLogic` (8 тестов, без БД): маппинг Load↔LoadDto с `DispatchCompanyId↔ActualCompanyId`
и координатами, снимок `ZipExtension` с `mscorlib`, `MscorlibBinder` читает оба имени, `NextLoadNumber` (обычный, prefix, по штатам),
`LocationHelper`, `SetupPacketBinaryHelper`, `StateHelper`, строковые хелперы. `AssertConfigurationIsValid` не вызывается — как в V1 и V3,
`CreatedByName`/`Files` заполняют конвертеры и сервисы.

## Открыто

1. RTF→PDF на Linux (Telerik Documents или QuestPDF с переписыванием шаблонов) — `ReportService.RtfToPdf`.
2. iTextSharp: выбран `iTextSharp.LGPLv2.Core` (LGPL, тот же API) вместо itext7 (AGPL) — подтвердить.
3. Миграция `AdUser.Signature` + перенос значения в скрипт ([[data-migration]]).
4. Секреты на хосте: `Sms:Bandwidth:*`, `OpenAI:ApiKey`, `Keeppingin:ApiKey`, `GoogleMap:Api` (значения — из V1 `Web/AppSettings.cs`, в репозиторий не попадают).
5. Проверка против БД после миграции и переноса: тесты по методам `IDispatchService` (этап 4 даёт `Api/Dispatch/Request`).
