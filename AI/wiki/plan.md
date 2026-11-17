# План AutoDispatch V3

Отделить AutoDispatch от V1 (`D:\Develop\AutoCall`) и перенести на Ubuntu тем же способом, что AutoCall V3
(`D:\Develop\AutoCallV3\Repo`): новый сервер ASP.NET Core в отдельном Docker-контейнере на том же хосте и отдельном
поддомене, новая БД, новый WPF-клиент. Логика и функционал клиента и сервера не меняются. Этапы в порядке зависимостей,
критерий — проверяемый результат. Решения, помеченные **решить**, принимает пользователь до начала этапа.

Что переносим — [[v1-inventory]]. Протокол App↔Web — [[protocol]]. Данные и цифры — [[data-migration]]. Хост и контейнер — [[deploy]].

## 0. Репозиторий и решения

`D:\Develop\AutoDispatch\Repo` по структуре V3: `Sources/App`, `Sources/Web`, `Deploy`, `Scripts`, `DataBase`, `AI`.
CLAUDE.md и `AI/wiki/{web-architecture,frontend,deploy,logging}.md` из AutoCallV3 — эталон, копируются как есть.
До старта: проверить диск на хосте (сделано: места хватает), замерить канал Windows-сервер → хост на одном каталоге ~1 ГБ,
снять список Quartz-джобов с прод-конфига WebV1.
Решения этапа приняты 2026-11-11: ключи Guid, поддомен `dispatch.myautocall.net`, БД `autodispatch` + `autodispatch_log`,
своя Identity только с ролями AutoDispatch (см. «Решить»).
Критерий: структура создана, таблица «Решить» без открытых строк, влияющих на этапы 1–2 — **выполнен**.

## 1. Скелет WebV3 для AutoDispatch (1–2 недели)

Копия `Sources/Web` из AutoCallV3 → `AutoDispatch.Web.Common/Data/Logic/Server/Test`, `AutoDispatch.Web.sln`.
Остаётся: ServicesHost, UnitOfWork, LinqToDB + EF Core Identity, Serilog в файл и БД логов, `Account`, `Admin*`
(Monitor, Logs, Metrics, Users, Companies), `Test`, `Download`, `ApiAccount`, `ApiLogs`, `ApiMetrics`, `ApiMaps`/`MapService`
(нужны для карт dispatch), Mailgun, фронт-сборка (npm, Tailwind v4 + daisyUI + esbuild), тесты.
Удаляется AutoCall-специфика: Pattern, PreparedPattern, RecordFilter, DatSoap, Proxy, Email, UserSetting.
Критерий: `/Test`, вход админа, `/Admin/Monitor` с обеими БД ok, `Api/Account/SignIn` отдаёт JWT; контейнер на новом
поддомене с сертификатом.
**Сделано 2026-11-11** — [[web]]: скелет скопирован, собирается, `/Test` отвечает, логин-страница и редиректы работают, офлайн-тесты проходят.
Отступления: `UserSetting` оставлен для секции Map карты; метрики DAT/Truckstop/SIP вырезаны; `RoleEnum` сразу = 7 ролей AutoDispatch.
Осталось для критерия: миграции `Init` обоих контекстов (делает пользователь, `Scripts/commands.txt`) → вход админа, Monitor, SignIn;
`npm run build` (Node под текущим пользователем нет); первая публикация контейнера по `Deploy/server-setup.txt`.

## 2. Модель данных и БД (1–2 недели)

29 сущностей `Entities/Dispatch` V1 переносятся в `AutoDispatch.Web.Data` как есть: Guid `EntityId`, `IsDeleted`,
`CreatedDate`; EF6-мапы `Mappings/Dispatch/*Map.cs` → `IEntityTypeConfiguration`, LinqToDB-маппинг.
Без `d_Message` (решено 2026-11-11). `Users/Company` V1 → `AdUser/Company` V3: Identity на Guid-ключах со старыми `Id` пользователей и
`EntityId` компаний (решено 2026-11-13, скелет переведён, миграцию `Init` пользователь пересоздаёт); `Users.Name` → `UserName`;
`UserVerify` не переносится — статус = `IsDeleted` (см. «Решить»). Геоданные: `geography` → пары `Latitude/Longitude decimal(9,6)`, справочники `State/Country` — статический класс в коде, `City` не нужна
(решено 2026-11-13, см. «Решить»). Открытых вопросов, влияющих на схему, нет.
**Сущности перенесены 2026-11-13** — [[data-model]]: 28 классов, `DispatchMap` (EF Core + LinqToDB), тест `07_DispatchModel` без БД.
Таблицы без префикса `d_`, `ActualCompany` → `DispatchCompany` (и `ActualCompanyId` → `DispatchCompanyId`), координаты парами decimal,
без FK на пользователей и справочники. Осталось: миграция `Dispatch` (пользователь), публикация, блоки 1–5 скрипта переноса и прогон на копии.
Критерий: миграция `Init` применена, Seed отработал, скрипт переноса ([[data-migration]]) прогнан на копии, 7 компаний видны
в `/Admin/Companies`.

## 3. Серверная логика (3–5 недель, критический путь)

Портировать `DispatchService.cs` (4651 строка, 74 метода после удаления чата и `ReportSimilarLoads`) и сервисы:
DatCompany, ActualCompany, Census, SetupPacket, Confirmation, CustomStatus, CustomerCompany, ShipperReceiver,
FactoringCompany, FixLocationLoads, Report, Geocode, OpenAI. На `BaseService(ServicesHost)` V3:
EF6 `DbContext` → LinqToDB, `HostService<T>` → `Host.GetService<T>()`, `HttpContext.Current` → параметры,
`ThreadHelper1.StartSTATask` убрать (STA нужен был Telerik под Windows), `ConfigurationManager` → `AppSettings.Secrets`,
`FilePath/RecyclePath` → `AppSettings.FilesPath` = volume `/files`.
Внешние сервисы в `WebServices/` с `ApiResult<T>`: CarrierView, SMS (Voxbone/Bandwidth), Bnsf, SaferFmcsa, Google Maps,
DatRate, OpenAI, Keeppingin. Конвертеры `Converters/Dispatch/**` и `BllMappingProfile` почти без изменений.
Quartz → `BaseBackgroundService` (чистка `d_HistoryObject`/`d_Event` по 30 дням — `QuartzService.cs`).
Проверить на Linux: Telerik Document Processing (`Telerik.Documents.*` кросс-платформенные), iTextSharp → `itext7`
или `iTextSharp.LGPLv2.Core`, Magick.NET. `Microsoft.ReportViewer` к dispatch не относится (см. [[v1-inventory]]).
Критерий: xunit `AutoDispatch.Web.Test` против поднятого сервера на копии данных — по сценарию на каждый из 74 методов.
**Код перенесён 2026-11-13** — [[web-logic]]: `DispatchService` (9 partial, 74 метода `IDispatchService`), 15 сервисов, конвертеры и
`DispatchMappingProfile`, `WebServices` (CarrierView, Bnsf, Safer, Bandwidth SMS, OpenAI), 4 `BackgroundService` вместо Quartz,
`Resources` (письма, RTF, fw9.pdf, шрифт), общий контракт `Sources/Shared/AutoDispatch.Dtos`. Решение собирается, `08_DispatchLogic` проходит.
RTF→PDF сделан 2026-11-16 (`TelerikPdfHelper`, см. «Решить»). Не сделано: запуск против БД (ждёт миграции `Dispatch` + `AdUser.Signature` и переноса данных).

## 4. API для App (1–2 недели, параллельно с 3)

`ApiDispatchController` по `ApiBaseController` V3, `[Authorize(Bearer)]`: `Request`, `Response` (long-polling с отменой
предыдущего ожидания пользователя, образец — `Api/DatSoap/AlarmNotification` V3), `RequestZip/ResponseZip`,
`CheckToken` = `Api/Account/Session`. Протокол и `TypeNameHandling` — [[protocol]].
Остальные эндпоинты App: `Api/DatCompany` (Search, SendSetupPocket), `Api/ActualCompany`, `Api/Census`,
`Api/Dispatch/Helpers` (SearchLoads — без `CacheLoadIndex`, см. «Решить»; SearchCustomers, PlaceAutocomplete, PlaceDetails, Distance, ExtractLoadFromPdf),
`Api/SetupPacket/FactoringCompanies`. `Api/Token` V1 (OWIN OAuth form) → `Api/Account/SignIn` → `SessionDto` с ролями.
Контракт `AutoDispatch.Dtos` — общий проект в `Sources/Shared`, имя сборки и namespace не меняются.
Критерий: старый App V1, переключённый на новый URL, проходит логин и полный цикл груза без правок (проверка контракта).
**Начато 2026-11-17** — [[protocol]]: `ApiDispatchController` (`Request`, `Response`, `RequestZip`, `ResponseZip`) на `ApiBaseController` V3;
`DispatchRequestHandlers` — таблица `Name → handler` по `IDispatchService` (Expression-делегаты, роль из `MinAccess`, иерархия `RoleEnum`);
`DispatchPollRegistry` (singleton) — отмена предыдущего ожидания пользователя; `IsDeleted` проверяется на каждом запросе; `MscorlibBinder`
пропускает `$type` только из `AutoDispatch.Dtos` и core-коллекций. Тест `09_DispatchApi` (таблица, whitelist, HTTP: SignIn → GetModules →
чужой UserId → RequestZip → long-polling с отменой) проходит против локального сервера.
**Остальные эндпоинты сделаны 2026-11-17** на маршрутах V3 (решено, см. «Решить»): `Api/Dispatch/Helpers/*` (SearchLoads, SearchCustomers,
PlaceAutocomplete, PlaceDetails, Distance, ExtractLoadFromPdf, ExtractLoadFromPdf2), `Api/DatCompany/Search`, `Api/ActualCompany`, `Api/Census/Search`,
`Api/SetupPacket/FactoringCompanies`; `SessionDto` + `HasBnsfRailway`, `CustomStatuses`; `/Temp` — статика из `AppSettings.TempPath`;
`DispatchService.SearchLoadsAsync` без `CacheLoadIndex`. Тест `09_DispatchApi.Test04` проходит (Distance — без ключа Google локально, 400).
Лимит тела: nginx `client_max_body_size 100m` (`Deploy/nginx`), Kestrel на `Request/RequestZip/ExtractLoadFromPdf` снят (`DisableRequestSizeLimit`); V1 — `maxRequestLength` 10 ГБ, т.е. без лимита. Файл в `FileSave` идёт base64 в JSON (+33 %), 100m = документ до ~75 МБ; индексы под `SearchLoads` уже есть (см. «Решить»);
критерий этапа переформулирован — старый App без правок не логинится (маршруты V3), контракт `Request/Response` проверяется тестом и App на этапе 7.

## 5. Публичный сайт без SetupPacket (1–2 недели)

Страницы без App: `Confirmation` (Index по `?LoadId=`, POST подписи, `Confirmed`, `Download/{loadId}`),
карты для App `Dispatch/RouteLocation|OrderStops|SelectLocation`. По V3 — Razor + htmx + Stimulus + Tailwind.
Knockout-viewmodels и bootstrap 3 переписываются; единственное место, где «логика не меняется» ≠ «код не меняется».
Публичные URL (`Confirmation/?LoadId=`, `Confirmation/Download/{id}`) — буква в букву как в V1: ссылки уже в разосланных письмах.
Карты переиспользуют `MapService` и `map_controller.ts` V3 — см. «Решить: карты для App» и «`Dispatch/Dispatcher`».
SetupPacket вынесен в этап 6 (решено 2026-11-17): старую knockout-реализацию не тянуть, переписать сразу на daisyUI + Stimulus;
переключение всё равно после реализации всего функционала.
Критерий: карриер по ссылке из письма подтверждает груз, PDF confirmation скачивается; карты открываются по ссылке с токеном.
**Сделано 2026-11-17** — [[public-site]]: `ConfirmationController` + `_PublicLayout`, `Map/SelectLocation` и `Map/OrderStops`
(`Api/Maps/SelectLocationLink|OrderStopsLink`, токены `r`, результат в App через `postMessage` / `window.reorder`), `Client/map_common.ts`;
попутно исправлен `Expenses_DtoToVm` — доплаты терялись из-за whitelist `$type` этапа 4. Тесты `10_PublicSite` (3) и `08.Test10` проходят;
подпись (POST) и письмо локально не проверены — нет PDF confirmation и SMTP, сверка на этапе 8.

## 6. SetupPacket на daisyUI + Stimulus (2–3 недели)

`CarrierRegistration` (15 шагов, версии views `StepXX_VV`, `View/Edit` под авторизацией сайта, `DownloadFileMC/COI`,
`FactoringCompanies`), `Print/SetupPacket/Agreement|InsuranceInstructions`. V1: 18 views (5 700 строк Razor, bootstrap 3),
16 knockout-моделей (970 строк TS), select2, signature pad (шаги 4, 6, 7), загрузка файлов (шаг 13). Публичный URL
`CarrierRegistration/?CarrierId=` как в V1. Серверная часть (`SetupPacketService`, `ReportService`, письма) перенесена на этапе 3.
Критерий: карриер по ссылке из письма проходит все 15 шагов, PDF setup packet и W-9 генерируются и уходят на почту.

## 7. Клиент AutoDispatch App (3–4 недели)

По аналогии «AppV3 = копия V2»: `Sources/App` V1 (`AutoCall.WPF`, `AutoDispatch.*`, `_Common`, `_Dtos`) → `Sources/App`.
- SDK-style csproj, `net10.0-windows`, x86, `AssemblyName = AutoDispatch`, `Directory.Build.props` как в V3;
  Unity → `Microsoft.Extensions.DependencyInjection`, log4net/Common.Logging → Serilog с `ServerLogSink`,
  Telerik WPF → текущие пакеты под .NET 8+.
- Вырезать модуль AutoCall: `AutoCall.Logic(.Contracts)`, `AutoCall.Data.Contracts`, `AutoPhoneV2`, борды и диалоги `DatPower*`,
  `Patterns`, `RecordFilter`, `QuickRateSearch`, `MicroSIP*`, `SimilarLoadsDialog` + `Microsoft.ReportViewer`,
  `IChatService/ChatService`. `LoginDialog` и `MainWindow` упрощаются: один модуль, борды Carrier/Broker/Accounting/History по ролям.
- `EngineDataService`, `LoginDataService`, `DatCompanyDataService`, `HelperDataService`, `ActualCompanyDataService` →
  `BaseApi` V3 (HttpClient, Bearer, `X-Request-Id`, `Settings.WebUrl`); `UrlApi` из app.config убрать.
- ClickOnce-профиль и `Scripts/publish-autodispatch-app.ps1` по образцу V3, раздача из `/Download` нового контейнера.
Критерий: один пользователь на V1 и на новом App видит одинаковые борды и проходит цикл
груз → назначение → подтверждение → инвойс → история; сверка пользователем.

## 8. Деплой, перенос данных, переключение (1 неделя)

`Deploy/AutoDispatch.Web/Dockerfile` (копия V3 + volume `/files`), `Deploy/nginx/dispatch.myautocall.net.conf`, `server-setup.txt`,
`Scripts/publish-autodispatch-web.ps1` (контейнер `autodispatch.web`, порт 5005), `Scripts/sync-dispatch-files.ps1`,
`appsettings.Production.json.example` — [[deploy]].
Перенос данных по блокам из [[data-migration]]: файлы (147 ГБ) заранее, БД и досинк файлов — в окно переключения,
укладывается в вечер. Пользователям 7 компаний выдаётся ссылка ClickOnce нового App; WebV1 остаётся на чтение для сверки.
Критерий: публикация одним скриптом, сайт отвечает по поддомену с сертификатом, App логинится, данные совпадают с V1.

## 9. Контроль

`/Admin/Monitor|Logs|Metrics` из V3. Dispatch-метрики: пользователи в long-polling, latency `Request` по именам методов,
ошибки внешних сервисов (CarrierView, SMS, OpenAI), прирост `d_Event` и `d_HistoryObject`.

Оценка: 3–4 месяца одного разработчика; критический путь — этапы 3 и 7.

## Решить

| Вопрос | Варианты / решение | Влияет на |
|---|---|---|
| Ключи БД | **Решено 2026-11-11: Guid, как в V1.** Контракт 170 типов и 74 метода, `HistoryId`, ссылки между 29 таблицами; int, как в V3, = переписать контракт и клиент. Единственное сознательное отступление от V3 | схема, перенос, контракт |
| Ключи пользователей и компаний | **Решено 2026-11-13: тоже Guid, старые `Id` сохраняются.** `IdentityUser<Guid>`, `Company.Id` Guid, `BaseEntity.Id` Guid; контракт несёт `Guid CompanyId/CreatedById/UserId`, с int-Identity V3 пришлось бы конвертировать на каждом ответе. Маппинг `_MigrateCompany/_MigrateUser` в скрипте переноса убран. Id ролей = `RoleEnum.ToRoleId()` (`00000000-0000-0000-0000-00000000000N`), System/Admin = `…001000/…001001` | скелет [[web]], миграция `Init` (пересоздать), [[data-migration]] |
| `UserVerify` / `VerifyId` | **Решено 2026-11-13: не переносить, как в V3.** Статус пользователя = `AdUser.IsDeleted` (Delete/Restore в `/Admin/Users`, lockout + security stamp). Проверку V1 «на каждом `Request` статус из БД» повторить в `ApiDispatchController` проверкой `IsDeleted` (этап 4), иначе заблокированный работает по JWT до 30 дней. Переносятся только `VerifyId = 2` (allowed) | этап 4, [[data-migration]] |
| `Users.Name` | **Решено 2026-11-13:** отображаемое имя = `AdUser.UserName` (пусто → email). `NormalizedUserName` уникален — при дублях скрипт добавляет email | [[data-migration]] |
| Признак входа пользователя | **Решено 2026-11-13: окно год** (у компаний — 6 месяцев). `Users.LastModifiedDate` (V1 обновляет при работе из App — по данным, не по коду) или `Session.LastActivity` (сайт). Одной `Session`, как в V3, мало — она только для сайта; по ней выпадали бы `billing` TORPEDO (6 334 правки) и `Vlad` Baltic. Итог 17 пользователей: TORPEDO 3, DDM 8, Baltic 2, BEST AMERICAN 2, BEST SOURCE 1, LEVSKI 1 | блок 0 [[data-migration]] |
| Строки `varchar` | **Решено 2026-11-13:** все строковые колонки dispatch — `varchar` (в данных латиница, БД `autodispatch` в UTF-8 collation `Latin1_General_100_CI_AI_SC_UTF8`, потерь нет). В `DispatchMap.Configure` — `SetIsUnicode(false)` для всех string; миграция поверх `Dispatch` — пользователь | [[data-model]], миграция |
| Таблица `CacheLoadIndex` | **Решено 2026-11-16: не переносить.** Кэш для автокомплита «город + компания» в окне груза (`Dispatch/SearchLoads`): одна строка на пару по первым 16 символам, писали `SaveLoadIndex` при сохранении груза, `CacheLoadJob` раз в сутки и `FixLocationLoads`; читал только `SearchLoadsAsync`. Проверка `autocall` 2026-11-16: байтовое переполнение в UTF-8 есть только у `d_CacheLoadIndex.Company`/`City` (61 строка, 15 у активных) — следствие `SUBSTRING(…,1,16)` в колонку из 16; остальные 40 колонок с не-ASCII (тире, апострофы, `Ö`, кириллица в именах файлов) в лимиты влезают. Убраны сущность, маппинг, `LoadIndexCacheService`, вызовы в `DispatchService.Save`, блок 2 скрипта; `CacheLoadJob` не переносится. На этапе 4 `SearchLoads` — прямой запрос `Load` + `ShipperReceiverCompany` своей компании (`like 'текст%'`, `distinct top 5`); контракт `SearchLoadsRequest`/`QueryLoadDto` тот же. **Сделано 2026-11-17** — `DispatchService.SearchLoadsAsync`: у `Load` нет `CompanyId`, компания — через `ShipperReceiverCompany.CompanyId` (join по `ShipperId`/`ReceiverId`), union origin+destination. Индексы `ShipperReceiverCompany (CompanyId)`, `Load (ShipperId)`, `Load (ReceiverId)` уже есть — EF Core создаёт их для внешних ключей (`IX_*` в миграции `Dispatch`, см. снапшот); отдельной миграции не нужно. Плюс: V1 резал текст до 16 символов и дедуплицировал без `CompanyId` | `DispatchMap`, `HistoryCleanupService`, `DispatchService.Save`, `HistoryCleanupBackgroundService`, миграция поверх (пользователь) |
| Collation `autodispatch` | **Открыто** (отложено до этапа 3, решить до боевого прогона переноса). Сейчас UTF-8, как в V3: `varchar(N)` = N байт, `é` — 2 байта; у `autocall` `SQL_Latin1_General_CP1_CI_AS`, `varchar(N)` = N символов. Падение на `CacheLoadIndex.City` снято — таблицы нет (решено 2026-11-16); по скану 2026-11-16 переполнений в остальных колонках нет, `DatCompany.Name` уже 63/64 байт. В данных V1 вне ASCII много (акценты), вне Latin-1 — 52 строки мусора. **Рекомендация (пересмотрена 2026-11-16)**: оставить UTF-8 — при `SQL_Latin1_General_CP1_CI_AS` кириллица в именах файлов (`Load.BOLFile` и др.) станет `?` и перестанет совпадать с файлами на диске. Альтернатива: `SQL_Latin1_General_CP1_CI_AS`, код и миграции не меняются | `DataBase/create autodispatch.sql`, повторный прогон переноса |
| Геоданные `geography` | **Решено 2026-11-13: заменить на пары `Latitude/Longitude decimal(9,6)`**, как в V3 (`MapPointDto`), без NetTopologySuite и `Microsoft.SqlServer.Types` на Linux. Колонки: `d_Load.OriginLocation/DestinationLocation`, `d_Warehouse.Location`, `d_Yard.Location`, `CurrentLocation` у `BaseDispatchEntity` (позиции CarrierView/BNSF). Единственный пространственный запрос — радиус в `LoadHistory2` (`DispatchService.cs:988-995`, сырой SQL `STDistance`) → гаверсинус в том же SQL. Контракт `LocationDto` уже `Latitude/Longitude`. Перенос: `OriginLocation.Lat/.Long` в [[data-migration]]. Автокомплит и геокодинг — `PlaceService` через Safer, как в V3 (этапы 3–4) | этап 2 схема, этап 3 `LoadHistory2`, [[data-migration]] |
| Справочники `City/State/Country` | **Решено 2026-11-13: таблиц в БД нет, как в V3.** `City` не нужна (`d_Load` хранит город строкой, `GeocodeService` → Safer). `State` и `Country` остаются статическим классом в коде с теми же `Id/Code/Name` (снять один раз с боевых таблиц `State`, `Country`): на них ссылаются `byte` колонки `d_SetupPacket.Physical/Mailing/W9StateId`, `Physical/MailingCountryId`, `d_SetupPacketFactoringCompany.StateId` — переносятся как есть, без FK. Читают `ReportService` (PDF), `SetupPacketController`/`PrintController` (списки), `FactoringCompanyService` (поиск по штату — в памяти), `GeocodeService`, SQL `DispatchService.cs:3689` | этап 2 схема, этапы 3 и 5 |
| Поддомен | **Решено 2026-11-11:** `dispatch.myautocall.net` | nginx, certbot, `Settings.WebUrl` |
| БД | **Решено 2026-11-11:** `db.myautocall.net`, базы `autodispatch` + `autodispatch_log` (без суффикса `_v3`) | [[deploy]], строки подключения, `DataBase/create autodispatch.sql` |
| Пользователи | **Решено 2026-11-11: своя Identity-БД.** Переносятся пользователи 7 компаний только с ролями AutoDispatch: Agent, Warehouse, Dispatcher, Sales, Accounting, Owner, Admin (`Permissions.cs:11-27`); остальные роли V1 не заводятся. Роли сохраняются как есть (на них завязаны `MinAccessAttribute` и `RolesOperator`); пароли — хэш Identity v2 | Seed, перенос, `RoleEnum` |
| Активные компании | **Решено 2026-11-11:** компании с `d_Event` за 6 месяцев — 7 компаний, их пользователи, данные и документы | [[data-migration]] |
| `d_Event` | **Решено 2026-11-11:** переносить для активных компаний, `Id` сохраняются | [[data-migration]] |
| `d_Message`, `ChatService` | **Решено 2026-11-11:** не переносить. В App чат мёртвый код, на Web используется только AutoCall-стороной | контракт (−2 метода), App |
| `ReportSimilarLoads` | **Решено 2026-11-13: не портировать.** Единственный вызов — «Similar Loads» из карточки лоадборда AutoCall. Побочно: в AutoCall V3 эта кнопка после разделения БД либо ходит в dispatch-API, либо отключается — решение для AutoCallV3 | контракт (−1 метод) |
| iTextSharp на Linux | **Решено 2026-11-16:** `iTextSharp.LGPLv2.Core` — та же библиотека, что в Safer (`D:\Develop\Safer\Source`, там 3.7.12), у нас последняя бесплатная 3.8.7; LGPL, тот же API 4.x — `PdfStamper`, `AcroFields`, `PdfCopy`, код V1 без переписывания. itext7 (AGPL) не используем | `ReportService.GenerateW9Pdf`, `DispatchPdfGenerator` |
| `AdUser.Signature` | **Решено 2026-11-13:** колонка `varchar(64)` в `AdUser` (V1 `Users.Signature`, подпись агента в письме confirmed). Миграция — пользователь; перенос значения добавить в блок 0 [[data-migration]] | миграция, скрипт переноса |
| PDF для OpenAI | **Решено 2026-11-13:** текст — PdfPig, PNG — PDFtoImage (pdfium) вместо Telerik Fixed/Spire/Magick.NET; формат разделителей страниц Telerik сохранён | `OpenAIService` |
| Кэш геокодинга | **Решено 2026-11-13:** таблицы `Cache` нет (как в V3), кэши `GeocodeService` в памяти процесса; `ClearExpiredCacheJob` не нужен | `GeocodeService` |
| Протокол `Request` | **Сделано 2026-11-17:** словарь `Name → handler`, построенный из `IDispatchService` при старте (`DispatchRequestHandlers`), `SerializationBinder` на `$type` (whitelist `AutoDispatch.Dtos`, `System.Private.CoreLib`, `System.Collections`) — [[protocol]] | `ApiDispatchController` |
| Маршруты эндпоинтов App вне `Request` | **Решено 2026-11-17: маршруты V3, ответ — данные как есть (`JsonSuccess`, camelCase), ошибка — 400 `{error}`; App адаптируется на этапе 7 вместе с `BaseApi`.** Исключение: `Api/DatCompany/Search` отдаёт словарь полей без camelCase — клиент ищет по ключам «Legal Name:». Место/детали — JSON без gzip (V1 `GetPlaceAutocompleteJson`). Было: план называет новые маршруты (`Api/DatCompany`, `Api/ActualCompany`, `Api/Census`, `Api/Dispatch/Helpers`, `Api/SetupPacket/FactoringCompanies`), а критерий этапа — «старый App V1 без правок». App V1 ходит на `api/DatCompany/Search`, `ActualCompany/ActualCompaniesByUserId`, `Home/SearchCensusCompanies`, `CarrierRegistration/FactoringCompanies`, `Dispatch/SearchLoads|SearchCustomers|GetPlaceAutocomplete|GetPlaceDetails|GetDistance|ExtractLoadFromPdf(2)` с ответами `{Success, Loads/Companies/ActualCompanies/CensusCompanies/Data/Fields, Message}` (PascalCase, `JsonExtension` V3 даёт camelCase — Newtonsoft клиента регистр не различает). Варианты: (а) старые URL и формы ответов как есть — критерий выполним, App на этапе 7 не трогает URL; (б) маршруты V3 — App V1 без правок не проверить, `DataServices` переписываются на `BaseApi` на этапе 7 всё равно. `api/DatCompany/SendSetupPocket` и `Api/Account/Check` на сервере V1 отсутствуют — мёртвые вызовы клиента, не переносятся | этап 4, этап 7 |
| Логин App | **Решено 2026-11-17: `SessionDto` расширен — `HasBnsfRailway` (есть `DispatchCompany` с `BnsfLogin`), `CustomStatuses` (`List<CompanyCustomStatusDto>` вместо JSON-словаря V1); роли — список имён, битовую сумму V1 App на этапе 7 не использует.** Было: `Api/Token` → `Api/Account/SignIn` (`SessionDto`: Token, Id, CompanyId, CompanyName, UserName, Email, TimeZoneId, Roles как список имён). OAuth-ответ V1 (`ApplicationOAuthProvider`) кроме токена нёс `roles` (битовая сумма), `hasBnsfRailway`, `customStatuses` (JSON списка статусов компании), `companyId/companyName`, `userId`, `name` — App V1 их читает при входе. Решить: (а) расширить `SessionDto` полями `HasBnsfRailway` и `CustomStatuses` (App на этапе 7 берёт их из сессии), либо (б) App запрашивает их отдельно после входа. Совместимый `Api/Token` (form → OAuth JSON) делать только если выбран вариант «старый App без правок» | `ApiAccountController`, `UserService.GetSessionAsync`, этап 7 |
| Telerik Documents на Linux | **Решено 2026-11-16: Telerik Document Processing (.NET Standard), как в Safer.** V1 использует WPF-модель `RadDocument` (`Telerik.Windows.Documents` + `FormatProviders.Rtf/Pdf`), установленные DLL R1 2022 ссылаются на `PresentationCore` — на Linux не работают. Кросс-платформенная линейка того же продукта (входит в лицензию UI for WPF): `Telerik.Documents.Flow` + `Telerik.Documents.Flow.FormatProviders.Pdf` (`RadFlowDocument`, `RtfFormatProvider.Import` → `PdfFormatProvider.Export`, код почти тот же). Из Safer взят только способ хранения: `Telerik.Documents.Core/Fixed/Zip` **2022.1.217** в локальном источнике `Libraries/Telerik` + `nuget.config` (RTF там не конвертируется: Carrier Onboarding — Razor → DinkToPdf, W9 — iTextSharp; Telerik Fixed только читает PDF). **Сделано 2026-11-16:** 5 пакетов 2022.1.217 в `Sources/Web/Libraries/Telerik` (Flow — из аккаунта, UI for WPF 2022.1.222; Core/Fixed/Zip — из Safer), `Sources/Web/nuget.config`, `Helpers/Dispatch/TelerikPdfHelper`: `FixedExtensibilityManager.FontsProvider` — шрифты из `Resources/Fonts` по фактическим запросам Telerik на шаблонах (Calibri ×4, маркеры списков Arial «•», Courier New «o», Wingdings «▪», Symbol «•»; Times New Roman не нужен) — см. строку «Шрифты в репозитории», `JpegImageConverter` — SkiaSharp (см. «ImageSharp»), PNG-логотип на белом фоне. Уровней сжатия deflate (в V1 — 9) в настройках Flow нет. Тест `08_DispatchLogic.Test09` (3 шаблона, шрифты встроены, без Helvetica) проходит на Windows и в Linux-контейнере (`sdk:10.0` на `ububuilder`, 2026-11-16; нативные `libSkiaSharp.so`/`libpdfium.so` без недостающих зависимостей и на `aspnet:10.0`). Экспорт под `lock` — статический `FontsRepository` Telerik не потокобезопасен, параллельные экспорты рушат его (воспроизведено в контейнере) | этап 3, `ReportService` |
| ImageSharp 4.x (лицензионный ключ) | **Решено 2026-11-16: SkiaSharp 4.154.0** (как в Safer; уже был транзитивно через PDFtoImage, Linux native — `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.154.0 — явный пин, как в Safer: PDFtoImage 5.4.0 тянет 4.150.1, и managed 4.154.0 на Linux её не принимает — `SKBitmap.Decode` возвращал null, Telerik падал «Not supported image format»; найдено в контейнере 2026-11-16). ImageSharp 4.x при Release-сборке требует ключ Six Labors (`SixLabors.ImageSharp.targets`), publish падал; 3.x — та же Split-лицензия. Переписаны `ImageHelper` (превью 200 px) и `TelerikPdfHelper.JpegImageConverter` | `Helpers/Dispatch/ImageHelper`, `TelerikPdfHelper` |
| Шрифты в репозитории | **Решено 2026-11-16:** шрифты Microsoft (Calibri, Arial, Courier New, Symbol, Wingdings) распространять нельзя — в `Resources/Fonts` метрически совместимые OFL: Calibri → Carlito ×4, Arial → Liberation Sans, Courier New → Liberation Mono (+ `OFL-*.txt`); Symbol «•» — стандартный PDF-шрифт, файл не нужен. Wingdings («▪» маркер 3-го уровня списка, 3 шт.) свободной замены нет: `FontsProvider` ищет `wingding.ttf` в `Resources/Fonts`, затем в `AppSettings.HostFontsPath` = `/files/_fonts` на хосте — положить туда или смириться с пустым маркером. подпись (W9, confirmation, RTF «Brush Script MT») — `DancingScript-Bold.ttf` (OFL, Google), тот же шрифт, что подписывает документы Safer (`wwwroot/fonts`); `BRUSHSCI.TTF` из V1 (Monotype) удалён | `TelerikPdfHelper`, `Resources/Fonts` |
| SetupPacket фронт | **Решено 2026-11-17: сразу daisyUI + Stimulus, отдельным этапом 6 после публичного сайта и до App.** Старую knockout-реализацию (18 views bootstrap 3, 16 TS-моделей, select2, signature pad, upload) не тянуть: переключение всё равно после реализации всего функционала, второй CSS-стек ради промежуточного шага не нужен. Серверная часть уже на этапе 3 | этапы 5–6 |
| `Dispatch/Dispatcher` | **Решено 2026-11-17: не переносить.** Веб-борд диспетчера на карте: `DispatchViewModel` (671 строк TS) + `EngineService`/`DispatchService` + `Data/*` (~4 800 строк knockout) ходят в `api/Dispatch/Request` тем же протоколом, что App, long-polling по `d_Event`. Ссылок на страницу нет ни в `Menu.cshtml`, ни в App; рядом заброшенная `DispatchViewModel (google map).ts`. Не переносятся и `Scripts/Dispatch/Data|Wrappers|Requests|Responses`, `EngineService.ts`, `DispatchService.ts` — нужны только модели страниц карт | этап 5 |
| Карты для App | **Решено 2026-11-17: `RouteLocation`/`RouteLocation2` → готовый `/Map` V3; `SelectLocation` и `OrderStops` — новые страницы по той же схеме (токен `r`, embedded layout, результат через `postMessage` WebView2); App на этапе 7 переходит на WebView2.** Было: V1: EO.WebBrowser с `Authorization: Bearer`, страницы под `AutoCallAuthorize`, точки открыто `?t=lat lon,…`; `SelectLocation` — App парсит HTML страницы по id `address/city/latlng` (`GetHtml()`); `OrderStops` только показывает остановки трейлера, порядок меняется в WPF; `RouteLocation2` (пустая карта), `InitializeRouteLocation` (прогрев, хост `myautocall.net` захардкожен в `App.xaml.cs:153`). V3: `/Map?r=шифрованный токен`, `[AllowAnonymous]`, WebView2. Рекомендация: `RouteLocation` → готовый `/Map` V3 (App берёт ссылку через `Api/Maps/Link`), `SelectLocation` и `OrderStops` — новые страницы по той же схеме (токен `r`, embedded layout, результат в App через `postMessage` WebView2). Тянет решение для этапа 7: EO.WebBrowser 24 → WebView2, как в AutoCall V3 | этап 5, этап 7 |
| Публичные URL и редирект с `myautocall.net` | **Решено 2026-11-17: маршруты как в V1** — `Confirmation/?LoadId=`, `Confirmation/Download/{id}`, `CarrierRegistration/?CarrierId=`, `Print/SetupPacket/Agreement|InsuranceInstructions` (`DispatchEmailGenerator.cs:43,205,233`), ссылки уже в разосланных письмах. На этапе 8 нужен редирект этих путей с `myautocall.net` на `dispatch.myautocall.net` (nginx AutoCall) и referrer `dispatch.myautocall.net` у `GoogleMap:JsKey`. `CarrierRegistration/View|Edit` — под cookie-логином сайта без ролей, как в V1; подтверждение груза от System `…001000` (V1 `AutoCallAdmin`) | этапы 5, 6, 8 |
| Quartz-джобы dispatch | **Решено 2026-11-13 по коду `Web/App_Start/JobScheduler.cs`** (прод-конфиг не смотрели): QuartzJob 01:00 будни — чистка истории, CacheLoadJob 00:10 — индекс грузов, LoadWaitingJob 5 мин, LoadSendSmsJob 5 мин, BnsfRailwayJob 30 мин (выключен: `AppSettings.BnsfRefreshEnabled = false`, в V1 никто не пользуется), ClearExpiredCacheJob — таблицы нет. Остальные джобы — AutoCall | `Server/BackgroundServices` |
| Бэкап `/files` | 150 ГБ на хосте без бэкапа; сейчас файлы бэкапятся вместе с Windows-сервером | хостинг |
