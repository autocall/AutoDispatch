# Wiki Log

## [2026-11-11] ingest | старт wiki AutoDispatch V3

Создана база знаний нового репозитория по итогам анализа V1 (`D:\Develop\AutoCall`) и AutoCall V3 (`D:\Develop\AutoCallV3\Repo`).
Страницы: [[plan]] (этапы, оценка, решения), [[v1-inventory]] (что переносим), [[protocol]] (конверт `EventRequest`, reflection,
`TypeNameHandling`, long-polling), [[data-migration]] (замеры боевой БД и файлов, блоки скрипта), [[deploy]] (хост, контейнер, volume).
Решения пользователя 2026-11-11: переносить 7 компаний с событиями за 6 месяцев, `d_Event` для них переносить, `d_Message`
и `ChatService` не переносить, места под 147.5 ГБ файлов на хосте хватает. Установлено: `Microsoft.ReportViewer` к dispatch
не относится (`Web/Reports/*.aspx` AutoCall и `SimilarLoadsDialog` лоадборда).

## [2026-11-11] decision | решения этапа 0

Пользователь: ключи БД остаются Guid; поддомен `dispatch.myautocall.net`; свои БД `autodispatch` + `autodispatch_log`
на `db.myautocall.net`; своя Identity, переносятся только роли AutoDispatch (Agent, Warehouse, Dispatcher, Sales, Accounting,
Owner, Admin). Обновлены [[plan]] (таблица «Решить», этап 0 выполнен), [[deploy]], [[data-migration]], `CLAUDE.md`.

## [2026-11-11] build | этап 1 — скелет AutoDispatch Web

`Sources/Web` скопирован из AutoCall V3 (`AutoCall.Web.*` → `AutoDispatch.Web.*`), вырезаны Pattern/PreparedPattern/RecordFilter/
Email/DatSoap/Proxy/Settings/CallReport/Vodia и метрики AutoCall; `RoleEnum` = 7 ролей AutoDispatch; `UserSetting` оставлен для карты.
Созданы `Deploy/AutoDispatch.Web`, `Deploy/nginx/dispatch.myautocall.net.conf`, `server-setup.txt`, `Scripts/publish-autodispatch-web.ps1`,
`Scripts/commands.txt`, `DataBase/create autodispatch*.sql`, `create log indexes.sql`. Локальные БД `autodispatch_local` и
`autodispatch_log_local` созданы в SQL Express. Проверено: build, `/Test`, логин-страница, таблица `Log`, офлайн-тесты.
Ждёт пользователя: миграции `Init` (`AdDbContext`, `AdLogDbContext`), сборка фронта (Node), первая публикация. Страница [[web]].

## [2026-11-12] build | блок 0 переноса — компании и пользователи

Написан `DataBase/migrate from autocall dispatch.sql` (только блок 0): активные компании по `d_Event`, пользователи с ролями AutoDispatch и входом за 6 месяцев, все роли, постоянные таблицы маппинга `_MigrateCompany`/`_MigrateUser`. На боевой БД не прогонялся (нет доступа из сессии). [[data-migration]] дополнена.

## [2026-11-13] deploy | первая публикация AutoDispatch Web

`dispatch.myautocall.net` поднят: БД, nginx, certbot, контейнер `2026.11.13-2`, метрика CPU в cron веб-хоста. В [[deploy]] добавлен раздел
«Первая публикация» с граблями (`AllowedHosts` из V3, nvm под другим пользователем, фактическое место `metric_cpu.sh`). Обновлены
`Deploy/server-setup.txt` и шапка `Deploy/metric_cpu.sh`.

## [2026-11-13] decision | этап 2: Guid для Identity, UserVerify → IsDeleted, Users.Name → UserName

Решения пользователя записаны в [[plan]] («Решить»): ключи пользователей и компаний — Guid со старыми Id (контракт несёт Guid, маппинг не нужен);
`UserVerify` не переносится — статус = `IsDeleted`, как в V3, проверка на `Request` — этап 4; отображаемое имя — `UserName`.
Скелет [[web]] переведён на `IdentityUser<Guid>` (47 файлов, сборка и 8 офлайн-тестов ок, 3 HTTP-теста ждут сервер),
`DataBase/migrate from autocall dispatch.sql` блок 0 переписан без таблиц маппинга, [[data-migration]] обновлена.
Миграцию `Init` пересоздаёт пользователь. Открыто для этапа 2: `DbGeography` на Linux, справочники `City/State/Country`. Префикс классов `Ac` → `Ad` (AutoDispatch) переименован в коде и wiki.

## [2026-11-13] decision | этап 2: геоданные и справочники

Записано в [[plan]] («Решить»): `geography` → `Latitude/Longitude decimal(9,6)` (как в V3; радиус в `LoadHistory2` — гаверсинус в сыром SQL,
автокомплит — `PlaceService`/Safer); таблиц `City/State/Country` нет, `State`/`Country` — статический класс в коде с прежними Id для
`d_SetupPacket*`. Вопросов, влияющих на схему этапа 2, не осталось. Префикс классов `Ac` → `Ad` сделан в коде и wiki.

## [2026-11-13] ingest | этап 2: 28 сущностей dispatch перенесены в AutoDispatch.Web.Data

Новая страница [[data-model]]. Источник — V1 `Entities/Dispatch`, `Entities/Base`, `Mappings/Dispatch` (2084 строки). Перенесены как есть с
решениями дня: таблицы без `d_`, `ActualCompany` → `DispatchCompany` (+ `ActualCompanyId` → `DispatchCompanyId`), `geography` → decimal-пары,
без FK на IdentityUsers и State/Country, все FK NoAction, nvarchar как в V1. `DispatchMap` подключён в `AdDbContext` и `LinqToDbContext`,
тест `07_DispatchModel` (EF-модель без БД, пары навигаций, колонки LinqToDB) — 3 теста проходят. Обновлены [[plan]], [[data-migration]], index.
Дальше: миграция `Dispatch` (пользователь), публикация, блоки 1–5 скрипта переноса.

## [2026-11-13] deploy | публикация с миграцией Dispatch

Контейнер `autodispatch.web:6292491`: `Migrating AdDbContext` → `Seeding database` → старт, `/Test` ok. 28 таблиц dispatch созданы на проде.
EF предупреждал о decimal без типа (`DefectRepair.Cost`, `Load.CarrierViewTracking/LineHaul/DatRate`) — в миграции и так `decimal(18,2)`,
в сущностях тип задан явно, снапшот не меняется, новая миграция не нужна.

## [2026-11-13] ingest | блоки 1–6 скрипта переноса

`DataBase/migrate from autocall dispatch.sql` дописан: блоки 1–2 (объекты, грузы, история положений — списки колонок сгенерированы из миграции
`Dispatch`, переименования `ActualCompanyId`/geography подставлены), 3–4 (`HistoryObject`/`Event` пакетами, возобновляемо, `_MigrateHistoryKeep`),
5 (список каталогов файлов), 6 (включение FK с проверкой). На боевой БД не прогонялся. [[data-migration]] обновлена.

## [2026-11-13] query | проверка переноса на боевой БД через db-readonly

MCP `db-readonly` подключён к репозиторию (`.mcp.json`, `AI/mcp/db-readonly`, `.env` в gitignore); запросы дольше 15 с — через `sqlcmd` тем же логином.
Сверены колонки `d_*` ↔ `autodispatch`, FK активных компаний (3 висячие ссылки — блок 6 обнуляет), замеры, активность компаний, кандидаты-пользователи.
Решения в [[plan]]: признак входа = `Users.LastModifiedDate` или `Session` (16 пользователей), все строки dispatch — `varchar`. [[data-migration]], [[data-model]] обновлены.

## [2026-11-13] query | collation и перенос

Прогон блока 2 упал на `CacheLoadIndex.City` (`Québec…`): `autodispatch` в UTF-8, `varchar(N)` там в байтах. Скан строк `d_*`: вне ASCII — массово,
вне Latin-1 — 52 строки мусора. Вопрос collation записан в [[plan]] как открытый (рекомендация — `SQL_Latin1_General_CP1_CI_AS`, как у `autocall`),
решить до боевого прогона переноса; этап 3 от него не зависит. Также исправлен скрипт: `#active` создаётся один раз в шапке.

## [2026-11-13] ingest | этап 3 — серверная логика dispatch

Перенесён BLL V1 в `AutoDispatch.Web.Logic` (~11 тыс. строк): `DispatchService` 9 partial-файлов и `IDispatchService` (74 метода, без чата и
`ReportSimilarLoads` — решено не портировать), 15 сервисов, конвертеры + `DispatchMappingProfile`, `WebServices` (CarrierView, Bnsf, Safer, Bandwidth,
OpenAI), `Utils` (PDF, письма), хелперы, общий контракт `Sources/Shared/AutoDispatch.Dtos` с `Common/` из `AutoCall.Common`. В Server: профиль в
`Program.cs`, 4 `BackgroundService` вместо Quartz (список снят с `JobScheduler.cs`), `Resources/` из V1, каталоги `files/_recycle`, `files/_temp`,
новые ключи `Secrets` в примерах конфигов. `AdUser.Signature` добавлена (миграция — пользователь). Тест `08_DispatchLogic` (8) проходит, решение собирается.
Не реализовано: `ReportService.RtfToPdf` (Telerik, открыто). Новая страница [[web-logic]], [[plan]] — статус этапа 3 и строки «Решить»
(ReportSimilarLoads, iTextSharp LGPL, AdUser.Signature, PDF для OpenAI, кэш геокодинга, Telerik, Quartz).

## [2026-11-16] decision | RTF→PDF и iTextSharp по образцу Safer
RTF→PDF: Telerik Document Processing .NET Standard (`Telerik.Documents.Flow` + `Flow.FormatProviders.Pdf` 2022.1.217, локальный NuGet-источник
как в `D:\Develop\Safer\Source\Libraries\Telerik`) — установленные WPF DLL R1 2022 ссылаются на `PresentationCore`, на Linux не годятся; пакеты Flow
на машине отсутствуют, ждём от пользователя. iTextSharp: `iTextSharp.LGPLv2.Core` как в Safer, оставлена последняя бесплатная 3.8.7. [[plan]], [[web-logic]].

## [2026-11-16] code | RTF→PDF на Telerik Document Processing .NET Standard
Пакеты Flow + Flow.FormatProviders.Pdf 2022.1.217 (аккаунт Telerik, UI for WPF 2022.1.222) и Core/Fixed/Zip из Safer — в `Sources/Web/Libraries/Telerik`,
`Sources/Web/nuget.config`. `TelerikPdfHelper` (Logic): шрифты из `Resources/Fonts` (скопированы из Windows: Calibri ×4, Arial, Courier New, Symbol,
Wingdings; Brush Script MT уже был), PNG → JPEG через ImageSharp. Тест `Test09_RtfToPdf` на 3 шаблонах проходит на Windows; в Linux-контейнере не проверен.
Уточнено: в Safer RTF→PDF нет (Razor → DinkToPdf), оттуда взят только локальный источник пакетов. Найдено: Release-сборка Logic падает без лицензии
ImageSharp 4.x (проверка в `SixLabors.ImageSharp.targets`, в Debug пропускается) — publish не пройдёт. [[plan]], [[web-logic]].

## [2026-11-16] code | ImageSharp → SkiaSharp, шрифты Microsoft заменены на OFL
Проверена реализация `TelerikPdfHelper` (RTF→PDF): по PDF через PdfPig шаблоны setup packet реально используют Calibri (regular/bold/italic),
маркеры списков Arial «•», Courier New «o», Wingdings «▪», Symbol «•» и Brush Script MT — карта шрифтов верна, Times New Roman/Cambria не запрашиваются.
ImageSharp 4.x (ключ при Release) заменён на SkiaSharp 4.154.0 (как в Safer, уже был транзитивно) — `ImageHelper`, `JpegImageConverter`;
Release-сборка решения проходит. Шрифты Microsoft убраны из `Resources/Fonts`, вместо них Carlito ×4, Liberation Sans/Mono (+ `OFL-*.txt`);
Wingdings — `AppSettings.HostFontsPath` = `/files/_fonts` на хосте, иначе пустой маркер 3-го уровня. `Test09_RtfToPdf` ждёт Carlito, 11 тестов проходят.
[[plan]] (строки «ImageSharp», «Шрифты в репозитории», «Telerik Documents»), [[web-logic]], [[deploy]].

## [2026-11-16] code | шрифт подписи — DancingScript-Bold из Safer
`BRUSHSCI.TTF` (Brush Script MT, Monotype) заменён на `DancingScript-Bold.ttf` (OFL), которым подписывает документы Safer: W9 (`ReportService`),
confirmation (`DispatchPdfGenerator`), «Brush Script MT» в RTF-шаблонах (`TelerikPdfHelper`). В `Resources/Fonts` теперь только OFL-шрифты. [[plan]], [[web-logic]].

## [2026-11-16] verify | PDF в Linux-контейнере
`AutoDispatch.Web.Test` опубликован под linux-x64 и прогнан в `mcr.microsoft.com/dotnet/sdk:10.0` на `ububuilder` (одноразовый контейнер, удалён).
Найдено и исправлено: (1) параллельные `RtfToPdf` рушат статический `FontsRepository` Telerik — экспорт под `lock`; (2) managed SkiaSharp 4.154.0
не принимал `libSkiaSharp.so` 4.150.1 от PDFtoImage — явный `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.154.0 (как пин в Safer).
После этого 12/12 тестов (RTF→PDF трёх шаблонов, PDF→PNG через pdfium, превью SkiaSharp) проходят; PNG страницы визуально верный (Carlito, маркеры).
`ldd` на `aspnet:10.0` — зависимостей не хватает нет. [[plan]], [[web-logic]].

## [2026-11-16] decision | CacheLoadIndex не переносится
Скан строковых колонок `d_*` в `autocall` (не-ASCII и переполнение в байтах UTF-8): переполнение только в `d_CacheLoadIndex.Company`/`City` (61 строка) из-за
`SUBSTRING(…,1,16)`; в остальных 40 колонках не-ASCII (тире, апострофы, `Ö`, кириллица в именах файлов) влезает. Таблица была кэшем автокомплита
`SearchLoads`, читал её только он. Убраны сущность, `DispatchMap`, тест (27 типов), `LoadIndexCacheService`, вызовы в `DispatchService.Save`,
`HistoryCleanupBackgroundService`, блок 2 скрипта. `SearchLoads` на этапе 4 — прямой запрос по `Load`. Рекомендация по collation пересмотрена: оставить UTF-8. [[plan]], [[data-model]], [[data-migration]], [[web-logic]].

## [2026-11-17] implement | Этап 4: Api/Dispatch Request/Response
`ApiDispatchController` (Request, Response, RequestZip, ResponseZip), `DispatchRequestHandlers` (таблица `Name → handler` из `IDispatchService`,
Expression-делегаты, роли по `MinAccess`), `DispatchPollRegistry` (отмена предыдущего long-polling пользователя), whitelist `$type` в `MscorlibBinder`,
проверка `IsDeleted` на каждом запросе. Тест `09_DispatchApi` (3 теста) проходит против локального сервера с миграциями `Init/Dispatch/Alter_Dispatch`.
Открыто и вынесено в «Решить»: маршруты остальных эндпоинтов App (старые URL V1 vs маршруты V3 — противоречие плана и критерия этапа),
состав ответа логина (`roles`/`hasBnsfRailway`/`customStatuses` из OAuth V1 против `SessionDto`). Мёртвые вызовы клиента V1 без сервера: `api/DatCompany/SendSetupPocket`, `Api/Account/Check`. [[plan]], [[protocol]].

## [2026-11-17] implement | Этап 4: остальные эндпоинты App, SessionDto
Решения пользователя: маршруты V3 (App адаптируется на этапе 6), `SessionDto` расширен (`HasBnsfRailway`, `CustomStatuses`). Сделано:
`ApiDispatchHelpersController` (SearchLoads, SearchCustomers, PlaceAutocomplete, PlaceDetails, Distance, ExtractLoadFromPdf/2), `ApiDatCompanyController`,
`ApiActualCompanyController`, `ApiCensusController`, `ApiSetupPacketController`, `DispatchService.SearchLoadsAsync` (без `CacheLoadIndex`, компания через
`ShipperReceiverCompany`), статика `/Temp`. Найдено: camelCase `JsonExtension` переписывает ключи словарей — `Api/DatCompany/Search` сериализуется без него.
`09_DispatchApi` — 4 теста проходят (Distance локально 400: нет `GoogleMap:Api`). Таблица эндпоинтов — [[protocol]]; статус этапа — [[plan]].

## [2026-11-17] decision | Этап 5 разбит: публичный сайт без SetupPacket, SetupPacket — этап 6 на daisyUI + Stimulus
Решение пользователя: старую knockout-реализацию SetupPacket не тянуть, переписать сразу на daisyUI + Stimulus отдельным этапом 6 (до App).
Этапы 6–8 → 7–9 (`plan`, ссылки в `protocol`, `web`). Сверка этапа 5 с кодом V1 добавила в «Решить»: `Dispatch/Dispatcher` (веб-борд на карте,
~4 800 строк knockout, ссылок нет — рекомендация не переносить), карты для App (Bearer + `?t=` V1 против `/Map?r=` V3, `SelectLocation` парсит HTML,
EO.WebBrowser → WebView2 на этапе 7), публичные URL как в V1 + редирект с `myautocall.net` на этапе 8. [[plan]].

## [2026-11-17] decision | Этап 5: `Dispatch/Dispatcher` не переносится, карты App — по схеме `/Map` V3
`RouteLocation`/`RouteLocation2` заменяет готовый `/Map`; `SelectLocation` и `OrderStops` — новые страницы с токеном `r`, результат в App через `postMessage` WebView2 (этап 7). [[plan]].

## [2026-11-17] implement | Этап 5: Confirmation и карты для App
`ConfirmationController` (`/Confirmation/?LoadId=`, POST htmx, `Download/{id}`) + `Views/Shared/_PublicLayout`; `Map/SelectLocation` и `Map/OrderStops`
(`MapService`: `CreateSelectLocationLink`, `CreateOrderStopsLink`/`ParseOrderStopsLink`, `GetLocationAsync`, `GetOrderStopsAsync`, `GetStopsRouteAsync`;
`Api/Maps/SelectLocationLink|OrderStopsLink`; `select_location_controller.ts`, `order_stops_controller.ts`, `map_common.ts`). Найдено: `Expenses_DtoToVm`
терял Gains/Expenses после whitelist `$type` этапа 4 — исправлено, тест `08.Test10`. Тесты `10_PublicSite` (3) проходят против локального сервера с тестовыми
данными (компания, карриер, трейлер, 2 груза). [[public-site]], [[plan]], [[protocol]].

## [2026-11-17] implement | Этап 6: SetupPacket (CarrierRegistration) на daisyUI + Stimulus
`CarrierRegistrationController` (Index/View/Edit, Step00–14, Prev01–14, DownloadFileMC/COI), `PrintController`, модели шагов в Logic + маппинг,
`SetupPacketService.SaveStepFileAsync`, views 9 включённых шагов и партиалы, Stimulus `copy_fields`/`toggle`/`mask`/`w9`, шрифт подписи.
Отступления: выключенные шаги 3, 8–12 и views версии 1 не перенесены (вынесены в «Решить»), packet без карточки FMCSA создаётся без
предзаполнения. Тесты `11_SetupPacket` (2) проходят; попутно `09.Test03` — id события из конверта. Safer в этот день отдавал 502
(`02_Places`, `09.Test04` падали по внешней причине). [[setup-packet]], [[plan]].

## [2026-11-17] decision | Отступления этапа 6 приняты
Пользователь: выключенные шаги 3, 8–12 и views версии 1 не переносить; packet без карточки FMCSA создаётся без предзаполнения — оставить. [[plan]], [[setup-packet]].

## [2026-11-17] decision | Настройки пользователя App
Пользователь: перенести текущие настройки, разделить по секциям как в V3; `Settings.Binary` не переносить — только свойства dispatch.
Найдено при разборе этапа 7: App V1 хранил настройки через WCF `svc/SettingsService` (в инвентаре пропущено). Классификация 16 секций по коду V1,
шаг 0.5 блока 0 `migrate from autocall dispatch.sql` (проверен select'ом на `autocall`: 17 пользователей, 201 строка, JSON валиден).
Открыто: дефолты колонок/фильтров компании (`Company.DefaultColumnSettings/DefaultFilterSettings`, заполнено у 1 из 7). [[user-settings]], [[plan]],
[[data-migration]], [[protocol]], [[v1-inventory]].

## [2026-11-17] decision | Дефолты колонок и фильтров компании не переносятся
Пользователь: `Company.DefaultColumnSettings/DefaultFilterSettings` V1 не переносить — новые пользователи получают дефолты кода App. [[user-settings]], [[plan]].

## [2026-11-17] decision | Telerik WPF в App — R1 2022 WPF60, как в V3
Решение пользователя: не «текущие пакеты», а установленный R1 2022 `Binaries/WPF60` (2022.1.222) по `HintPath`. Сверка 26 DLL V1 (2021.1.223):
19 есть с теми же именами, 7 (Documents, FormatProviders.*, RichTextBoxUI) слиты в `Telerik.Windows.Controls.RichTextBox.dll` с теми же namespace. [[plan]].

## [2026-11-18] decision | DI и AutoMapper в App — как в AutoCall V3
Решение пользователя: MS DI с регистрацией конкретных классов без интерфейсов, все регистрации при старте; AutoMapper 12 только для
автоматических маппингов; конвертеры V1 — статические `XConverter.ToVm/ToDto` (F12 ведёт в код), без reflection. Отступление от V3 —
`App.Services` в UI (~60 мест: диалоги V1 создаются через `new`). Отвергнут фасад `ServiceLocator` + прослойка `TypeConverter`. [[plan]] «Решить».

## [2026-11-18] refactor | Сервер: конвертеры dispatch — статические классы, DispatchMappingProfile без reflection
По решению пользователя правится только `DispatchMappingProfile`: ~30 конвертеров `Mapping/Dispatch` → `XConverter.ToModel/ToDto`,
`BaseConverter.Fill*`, `ObjectConverter` для обобщённых методов `DispatchService`, `DispatchCrudService.ToDto/ToModel`; около 100 мест
`Mapper.Map<…>` → вызов конвертера. Сборка без ошибок, офлайн-тесты 27/27 (новый `Test11_ObjectConverterByType`); 12 HTTP-тестов
ждут запущенного сервера (миграции). [[web-logic]], [[plan]].

## [2026-11-18] query | Сканер TWAIN в App — использование по боевой БД
Схемы трейлеров сканировала одна активная компания (BEST AMERICAN): 2 454 схемы, 2020–2024, последняя 2024-09-01, после — ни одной.
`ScanEmail` у 2 пользователей. Сканы документов груза не отличить от загрузки. Вопрос открыт в [[plan]] «Решить» («Сканер TWAIN в App»).

## [2026-11-18] decision | Сканер TWAIN в App — переносится как есть
Решение пользователя: проверить использование у клиентов нельзя, сканер переносится как есть (TwainDotNet). Если под net10 или со сканером
клиента не заработает — переделать `ScannerDialog` на NTwain. [[plan]] «Решить», этап 7.

## [2026-11-18] decision | Сканер: ScanFileHelper.FindFile/RenameTempFile — закомментированными
Решение пользователя: мёртвый код `FindFile` и `RenameTempFile` и вызов в `DispatchActions` переносятся закомментированными с пометкой;
`DeleteOldTempFiles` остаётся. [[plan]] «Решить» — «Сканер TWAIN в App».

## [2026-11-18] lint | Предел загрузки файлов закрыт по коду
App V1 ограничивает файл 20 МБ (`FileSizeHelper.Check20MBSize`), эндпоинты App на сервере с `DisableRequestSizeLimit`, nginx `100m` — достаточно.
Пометка «уточнить» снята в [[deploy]] и [[web]].

## [2026-11-18] decision | Перенос: Company — все активные компании, в том числе без пользователей
Блок 6 скрипта переноса падал на `FK_Truck_Company_CompanyId`: UA LOGISTICS INC (`9B33CE4A…`) в `#active` (2 события за 6 мес), но без пользователей
allowed, поэтому не попала в `#company`, а её Truck/Driver/Trailer/Load перенеслись. Решение пользователя: переносить компанию без пользователей —
шаг 0.3 берёт `Company` по `#active`. [[data-migration]]

## [2026-11-18] decision | Критерий этапа 7 — проверка на хосте (вариант Б)
Решение пользователя: проверять на `autodispatch` на `db.myautocall.net`, куда перенесены данные 2026-11-18. Проверено через MCP: доступ есть,
количество строк 12 основных таблиц совпадает с `autocall`, FK доверенные, 17 пользователей с ролями и настройками. V1 продолжает работать
(снимок 10:13 UTC). [[plan]] «Решить», [[data-migration]] «Пробный перенос на хосте».

## [2026-11-18] decision | Догон переноса перед сверкой этапа 7; скрипт под догон, файлы
Решение пользователя: перед сверкой догнать перенос; файлы не синхронизированы. Скрипт `migrate from autocall dispatch.sql` поправлен под повторный
запуск: окно `#active` зафиксировано с 2026-05-18 (иначе UA LOGISTICS выпала бы 2026-12-08), блок 0 не удаляет `Company` (FK данных после блока 6
не дали бы удалить) и чистит Identity-claims/logins/tokens, блок 3 дополняет `_MigrateHistoryKeep` и докачивает пропущенные снимки ниже `max(Id)`,
итог блока 3 выводит `maxId`, шаг удаления записей нового сервера (закомментирован), комментарий блоков 1–5 «не прогонялись» заменён.
Блок 5 брал только Load/DatCompany/SetupPacket — теперь все сущности с каталогами (Driver, Truck, Trailer, Defect, DefectRepair, DispatchCompany),
из `autodispatch`, Guid в нижнем регистре. Новый `Scripts/sync-dispatch-files.ps1` (Full пачками с возобновлением, Diff с `-DeleteExtra`).
Правки скрипта и синхронизация не выполнялись. [[data-migration]] «Догон», «Файлы»; [[plan]] этап 7.

## [2026-11-18] update | sync-dispatch-files.ps1 — параметры -Tar/-Ssh
На сервере V1 нет `tar` (Server 2016 или старше). Добавлены пути к портативным bsdtar и Win32-OpenSSH; прогон с заглушкой ssh локально:
каталог `AAAA…` на диске уходит в архив как `aaaa…`, строки-заголовки `dirs.txt` отбрасываются, повторный запуск пропускает залитые пачки. [[data-migration]] «Файлы».

## [2026-11-19] decision | Документы — весь E:\Files через FileZilla
Решение пользователя: документы переносятся целиком FileZilla (подключения есть), а не скриптом по списку — на сервере V1 нет `tar`/`ssh`.
Хост: свободно 765 ГБ из 878, в `/home/autodispatch/files` пока только `_recycle`, `_temp`. После заливки — каталоги в нижний регистр (команда в
[[data-migration]] «Файлы»), досинк — `robocopy /MAXAGE` + FileZilla. `sync-dispatch-files.ps1` помечен «не используется». Обновлены
[[data-migration]], [[deploy]], [[plan]] («Решить»), `Deploy/server-setup.txt`, хаб.

## [2026-11-19] update | Документы: FileZilla в /home/safer/temp, затем copy-dispatch-files.sh
Уточнение пользователя: FileZilla кладёт весь `E:\Files` в `/home/safer/temp`, оттуда — в `/home/autodispatch/files`. Новый `Scripts/copy-dispatch-files.sh`
(на хосте под root): rsync всех каталогов-Guid, имена в нижнем регистре, root 755/644, повтор докопирует только новое (досинк), `--check dirs.txt`
(блок 5) — список отсутствующих. Проверен на хосте на тестовых каталогах в `/tmp` (регистр, права 600 → 644, пробелы в именах, дельта), тест удалён.
Хост: `temp` и `files` на одном `/dev/md2`, свободно 765 ГБ — две копии по 255 ГБ помещаются; `temp` никто не чистит. `temp` удалить после
переключения. Ручная команда `mv` в нижний регистр в [[data-migration]] заменена скриптом; обновлены [[plan]], [[deploy]], `Deploy/server-setup.txt`, хаб.

## [2026-11-19] decision | Софтфоны в App — переносятся AutoPhone и AutoPhone v.2, мёртвый код — закомментированным
Звонок по клику — часть dispatch-диалогов (колонка Phone бордов, справочники, карточка карриера, контакты груза/карриера/брокера), шёл через
софтфон модуля AutoCall. Решение пользователя: перенести те, что есть в AutoCall V3 — AutoPhone (MicroSIP), AutoPhone v.2 (`AutoPhoneV2`, Abto),
None, и окно Settings только с вкладкой Call Services. Сервер: `AdUser.SipLogin/SipPassword` + `SessionDto` (миграция — пользователь); скрипт
переноса — SIP в шаге 0.4, секция `CallServiceSettings` в 0.5 (13 из 17 пользователей, JSON проверен на боевой). Мёртвый код V1 —
`EditBaseLoadControl.Phone_OnMouseUp`, `PhoneControl`, DAT rate (`GetHybridRate` на сервере V1 всегда `DISABLED`) — закомментированным с пометкой.
[[plan]] «Решить» — «Софтфоны в App», [[user-settings]], [[data-migration]].

## [2026-11-19] ingest | Этап 7 — клиент AutoDispatch App перенесён (код)
`Sources/App/AutoDispatch.sln` (.NET 10, x86): AutoCall.Common, AutoDispatch.Data/Logic/Logic.Contracts, AutoPhoneV2, AutoCall.WPF (`AssemblyName = AutoDispatch`).
Unity → MS DI без интерфейсов, AutoMapper — только автоматические пары, остальное статические конвертеры; data-сервисы на `BaseApi` V3, один вход
`Api/Account/SignIn`; `SettingsService` по секциям (сервер `ApiSettingsController` + `UserSettingService.GetAllJsonAsync/SetJsonAsync`); Serilog +
`ServerLogSink`; карты — WebView2; софтфоны MicroSIP/Abto; ClickOnce x86 + `Scripts/publish-autodispatch-app.ps1`. Данные модуля AutoCall,
которыми пользовался dispatch, заменены (коды оборудования и зоны штатов — статические таблицы с `autocall`, поиск городов — Places).
Мёртвый код V1 закомментирован с пометкой. Сборка 0 ошибок, App до входа, AutoMapper и `SettingsService` проверены; вход ждёт миграции
`AdUser.SipLogin/SipPassword`. Новая страница [[app]]; обновлены [[plan]], [[user-settings]], [[protocol]], [[deploy]], [[index]].

## [2026-11-19] fix | New Driver Application Package.pdf — назначение и адрес
Исправлено описание в [[deploy]]: PDF — бланк для нового водителя по ссылке в справочнике Drivers App, а не вложение писем setup packet.
Адрес в `DriversDialog` — `{WebUrl}Download/…` вместо `//Download` V1 (Kestrel с `RequestPath /Download` его не отдаёт). [[app]].

## [2026-11-19] decision | App: сертификат ClickOnce из V3, проекты не переименовываются
Решение пользователя: `AutoCall.Common` и `AutoCall.WPF` остаются с именами V1; подпись ClickOnce — сертификатом AutoCall V3
(`AutoCall.Publish_3_TemporaryKey.pfx`, `CN=AutoCall`, в `.gitignore`), отпечаток записан в `ClickOnceProfile.pubxml`; пробный `msbuild /t:Publish`
подписал манифест. Папка `Resources/phone` совпадает с V1/V3 и вся относится к двум софтфонам — не менялась. [[deploy]], [[app]], [[plan]].

## [2026-11-19] fix | App: иконки RichTextEditor под Telerik R1 2022
`XamlParseException` при создании `RichTextEditor` — `images/msoffice/16/new.png` не найден: в WPF60 R1 2022 ресурсы бывшего `RichTextBoxUI` лежат
в `Telerik.Windows.Controls.RichTextBox.dll` с префиксом `RichTextBoxUI/`. Пути `IconSources` исправлены, все 185 иконок есть во всех трёх темах. [[app]].
Стартовые проекты: в `AutoDispatch.sln` и `AutoDispatch.Web.sln` первыми стоят `AutoCall.WPF` и `AutoDispatch.Web.Server`.

## [2026-11-19] ingest | Локальная копия одной компании для проверки App
`DataBase/copy company to local.sql`: компания пользователя `@email` из пробного `autodispatch` (linked server, `OPENQUERY`) в `autodispatch_local`,
пароль от локального admin, повторный запуск заменяет копию. Синтаксис проверен `parseonly`, фильтры — через MCP на LEVSKI INC; сам скрипт не запускался
(чтение linked server из сессии Claude запрещено). [[data-migration]] «Локальная копия компании».

## [2026-11-19] decision | локальная копия по умолчанию — тестовый пользователь Anton Gmail (DDM Logistics)

`copy company to local.sql`: `@email` по умолчанию сменён на тестового пользователя (Owner DDM Logistics); объёмы DDM записаны в [[data-migration]].

## [2026-11-19] fix | copy company to local.sql — конфликт collation

`#rcol` создавалась в collation сервера (`SQL_Latin1_General_CP1_CI_AS`), `autodispatch_local` — `Latin1_General_100_CI_AI_SC_UTF8`; колонки временной таблицы теперь `collate database_default`.

## [2026-11-20] fix | App: XamlParseException OnWindowLoaded при запуске

`MainWindow.xaml` ссылался на удалённые с модулем AutoCall `OnWindowLoaded` (горячие клавиши) и `Window_Activated` (подсветка иконки) — атрибуты убраны. Корневые события всех XAML проверены скриптом, других висячих нет; App собирается. [[app]]

## [2026-11-20] decision | кэш адресов геокодинга в БД (GeocodeAddress) + перенос из Cache V1

App висел на Loading: LoadLoads DDM (~3 500 активных грузов, 3 173 в Canceled1, ~2 200 точек) по одному запрашивал адреса у Safer (~160 мс), кэш в памяти обнулялся перезапуском, 100 с клиента не хватало; Safer часть точек отдаёт 500 (дубль ключа ResultGoogle/QueryLatLngGoogle на его стороне). Решение 2026-11-13 пересмотрено для адресов: таблица `GeocodeAddress`, `GeocodeService` — память → таблица → Safer по 8 параллельно, одинаковые точки одним вызовом; блок 7 скрипта переноса — адреса из `Cache` (формат ключа проверен на данных, UTF-16 → UTF-8 без потерь проверено локально). Web собирается; миграция — пользователь; блок 7 не прогонялся. [[plan]], [[data-migration]], [[data-model]], [[web-logic]]

## [2026-11-20] fix | проверка App под DDM: создание груза, статусы, инвойс

Прогон App через UI Automation (локальная копия DDM). Создан груз 9086 (Carrier, Chicago → Dallas, confirmation загружен в files/{EntityId}), назначен водитель. Найдено и исправлено: (1) App — нет пары AutoMapper `LocationDto ↔ LocationViewModel`, из-за неё молча не работали подсказки Origin/Destination окна груза (`QueryLoadDto → QueryLoadViewModel`) и поиск Carrier History (`CarrierHistoryViewModel → RequestHistory2Dto`); остальные пары проверены прогоном всех TypeMap. (2) Web — `_modifyLocationAsync` принимал `BaseDispatchEntity`, LinqToDB `UpdateAsync` падал «no fields to update» при любой смене положения (At The Pick Up и др.); сделан generic. (3) Web — `Api/ActualCompany/List` отдавал вариант веб-грида без шаблонов confirmation/BOL/invoice, LogoBig и Customizes (V1 для WPF — `SearchForAsync`, полный); инвойс в App падал `StringBuilder.Remove(-1)`; теперь `SearchFor2Async`. Не ошибки: `confirmation.rtf` UNITED GLOBAL — документы не скопированы локально; Distance не считается после ввода Destination — как в V1. [[app]], [[web-logic]]

## [2026-11-20] ingest | документы для локальной копии — Scripts/pull-local-files.ps1

Скачивание с хоста только каталогов {EntityId} строк autodispatch_local (запрос блока 5) из /home/safer/temp/Files в FilesPath локального Web, tar | ssh пачками, докачка; прямое подключение каталога хоста к отладке отклонено — локальный сервер пишет в FilesPath. Скрипт не запускался. [[data-migration]]

## [2026-11-20] decision | локальные документы — E:\AutoDispatch\Files

`FilesPath` в локальном AppSettings.Environment.Local.cs (gitignored) и `-Target` по умолчанию в Scripts/pull-local-files.ps1 — `E:\AutoDispatch\Files\` (решение пользователя); прежнее содержимое D:\Develop\AutoDispatch\Data\files скопировано туда. Шаблон .example не менялся. [[data-migration]]

## [2026-11-20] ingest | страница app-testing — журнал ручной проверки App

Сессия 1 (до перезапуска) перенесена из лога в [[app-testing]]; дальнейшие проверки пишутся туда.

## [2026-11-20] query | проверка App, сессия 4

Distance с новым ключом работает; Send Invoice не уходит — у DDM LOGISTICS INC нет пароля SMTP (и в V1), у ящика ddmtest2019@gmail.com Gmail отклоняет пароль (535, нужен пароль приложения) — данные; BOL/confirmation брокера, справочники, Loads History, Settings, Map, Show Trip, View History, Edit Load, Broker Accounting — работают. Итог и открытые пункты — [[app-testing]].

## [2026-11-20] query | бланк водителя, SMTP компаний, сканер

New Driver Application Package.pdf скопирован из V1 Web/Download в локальный DownloadPath (ссылка Drivers — 200). Найден пробел: в V3 нет интерфейса настройки компаний dispatch (SMTP, шаблоны, логотипы, нумерация) — в V1 админская страница ActualCompany/Index, не перенесена; вопрос пользователю. Сканер не проверить — нет устройств. [[app-testing]]

## [2026-11-20] ingest | страница Dispatch Companies (V1 ActualCompany/Index) для Admin и Owner

Решение пользователя: перенести. `/Admin/DispatchCompanies` — `AdminDispatchCompaniesController`, views `AdminDispatchCompanies/*`, `DispatchCompanyFormConverter`, Stimulus `board`/`rows`, `LogoGenerator` (SkiaSharp вместо System.Drawing), дефолтные логотипы в `wwwroot/images/dispatch`; `DispatchCompanyService.HasPasswordsAsync` и удаление убранных Customizes, null-safe пароль в тестовом письме. Отличия от V1 (пароли не выводятся, Test SMS через Bandwidth) и локальная проверка под временными Owner/Admin — [[dispatch-companies]]. Переименование таблиц `d_ActualCompany` → `DispatchCompany`, `d_ActualCompanyCustom` → `DispatchCompanyCustom` повторено на странице и в решении. Обновлены [[plan]] (решение, этап 5), [[app-testing]] (пункт «Открыто» закрыт), [[index]].

## [2026-11-20] query | Send Invoice после настройки SMTP на странице Dispatch Companies

Test SMTP с формы ушёл (пароль приложения рабочий), но Save не сохранил: поле получателя теста `type=email` с «test test» блокировало отправку формы браузером. Поля теста — `type=text` (адрес проверяет сервер), ошибки формы — списком сверху. Send Invoice 9086 снова не ушёл — у DDM прежний SMTP. [[app-testing]] сессия 5, [[dispatch-companies]].

## [2026-11-20] query | проверка App, сессия 5: Send Invoice, Undo, Parse Confirmation, Generate Load, Split

Send Invoice с SMTP nata.kachina88@gmail.com ушёл, Undo откатил. Исправлено: `MscorlibBinder` (внешнее имя сборки оставалось `System.Private.CoreLib`, тест ужесточён), null в `ExpensesConverter.ToDto`/Gains (Split в App падал), маска пароля на странице Dispatch Companies, обрезка пробелов в SMTP-адресах. Ключ OpenAI в локальном `appsettings.Development.json` был с обёрткой `@"…"` — убрана. [[app-testing]] сессия 5, [[dispatch-companies]].

## [2026-11-22] query | проверка App, сессия 6: Split, confirmation, Factoring, Issues/Bond/Paid, комментарии RadDocument

Split, Resend/Edit Confirmation (только на nata.kachina88@gmail.com), Factoring Company, Send Invoice на factoring, Issues → Bond → Paid, фильтры Accounting — работают. Исправлено: колонки комментариев пустели после сохранения груза в App V3 — Telerik V3 пишет RadDocument с `assembly=Telerik.Windows.Controls.RichTextBox`, XPath искал только namespace V1; теперь namespace берётся из документа (`LoadViewModel`, `DatCompanyViewModel`). Parse Confirmation: ключ OpenAI тот же, что в V1, и отклонён — нужен действующий. Post Load в V3 не подключён (модуль AutoCall) — решить. [[app-testing]] сессия 6.

## [2026-11-22] decision | Post Load убран из App, Parse Confirmation оставлен не допроверенным

Post Load (V1 — переход в лоадборд DAT, модуль AutoCall) в V3 не подключён — пункты меню, команда и действие удалены. Parse Confirmation оставлен как есть: ключ OpenAI отклонён, разбор целиком проверяется только на сервере (OpenAI забирает картинки по URL сайта). Модель и температура OpenAI остаются константами `AppSettings.General` (как V1). [[app-testing]] 6.1, 6.11.

## [2026-11-22] query | проверка App, сессия 6 (продолжение): CarrierView, setup packet, окно карриера

Sync Tracker работает (только чтение CarrierView); Enable/Disable не протестированы — нет тестового номера. Найдено: webhook CarrierView `POST /Dispatch/UpdateLocation` V1 не перенесён и пропущен в инвентаризации — решить. Email Setup Packet ушёл только на nata.kachina88@gmail.com. Исправлено: Edit Carrier/Broker спрашивал о сохранении без правок — окно пересохраняет комментарий в формате Telerik V3, `HasChanges` теперь сравнивает текст. Комментарии в колонках после перезапуска App видны (6.3). Схема трейлера — как V1. [[app-testing]] 6.12–6.19.

## [2026-11-22] decision | webhook CarrierView перенесён

V1 `DispatchController.UpdateLocation` → `CarrierViewController` (`POST /Dispatch/UpdateLocation`, без логина, ответ как V1) + `DispatchService.CarrierViewUpdateLocationAsync`; `CarrierViewStatusesJson` с `$type` как в бою, Sync выровнен. Проверено на копии сервера тестовым телом. При переезде сменить адрес webhook в настройках CarrierView — [[deploy]] «Переключение с V1», [[app-testing]] 6.14, [[v1-inventory]].

## [2026-11-23] query | проверка после перезапуска: ссылка setup packet, webhook, тесты

Ссылка регистрации карриера из письма открывается (формат V1, на сервере — домен dispatch.myautocall.net). `/Dispatch/UpdateLocation` отвечает на запущенном Web. Тесты `DispatchLogic`+`DispatchApi`+`PublicSite` 20/20. Проверка Edit Carrier без вопроса о сохранении (6.17) не выполнена: окно RDP-сессии не принимает мышь и снимки экрана, строку грида через UIA не выбрать. [[app-testing]].

## [2026-11-23] query | Edit Carrier: вопрос о сохранении — вторая причина из V1

После перезапуска App вопрос «save the changes» у PINA TRUCKING LLC остался: окно при каждом открытии добавляет телефон из FMCSA (формат «(xxx) xxx-xxxx» против цифр в контактах) — так же в V1, в боевой базе у карриеров копятся одинаковые телефоны. Решение — за пользователем. [[app-testing]] 6.17.

## [2026-11-23] decision | дубли телефонов карриеров: исправление App и чистка при переносе

`EditCarrierDialog` сравнивает телефон/факс из FMCSA после того же конвертера, что и при сохранении — не добавляет его при каждом открытии. В блок 1 скрипта переноса добавлена чистка точных повторов `DatCompany.Contacts` (OPENJSON во временную таблицу + STRING_AGG); на локальной копии 8281 → 4368 контактов. [[app-testing]] 6.17, [[data-migration]].

## [2026-11-23] query | Edit Carrier закрывается без вопроса — проверено

После перезапуска App: PINA TRUCKING LLC и MSD TRUCKING INC (комментарий V1, телефон из FMCSA) закрываются без «save the changes», в БД ничего не записано. [[app-testing]] 6.17.

## [2026-11-23] query | All Carriers: поиск работает, ссылки FMCSA с машины разработки — 403

Поиск census проверен пользователем. safer.fmcsa.dot.gov и ai.fmcsa.dot.gov отвечают 403 (AWS ELB) на любой запрос с машины разработки (Франкфурт, AWS) — блокировка по адресу, не ошибка ссылок; шаблоны совпадают с V1. [[app-testing]] 6.18.

## [2026-11-23] query | setup packet по ссылке: регистрация и Accept — три исправления

Прошёл регистрацию карриера по HTTP (STRAIGHT THROUGH — packet из V1, BH GLOBAL — новый) и Accept в App: письмо с setup packet и W-9 ушло на nata.kachina88@gmail.com. Исправлено: `ConfidantContactId` в форме шага 0; `AccessDenied`, когда создателя карриера нет среди перенесённых пользователей; генерация W-9 на iTextSharp.LGPLv2.Core. Тест `11_SetupPacket` усилен, 2/2. [[app-testing]] 6.20–6.23, [[setup-packet]].

## [2026-11-23] decision | форма W-9 Rev. March 2024 из Safer, поле 3b в контракте

`fw9.pdf` заменён на `FormW9.pdf` Safer, текст шага 7 — по Safer Step06, `GenerateW9Pdf` переписан под новые имена полей (SSN/EIN на уровне Page1 — в Safer, похоже, заполняются по старым путям). Поле 3b `W9ForeignPartnersHas` добавлено в `SetupPacketDto` (решение пользователя — исключение «контракт не меняется»), сущность, конвертеры сервера и App, тест `11_SetupPacket`. Нужна EF-миграция пользователя. [[plan]] «Форма W-9 2024», [[setup-packet]], [[protocol]], [[data-model]], [[data-migration]], [[app-testing]] 6.24.

## [2026-11-23] query | W-9 2024 после миграции: тесты и Accept

Миграция пользователя `20261123092555_Alter_SetupPackage` добавила `SetupPacket.W9ForeignPartnersHas`. Тесты Web 22/22; Accept в App (STRAIGHT THROUGH, LLC+P, 3b) — письмо с setup packet и W-9 новой формы ушло на nata.kachina88@gmail.com, PDF проверен просмотром. [[app-testing]] 6.24.

## [2026-11-23] query | setup_packet.pdf: обрезанные заголовки контактов — ошибка V1, исправлена

Пользователь заметил обрезанные заголовки на странице Contact Information setup packet. Та же ошибка в PDF V1: `FillRtfTemplate` удалял маркеры MARKER1..4 до вырезания секций и затем резал по устаревшим позициям. Исправлено, тест `Test12_SetupPacketContactSections`. [[app-testing]] 6.25.

## [2026-11-23] query | setup packet: рамки, заголовок Accounting, W-9 3b и Requester, шаг 13 View

Рамки таблиц PDF (стиль Table Grid, Telerik рисует, WPF V1 нет) убраны в `FillRtfTemplate`; заголовок секции AfterHour в шаблонах — «Accounting Contact Information»; на шаге 7 галочка 3b после текста, Requester в строке с 5–6; шаг 13 в просмотре без полей файлов. Тесты дополнены, 24/24. [[app-testing]] 6.26.

## [2026-11-23] query | W-9 шаг 7: 3b, пункт 4, сообщения о незаполненных полях как в V1

Галочка 3b в конце текста, подписи и списки пункта 4 с новой строки (daisyUI 5 без `form-control`). «Заполните это поле» — подсказка браузера; по решению пользователя формы регистрации `novalidate`, сообщения сервера под полем, как в V1; Business name обязательный, как в V1. Обновлены [[setup-packet]] (замена инструментов, раскладка шага 7) и [[app-testing]] 6.27.

## [2026-11-23] decision | W-9: SSN или EIN, не оба — отступление от V1

V1 принимал оба номера; по инструкции IRS (Part I — один TIN) и как в Safer шаг 7 теперь отклоняет оба сразу. Записано в [[plan]] (решения), [[setup-packet]], [[app-testing]] 6.28.

## [2026-11-23] query | шаг 13: поля загрузки файлов — daisyUI file-input-ghost

По просьбе пользователя поля File MC / File COI шага 13 — `file-input file-input-ghost` вместо `file-input-bordered` ([[setup-packet]]).

## [2026-11-23] query | App: документы не открывались — UseShellExecute в .NET 10

`Process.Start` файла без `UseShellExecute = true` (в .NET 10 по умолчанию false) — 13 мест открытия файлов в App исправлены, [[app-testing]] 6.29.

## [2026-11-23] decision | W-9 2024 — версия setup packet 3, старые packet'ы на форме V1

По замечанию пользователя W-9 2024 — только для новых packet'ов через версию packet'а (как `_02` в V1): `SetupPacketVersion = 3`, view, шаблон и заполнение PDF по версии. Обновлены [[plan]] «Форма W-9 2024», [[setup-packet]], [[app-testing]] 6.30.

## [2026-11-23] fix | Регистрация: «same as» по колонке полей, дата подписи Agreement по умолчанию

«same as» в контактах (шаг 2) и агентствах (шаг 6) — над полем, в колонке First & Last Name / Company Name; дата подписи шага 4 — сегодня с первого показа (V1 `moment(null)`), поле только для чтения, как в V1. [[app-testing]] 6.31.

## [2026-11-24] decision | App: убрана мигающая метка «new» у кнопок My Carriers

По решению пользователя `new.gif` (`IsNewDatCompany` — есть завершённые, но не принятые setup packet'ы) снят с кнопок My Carriers в `DispatchBoard` и `DispatchAccountBoard`; App собирается. [[plan]] «Метка «new» у My Carriers», [[app-testing]] 6.32.

## [2026-11-24] decision | App: метка «new» у My Carriers остаётся, как в V1

Решение предыдущей записи отменено пользователем: удаление `new.gif` откачено, строка в [[plan]] убрана, [[app-testing]] 6.32 обновлена.

## [2026-11-24] test | Setup packet: Accept v2/v3, повторная отправка, Workers Compensation «No», Insurance Instructions

Проверено через `Api/Dispatch` на копии сервера: W-9 по версии packet'а, переход v2→v3 с шага 4, возврат на шаг 6, все письма (в том числе Insurance Instructions) — на тестовый адрес. Найдено: Release-сборка тестов и сервера берёт боевой `Website` — записано в [[app-testing]] «Окружение». [[app-testing]] 6.33.

## [2026-11-24] fix | Test01 SetupPacket: скрытые Id контактов/агентств и даты — как браузер

Пустые контакты в setup_packet.pdf тестового карриера — тест не отправлял скрытые `*ContactId`/`*InsuranceAgencyId`, `W9AgreeDate`, `WorkersCompensationSignatureDate`; регистрация в браузере не затронута. Тест исправлен, 6.33 уточнена. [[app-testing]] 6.34.

## [2026-11-24] fix | Снимок setup packet для App — с контактами и агентствами (Accept давал «error»)

`SaveWithHistoryAsync` писал снимок из сущности LinqToDB без связей (в V1 их подтягивал EF); App обновлял packet из события и терял `WorkersCompensationInsuranceAgency` — Accept после правки в Web падал. `SaveSetupPacket` перезаписывает снимок со связями. [[app-testing]] 6.35.

## [2026-11-24] test | Брокерские борды: полный цикл брокерского груза в App

Broker Dispatch → Broker Accounting на тестовых грузах 13170/13171: Assign/Unassign Carrier, Create/Send/Delete Confirmation, BOL, статусы, Attach Invoice/Check, Paid/Unpaid, canceled, Undo, Delete — ошибок нет; письмо — только на тестовый адрес. [[app-testing]] сессия 7.

## [2026-11-24] test | Брокерские борды: Line Haul, Expenses, Accessorial, Change Location, Copy Summary, Export

Тестовый груз 13172: правки ставок и расходов (форматы JSON как V1), выбор места на карте и история мест, сводка в буфер, выгрузка в Excel — ошибок нет; груз удалён. [[app-testing]] 7.11–7.17.

## [2026-11-24] change | Карты App в едином виде: SelectLocation и OrderStops как /Map

По замечанию пользователя: карточка места не наезжает на поиск (раскладка /Map), панель «Map options» (спутник, Traffic, Weather, тема) — общий `_MapOptionsPart` + `map_options_controller.ts`, координаты — Copy и Google Maps (в App — системный браузер). [[public-site]] «Единый вид карт», [[app-testing]] 7.18.

## [2026-11-24] change | Карта выбора места: без Copy/Google Maps и строки ссылки

По решению пользователя кнопки координат убраны (App снова без перехвата NewWindowRequested), строку ссылки как у Show Trip не добавлять; раскладка и Map options остаются. [[public-site]] «Единый вид карт».

## [2026-11-24] change | Перенесена страница Factoring Companies (`/Admin/FactoringCompanies`, только Admin)

Справочник факторинговых компаний для шага 2 setup packet — V1 `FactoringCompany/Index`: список с поиском и сортировкой, добавление, правка, удаление; доступ только Admin (решение пользователя; в V1 страница была без авторизации). [[setup-packet]], [[plan]], [[app-testing]] 7.19.

## [2026-11-24] change | Перенесена страница Custom Statuses; имена статусов при входе App

`/Admin/CustomStatuses` (Admin, Owner — как V1) на перенесённом `CustomStatusService`. Найдено и исправлено: `SessionDto.CustomStatuses` уходил в App без имён (`BaseDto` — `[DataContract]`, у `Name/CompanyId` не было `[DataMember]`) — по решению пользователя атрибуты добавлены в контракт. [[dispatch-companies]], [[plan]], [[protocol]], [[app-testing]] 7.20.

## [2026-11-24] fix | App: ссылка на Custom Statuses в окне фильтров — на новый сайт

`FiltersDialog` открывал V1 `https://myautocall.net/CustomStatus`; теперь `Settings.WebUrl` + `Admin/CustomStatuses`. [[app-testing]] 7.20.

## [2026-11-24] fix | App: ссылки окна новой версии — на dispatch.myautocall.net

`NewVersionDialog` вёл на `v3.myautocall.net/Download/setup.exe` и `AutoCall.application` (AutoCall V3) — теперь `dispatch.myautocall.net/Download/setup.exe` и `AutoDispatch.application`. Окно в App не открывается (вызов убран с модулем AutoCall), текст — про AutoCall V2/DAT. Ссылки Settings (DeskPins, ABTO VoIP SDK, Vodia, расширение Chrome) — обработчики без разметки и в V1, на экране их нет; не трогались. [[app]]

## [2026-11-24] change | App: удалены NewVersionDialog и неиспользуемые ссылки Settings

По решению пользователя: `NewVersionDialog` (AutoCall V1 — переход пользователей на AutoCall V2, в AutoDispatch не открывался) удалён вместе с правкой его ссылок из предыдущей записи; из `SettingsDialog` удалены обработчики без разметки (Vodia, расширение Chrome, DeskPins, ABTO VoIP SDK) и ненужный `using System.Diagnostics`. App собирается. [[app]]

## [2026-11-24] fix | App: AutoPhone v.2 не запускался в Debug-сборке

`PhoneForm.ConfigurePhone` (V1) возвращал false под `#if DEBUG` — софтфон Abto не создавался, ошибок нет. Заглушка убрана, как в AutoCall V3; ошибки настройки телефона пишутся в лог. [[app-testing]] 7.21.

## [2026-11-24] fix | App: заглушка AutoPhone v.2 для Debug возвращена

Без `#if DEBUG return false` App из VS (F5) падал при старте `0xc000041d` — AutoPhone v.2 выбран в настройках и стартует вместе с App. Заглушка V1 возвращена с предупреждением в логе; AutoPhone v.2 проверять Release-сборкой без отладчика. [[app-testing]] 7.21.

## [2026-11-24] fix | App: AutoPhone v.2 — ContextMenu → ContextMenuStrip (причина падения по F5)

Падение `0xc000041d` вызывал не отладчик, а `new ContextMenu()` в `SIPPhone_Load` (V1): в .NET 10 WinForms бросает `PlatformNotSupportedException`. Как в AutoCall V3 — `ContextMenuStrip`; заглушка `#if DEBUG` снова убрана, `InvalidCastException` при создании телефона перехватывается. Прежняя запись о проверке потоков под отладчиком — неверная гипотеза. [[app-testing]] 7.21.

## [2026-11-24] change | Софтфоны: лог (Scope sip) и метрики звонков/регистрации как в AutoCall V3

App: `CallService` (звонки, ошибки), `MicroSIPHelper`/`AbtoPhoneHelper`, `PhoneForm` (регистрация SIP, `IsCallError` → Failed, `CreateAbtoPhone` + диагностика COM; регистрация SDK вынесена в `AutoPhoneV2/Services/SipVoipSdk`), `ClientMetrics` звонков и SIP. Сервер: `MetricTypeEnum` 42–46, `ClientMetricsDto.Calls/SipRegistered`, `MetricCollector`, колонки на `/Admin/Metrics`; миграция не нужна (строки Metric по типу). [[app]], [[plan]], [[app-testing]] 7.21–7.22.

## [2026-11-24] fix | Web: место склада при Move To Warehouse (keepLocation)

`BindLoads` (склад) и `SaveParentAsync` ставили `CurrentLocation`, а `_saveWithHistory_SpecialUpdate` возвращал место из БД (V1 EF-сущность уже несла изменение, V3 перечитывает БД). Добавлен флаг `keepLocation` в `BaseSaveAsync`/`BaseSavesAsync`/`SaveWithHistory(ies)Async`. Проверено на копии сервера, тесты 46/46; в Web пользователя — после перезапуска. [[web-logic]], [[app-testing]] 8.5.

## [2026-11-24] test | App: сессия 8 — Carrier Dispatch (трейлер и склад, документы, несколько точек, связка с брокерским грузом)

16 проверок на грузах 20 и 13173 (части /1, /2): Assign Driver с confirmation, окно Trailer, OrderStops (0 остановок у assigned — как V1; 4 остановки, перетаскивание меняет маршрут 2 505 → 2 170 mi и сохраняет порядок), Assign Brokerage Load (Shipment как V1), Go To Linked Load (по Pro#, не по ShipmentId), Edit Consolidation на контейнере. Найдена и исправлена одна ошибка (8.5); как V1 — перенос строки между таблицами только по Refresh, View Trailer у груза на складе ничего не делает. [[app-testing]].

## [2026-11-24] test | App: Move To Warehouse после перезапуска Web — место склада сохраняется

Груз 20 → Ontario Whse и часть 13173/1 с трейлера → Hodgkins Whse получили координаты склада (исправление 8.5 подтверждено в App). Move To Warehouse на строке контейнера с назначенными частями ничего не делает — как V1 (`ReplaceContainers`). [[app-testing]] 8.5, 8.17.

## [2026-11-24] fix | App: раскладки гридов V1 не применялись (имя сборки AutoCall.WPF → AutoDispatch)

Раскладки `PersistenceManager` (`EquipmentsDialogGridView*`, `DatCompaniesDialogGridView`) V1 записаны с типами `…ColumnProxy, AutoCall.WPF, Version=…`. В App V3 (`AutoDispatch`) они молча пропускались, а при закрытии окна App перезаписывал их дефолтными. По решению пользователя исправлено в App: `GridViewExtensions.SetSettings` заменяет имя сборки перед загрузкой. Проверено отдельной программой на коде App и Telerik R1 2022: 355 раскладок из `autocall_local` — до правки 0, после правки применились все 355. Версии Telerik 2016/2021 не мешают. `DockingLayout` не загружается — как V1. [[app-testing]] 8.18, [[user-settings]], [[app]].

## [2026-11-24] query | Серверная часть: все ли страницы dispatch перенесены, что осталось

Меню WebV1 сверено с контроллерами V3. Страницы dispatch перенесены все: Dispatch Companies, Factoring Companies, Custom Statuses, Confirmation, CarrierRegistration + Print, Map (SelectLocation/OrderStops/маршрут), Download, webhook CarrierView, Api для App. По решению не переносятся страницы модуля AutoCall: AutoCall Emails (почта DAT-лоадборда), CHRobinson, Data Sources, Lifetime, Home (звонки, кэш DAT, флаги GPS; флаг BNSF заменён `BnsfRefreshEnabled`), UnCity, TruckStop, CallReport, UserScript. `GetHybridRate` в V1 всегда возвращал DISABLED. Найден пробел: в `/Admin/Users` нельзя задать `Signature` и учётку AutoPhone, которые в V1 правились на странице User — в [[plan]] «Решить».

## [2026-11-24] fix | Web: Signature и AutoPhone в /Admin/Users, перенос Call Report

По решению пользователя: в форме пользователя под Name — Signature (подпись агента на confirmed, V1 `User/Index`), шрифтом подписи; в списке — колонка AutoPhone (`SipLogin`, ссылка на `/CallReport` компании, как AutoCall V3). Call Report изначально считался частью AutoCall, но софтфоны App переносятся — перенесён копией AutoCall V3: `/CallReport`, блок Phone channel congestion на Home (Owner, Admin), `CallReportService`, `VodiaWebService` (CDR Vodia по FTP), `Secrets:Vodia` (в example и [[deploy]]), тест `14_CallReport` 2/2. Проверено на копии сервера (7183) временным тестом: колонка и ссылка, порядок полей, сохранение и возврат подписи, страницы рисуются; без `Secrets:Vodia` — «Vodia FTP is not configured». Найдено: Save пользователя с пробелом в имени падает на правиле Identity — в [[plan]] «Решить». [[web]], [[plan]].

## [2026-11-25] fix | Web: имя пользователя с пробелом сохраняется в /Admin/Users

По решению пользователя `options.User.AllowedUserNameCharacters = null`: `UserName` в AutoDispatch — отображаемое имя V1 `Users.Name` (вход по email), правило Identity «только буквы и цифры» не давало сохранить перенесённых пользователей вроде «Anton Gmail». В AutoCall V3 `UserName` — логин V1 (обычно email), поэтому там ошибки у этого пользователя нет. Проверено на копии сервера (7183): Save без изменений — 200, строка пользователя и роль прежние. [[plan]] «Имя пользователя с пробелом».
