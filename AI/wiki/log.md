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
