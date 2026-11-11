# AutoDispatch в V1 — что переносим

Инвентарь AutoDispatch в репозитории V1 `D:\Develop\AutoCall\Sources` (состояние на 2026-11-11). AutoDispatch — модуль
внутри общего решения AutoCall: общий WPF-клиент, общий сайт WebV1, общая БД `autocall`. Setup Packet — не отдельный модуль,
а часть AutoDispatch, привязанная к карриеру (`d_DatCompany`).

## Контракт — `_Dtos/AutoDispatch.Dtos`

Общий проект клиента и сервера. `Data/*RequestData|ResponseData` (~170 типов), `Dtos/` (51), `Enums/` (20: `EventTypeDto`,
`DispatchBoardType`, `ResponseStatus`, `DispatchPermissions`…), `Requests/EventRequest`, `Responses/EventResponse`.
`ModuleName.AutoDispatch` — `_Common/AutoCall.Common/Enums/ModuleName.cs:13`. Протокол — [[protocol]].

## Клиент — `App/`

| Проект | Содержимое |
|---|---|
| `Project/Data/AutoDispatch.Data(.Contracts)` | `IUserSession/UserSession` |
| `Project/Logic/AutoDispatch.Logic(.Contracts)` | ViewModels (37 Pages, 45 Items), сервисы `IDispatchService` (1166 строк), `ILoginService`, `IReportService`, `IModuleService`, `IChatService` (не используется UI); менеджеры `IEngineManager`, `ILoginManager`; data-сервисы `IEngineDataService`, `ILoginDataService`, `IDatCompanyDataService`, `IActualCompanyDataService`, `IHelperDataService`; `Bootstrapper.cs` (Unity), `BllMappingProfile`, конвертеры, `Heplers/PdfConverter`, `ScanFileHelper`, `PlaceAutocompleteJsonParser` |
| `Project/UI/AutoCall.WPF` | 488 cs/xaml, из них ~213 dispatch: `Dialogs/Dispatch`, `Dialogs/LoadDetails`, `UserControls/Boards` (DispatchCarrierBoard, DispatchBrokerBoard, DispatchCarrierAccountBoard, DispatchBrokerAccountBoard, DispatchCarrierHistoryBoard). `MainWindow.xaml.cs:218-334` создаёт борды по ролям (`RolesOperator.CarrierBoards/BrokerBoards/AccountingBoards`); `LoginDialog.xaml.cs:145-156` — `AutoDispatch.Logic.Bootstrapper.Initialize` + `ILoginManager.Login`, если в модулях есть AutoDispatch; `Launch/AutoCall.Publish/Program.cs:26-27` — `Assembly.LoadFrom` AutoDispatch.Data/Logic.dll |

Стек: .NET Framework 4.6.2, Telerik WPF (Controls, GridView, Documents, Spreadsheet, Pdf/Rtf), Unity 4, CommonServiceLocator,
AutoMapper 4.2.1, log4net/Common.Logging, Newtonsoft 8, System.ServiceModel. `AutoDispatch.Logic` ссылается на `AutoCall.Common`,
`AutoCall.Dtos`, `AutoCall.Logic.Contracts` (App).

Обращения App к WebV1 (`BaseUrl` из `app.config` `UrlApi`, JSON в теле `application/x-www-form-urlencoded`):

| Data-сервис | Эндпоинты |
|---|---|
| `EngineDataService` | `Api/Dispatch/Request`, `Response`, `RequestZip`, `ResponseZip` |
| `LoginDataService` | `Api/Token`, `Api/Account/Logout`, `Api/Account/Check` |
| `DatCompanyDataService` | `api/DatCompany/Search?dot&mc`, `api/DatCompany/SendSetupPocket`, `Home/SearchCensusCompanies`, `CarrierRegistration/FactoringCompanies` |
| `HelperDataService` | `Dispatch/SearchLoads`, `SearchCustomers`, `GetPlaceAutocomplete`, `GetPlaceDetails`, `GetDistance`, `ExtractLoadFromPdf(2)` |
| `ActualCompanyDataService` | `ActualCompany/ActualCompaniesByUserId` |

Не dispatch, хотя лежит в `Dialogs/Dispatch`: `SimilarLoadsDialog` (WinForms `ReportViewer`, `SimilarLoadsReport.rdlc`) —
открывается кнопкой «Similar Loads» из карточки груза лоадборда (`RecordBaseDetailsDialog.cs:181`, `LoadDetails*Dialog.xaml`),
вызывает `IDispatchService.ReportSimilarLoads` → сервер `DispatchService.cs:3445` → `ISimilarSearchRepository` по `d_Load`.
В AutoDispatch App не переносится.

## Сервер — `Web/`

**Контроллеры.** `DispatchApiController` (`[RoutePrefix("api/Dispatch")]`: CheckToken, Request, Response, RequestZip, ResponseZip);
`DispatchController` (Dispatcher — карта, RouteLocation, OrderStops, SelectLocation, SearchLoads, SearchCustomers,
GetPlaceAutocomplete, GetPlaceDetails, GetDistance, GetTimeZone, ExtractLoadFromPdf(2), GetHybridRate);
`SetupPacketController` (route `CarrierRegistration`: Index публичный по carrierId, View/Edit под `AutoCallAuthorize`,
Step00..Step14, Prev01..14, DownloadFileMC/COI, FactoringCompanies); `PrintController` (Print/SetupPacket/Agreement,
InsuranceInstructions); ActualCompanyController, DatCompanyApiController, ConfirmationController, CustomStatusController,
FactoringCompanyController, AutoCallEmailController, HomeApiController (SearchCensusCompanies).

**BLL** (`Logic/AutoCall.Logic.BLL`): `DispatchService` (4651 строка, `IDispatchService` 77 методов — список в [[protocol]]),
DatCompanyService, ActualCompanyService, CensusService, SetupPacketService (`ISetupPacketService`: GetFactoringCompaniesAsync,
CreateSetupPacketAsync, OnlySaveAsync, SendAsync), ConfirmationService, ChatService (не переносится), CustomStatusService,
CustomerCompanyService, ShipperReceiverCompanyService, FactoringCompanyService, FixLocationLoadsService, ReportService
(Telerik.Windows.Documents Rtf/Pdf + iTextSharp; шаблоны `Documents/05_02.rtf`, `fw9.pdf`, `000102040506071314.rtf`),
OpenAIService (разбор PDF груза), GeocodeService; `Utils/DispatchPdfGenerator`, `DispatchEmailGenerator`;
`Helpers/SetupPacketBinaryHelper`, `DispatchHelper`, `DatCompanyContactHelper`. `QuartzService` — SQL-чистка `d_HistoryObject`
и `d_Event` старше 30 дней (`historyExpires = 30`), кроме снимков, на которые ссылаются живые объекты и грузы не в статусе Paid.

**Внешние сервисы** (`DataServices/`): ICarrierViewDataService (7 вызовов), ISmsDataService (Voxbone/Bandwidth, 4),
IBnsfRailwayDataSource (2), ISaferFmcsaDataService, IGoogleMapDataService, IDatRateDataService, OpenAI, Keeppingin, Gmail SMTP;
Magick.NET.

**Настройки** (`IAppSettings`): FilePath, RecyclePath, SetupPacketVersion, GoogleMapApiKey, BingMapApiKey, Voxbone/Bandwidth,
OPENAI_*, GmailSmtp*, BrokersnapshotTokenApi, DatPowerService*, GpsHost/GpsKey, Domain, DefaultPassword.
Роли с модулем AutoDispatch (`Permissions.cs:11-27`): Agent, Warehouse, Dispatcher, Sales, Accounting, Owner, Admin.

**`Microsoft.ReportViewer`** к dispatch не относится: `Microsoft.ReportViewer.WebForms` в `AutoCall.Logic.BLL.csproj:120` —
мёртвая ссылка без обращений в коде; реально используется только в `Web/Reports/*.aspx` (LogPatternReport, LogLoadUserCallsReport,
LogDataSourceReport, RecordsDataSourceReport) — админские отчёты AutoCall. Отчёты dispatch идут через Telerik Documents и iTextSharp.

**Фронт**: knockout + jQuery + bootstrap 3 + Kendo + TypeScript (`Scripts/Dispatch/**`, `Scripts/Main/Models/SetupPacketStep*`,
бандл `~/Bundle/Scripts/Dispatch` в `BundleConfig.cs:112`).

## Setup Packet

Мастер онбординга карриера, 15 шагов: 0 Confidant, 1 Carrier Registration Service, 2 Contact Information, 3 Questionnaire,
4 Agreement, 5 Insurance Instructions, 6 Producer Requirements, 7 Electronic W9, 8 Carrier Profile, 9 Equipment Profile,
10 References, 11 W8 Upload, 12 Supplier Diversity Information, 13 Your Documents, 14 Send Email (`SetupPacketViewModel.cs:28-57`).
Версионирование views — `DispatchService.GetViewNameByVersion` (`StepXX_VV.cshtml`: 04_02, 06_02, Agreement_02) и `GetFileByVersion`.
Флаги в `DatCompanyDto`: SetupPacketId/EmailSent/Sent/Completed/Accept/File, W9File. `SendSetupPacketAceeptEmailAsync`
(`DispatchService.cs:2871`) генерирует PDF (insurance instructions, W-9, setup packet) в `FilePath/{carrierId}` и шлёт письмо
(под `#if DEBUG` отключено). В App — кнопки `EditCarrierDialog.xaml.cs:377-499`, `OpenWebSetupPacketAction` (`DispatchActions.cs:1922`)
открывает `CarrierRegistration/Edit?CarrierId=&step=2`. Таблицы: `d_SetupPacket`, `d_SetupPacketContact`,
`d_SetupPacketFactoringCompany`, `d_SetupPacketInsuranceAgency`, `d_SetupPacketReference`.

## БД — таблицы `d_*` в `autocall` (EF6, Guid `EntityId`, soft delete)

d_ActualCompany, d_ActualCompanyCustom, d_CacheLoadIndex, d_CompanyCustomStatus, d_CustomerCompany, d_DatCompany, d_DatDriver,
d_Defect, d_DefectRepair, d_Driver, d_Event, d_HistoryLocationDatDriver, d_HistoryLocationDriver, d_HistoryLocationLoad,
d_HistoryLocationTrailer, d_HistoryLocationTruck, d_HistoryObject, d_Load, d_Message (не переносится), d_SetupPacket,
d_SetupPacketContact, d_SetupPacketFactoringCompany, d_SetupPacketInsuranceAgency, d_SetupPacketReference,
d_ShipperReceiverCompany, d_Trailer, d_Truck, d_Warehouse, d_Yard. Общие с AutoCall: Users, Roles, UserRoles, Company, Session,
UserVerify (`VerifyId` читается на каждом `Request`), City/State/Country.

`d_Event` (`Id int identity`, CompanyId, CreatedDate, Type, HistoryId) — журнал для long-polling; сервер читает `max(Id)` по
компании (`DispatchService.cs:130`). `d_HistoryObject` (`Id int`, `Data varbinary`, CreatedDate, ReferenceId, GroupId, Undo) — снимки
объектов для `LoadHistory*` и `UndoPreview/UndoApply`; на них ссылаются по `HistoryId` `d_Load`, `d_HistoryLocation*`, справочники.
Объёмы и отбор — [[data-migration]].

## Файлы

`FilePath\{EntityId}\имя` (`DispatchService.FileGetFullPath`, строка 2631): каталог по `EntityId` груза (документы), карриера
(PDF setup packet, W-9, подкаталог `Insurance` для COI) или setup packet. Имя уникализируется (`UniqueFileFullPath`), файлы после
записи не меняются; удаление — перенос в `RecyclePath\{EntityId}`. На проде `E:\Files`: 570 222 файла, 255 ГБ.
