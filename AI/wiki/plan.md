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

## 4. API для App (1–2 недели, параллельно с 3)

`ApiDispatchController` по `ApiBaseController` V3, `[Authorize(Bearer)]`: `Request`, `Response` (long-polling с отменой
предыдущего ожидания пользователя, образец — `Api/DatSoap/AlarmNotification` V3), `RequestZip/ResponseZip`,
`CheckToken` = `Api/Account/Session`. Протокол и `TypeNameHandling` — [[protocol]].
Остальные эндпоинты App: `Api/DatCompany` (Search, SendSetupPocket), `Api/ActualCompany`, `Api/Census`,
`Api/Dispatch/Helpers` (SearchLoads, SearchCustomers, PlaceAutocomplete, PlaceDetails, Distance, ExtractLoadFromPdf),
`Api/SetupPacket/FactoringCompanies`. `Api/Token` V1 (OWIN OAuth form) → `Api/Account/SignIn` → `SessionDto` с ролями.
Контракт `AutoDispatch.Dtos` — общий проект в `Sources/Shared`, имя сборки и namespace не меняются.
Критерий: старый App V1, переключённый на новый URL, проходит логин и полный цикл груза без правок (проверка контракта).

## 5. Публичный сайт (2–3 недели)

Страницы без App: `CarrierRegistration` (15 шагов, `View/Edit` под авторизацией), `Print/SetupPacket/*`, `Confirmation`,
карты `Dispatch/Dispatcher|RouteLocation|OrderStops|SelectLocation`. По V3 — Razor + htmx + Stimulus + Tailwind.
Knockout-viewmodels, Kendo и bootstrap 3 переписываются; единственное место, где «логика не меняется» ≠ «код не меняется».
**Решить:** перенести `SetupPacket` views и TS как есть первым шагом (knockout через esbuild), переделку на Stimulus —
отдельным этапом. Карты переиспользуют `MapService` и `map_controller.ts` V3.
Критерий: карриер по ссылке из письма проходит все 15 шагов, PDF setup packet и W-9 генерируются и уходят на почту.

## 6. Клиент AutoDispatch App (3–4 недели)

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

## 7. Деплой, перенос данных, переключение (1 неделя)

`Deploy/AutoDispatch.Web/Dockerfile` (копия V3 + volume `/files`), `Deploy/nginx/dispatch.myautocall.net.conf`, `server-setup.txt`,
`Scripts/publish-autodispatch-web.ps1` (контейнер `autodispatch.web`, порт 5005), `Scripts/sync-dispatch-files.ps1`,
`appsettings.Production.json.example` — [[deploy]].
Перенос данных по блокам из [[data-migration]]: файлы (147 ГБ) заранее, БД и досинк файлов — в окно переключения,
укладывается в вечер. Пользователям 7 компаний выдаётся ссылка ClickOnce нового App; WebV1 остаётся на чтение для сверки.
Критерий: публикация одним скриптом, сайт отвечает по поддомену с сертификатом, App логинится, данные совпадают с V1.

## 8. Контроль

`/Admin/Monitor|Logs|Metrics` из V3. Dispatch-метрики: пользователи в long-polling, latency `Request` по именам методов,
ошибки внешних сервисов (CarrierView, SMS, OpenAI), прирост `d_Event` и `d_HistoryObject`.

Оценка: 3–4 месяца одного разработчика; критический путь — этапы 3 и 6.

## Решить

| Вопрос | Варианты / решение | Влияет на |
|---|---|---|
| Ключи БД | **Решено 2026-11-11: Guid, как в V1.** Контракт 170 типов и 74 метода, `HistoryId`, ссылки между 29 таблицами; int, как в V3, = переписать контракт и клиент. Единственное сознательное отступление от V3 | схема, перенос, контракт |
| Ключи пользователей и компаний | **Решено 2026-11-13: тоже Guid, старые `Id` сохраняются.** `IdentityUser<Guid>`, `Company.Id` Guid, `BaseEntity.Id` Guid; контракт несёт `Guid CompanyId/CreatedById/UserId`, с int-Identity V3 пришлось бы конвертировать на каждом ответе. Маппинг `_MigrateCompany/_MigrateUser` в скрипте переноса убран. Id ролей = `RoleEnum.ToRoleId()` (`00000000-0000-0000-0000-00000000000N`), System/Admin = `…001000/…001001` | скелет [[web]], миграция `Init` (пересоздать), [[data-migration]] |
| `UserVerify` / `VerifyId` | **Решено 2026-11-13: не переносить, как в V3.** Статус пользователя = `AdUser.IsDeleted` (Delete/Restore в `/Admin/Users`, lockout + security stamp). Проверку V1 «на каждом `Request` статус из БД» повторить в `ApiDispatchController` проверкой `IsDeleted` (этап 4), иначе заблокированный работает по JWT до 30 дней. Переносятся только `VerifyId = 2` (allowed) | этап 4, [[data-migration]] |
| `Users.Name` | **Решено 2026-11-13:** отображаемое имя = `AdUser.UserName` (пусто → email). `NormalizedUserName` уникален — при дублях скрипт добавляет email | [[data-migration]] |
| Признак входа пользователя | **Решено 2026-11-13: окно год** (у компаний — 6 месяцев). `Users.LastModifiedDate` (V1 обновляет при работе из App — по данным, не по коду) или `Session.LastActivity` (сайт). Одной `Session`, как в V3, мало — она только для сайта; по ней выпадали бы `billing` TORPEDO (6 334 правки) и `Vlad` Baltic. Итог 17 пользователей: TORPEDO 3, DDM 8, Baltic 2, BEST AMERICAN 2, BEST SOURCE 1, LEVSKI 1 | блок 0 [[data-migration]] |
| Строки `varchar` | **Решено 2026-11-13:** все строковые колонки dispatch — `varchar` (в данных латиница, БД `autodispatch` в UTF-8 collation `Latin1_General_100_CI_AI_SC_UTF8`, потерь нет). В `DispatchMap.Configure` — `SetIsUnicode(false)` для всех string; миграция поверх `Dispatch` — пользователь | [[data-model]], миграция |
| Геоданные `geography` | **Решено 2026-11-13: заменить на пары `Latitude/Longitude decimal(9,6)`**, как в V3 (`MapPointDto`), без NetTopologySuite и `Microsoft.SqlServer.Types` на Linux. Колонки: `d_Load.OriginLocation/DestinationLocation`, `d_Warehouse.Location`, `d_Yard.Location`, `CurrentLocation` у `BaseDispatchEntity` (позиции CarrierView/BNSF). Единственный пространственный запрос — радиус в `LoadHistory2` (`DispatchService.cs:988-995`, сырой SQL `STDistance`) → гаверсинус в том же SQL. Контракт `LocationDto` уже `Latitude/Longitude`. Перенос: `OriginLocation.Lat/.Long` в [[data-migration]]. Автокомплит и геокодинг — `PlaceService` через Safer, как в V3 (этапы 3–4) | этап 2 схема, этап 3 `LoadHistory2`, [[data-migration]] |
| Справочники `City/State/Country` | **Решено 2026-11-13: таблиц в БД нет, как в V3.** `City` не нужна (`d_Load` хранит город строкой, `GeocodeService` → Safer). `State` и `Country` остаются статическим классом в коде с теми же `Id/Code/Name` (снять один раз с боевых таблиц `State`, `Country`): на них ссылаются `byte` колонки `d_SetupPacket.Physical/Mailing/W9StateId`, `Physical/MailingCountryId`, `d_SetupPacketFactoringCompany.StateId` — переносятся как есть, без FK. Читают `ReportService` (PDF), `SetupPacketController`/`PrintController` (списки), `FactoringCompanyService` (поиск по штату — в памяти), `GeocodeService`, SQL `DispatchService.cs:3689` | этап 2 схема, этапы 3 и 5 |
| Поддомен | **Решено 2026-11-11:** `dispatch.myautocall.net` | nginx, certbot, `Settings.WebUrl` |
| БД | **Решено 2026-11-11:** `db.myautocall.net`, базы `autodispatch` + `autodispatch_log` (без суффикса `_v3`) | [[deploy]], строки подключения, `DataBase/create autodispatch.sql` |
| Пользователи | **Решено 2026-11-11: своя Identity-БД.** Переносятся пользователи 7 компаний только с ролями AutoDispatch: Agent, Warehouse, Dispatcher, Sales, Accounting, Owner, Admin (`Permissions.cs:11-27`); остальные роли V1 не заводятся. Роли сохраняются как есть (на них завязаны `MinAccessAttribute` и `RolesOperator`); пароли — хэш Identity v2 | Seed, перенос, `RoleEnum` |
| Активные компании | **Решено 2026-11-11:** компании с `d_Event` за 6 месяцев — 7 компаний, их пользователи, данные и документы | [[data-migration]] |
| `d_Event` | **Решено 2026-11-11:** переносить для активных компаний, `Id` сохраняются | [[data-migration]] |
| `d_Message`, `ChatService` | **Решено 2026-11-11:** не переносить. В App чат мёртвый код, на Web используется только AutoCall-стороной | контракт (−2 метода), App |
| `ReportSimilarLoads` | **Рекомендация: не портировать.** Единственный вызов — «Similar Loads» из карточки лоадборда AutoCall. Побочно: в AutoCall V3 эта кнопка после разделения БД либо ходит в dispatch-API, либо отключается — решение для AutoCallV3 | контракт (−1 метод) |
| Протокол `Request` | **Рекомендация:** словарь `Name → handler`, построенный из `IDispatchService` при старте (reflection один раз), `SerializationBinder` на `$type` — [[protocol]] | `ApiDispatchController` |
| Telerik Documents на Linux | проверить `Telerik.Documents.*` на RTF→PDF шаблонов `Documents/*.rtf`; запасной — QuestPDF с переписыванием шаблонов | этап 3, `ReportService` |
| SetupPacket фронт | knockout как есть → Stimulus позже, или сразу Stimulus | этап 5, сроки |
| Quartz-джобы dispatch | снять список с прод-конфига; в коде найден только SQL чистки истории | этап 3 |
| Бэкап `/files` | 150 ГБ на хосте без бэкапа; сейчас файлы бэкапятся вместе с Windows-сервером | хостинг |
