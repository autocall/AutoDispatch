# AutoDispatch Web — скелет сервера (этап 1)

`Sources/Web/AutoDispatch.Web.sln` — копия `Sources/Web` AutoCall V3 (состояние 2026-11-11) с переименованием `AutoCall.Web.*` →
`AutoDispatch.Web.*` и вырезанной AutoCall-спецификой. Архитектура, слои, логирование и фронт — как в эталоне
(`AutoCallV3/Repo/AI/wiki/{web-architecture,frontend,logging}.md`); здесь только то, чем копия отличается.

## Что осталось от V3

Common (`AppSettings` partial: `General`, `Secrets`, `Environment.Remote/Local`), Data (`AdDbContext` Identity int-ключи,
`LinqToDbContext`, `AdLogDbContext`/`LogLinqToDbContext`, `UnitOfWork`, репозитории, `Company`, `AdUser`, `UserSetting`),
Logic (`ServicesHost`, `BaseService`, `AuthService` JWT, `UserService`, `CompanyService`, `LoginService`, `LogService`,
`MetricService`, `MonitorService`, `PasswordService`, `DownloadService`, `MapService`, `PlaceService`, `UserSettingService`;
`WebServices/` Mailgun и Brokersnapshot), Server (`Account`, `Admin{Companies,Users,Logs,Monitor,Metrics}`, `ApiAccount`,
`ApiLogs`, `ApiMetrics`, `ApiMaps`, `ApiPlaces`, `Map`, `Download`, `Home`, `Test`; `RequestContextMiddleware`, `ExceptionHandler`,
фоновые `ActivityFlush`, `Retention`, `MetricBackground`), фронт (`Client/` htmx + Stimulus + Tailwind v4 + daisyUI, те же
контроллеры), тесты `01_Account`, `02_Places`, `04_Metrics`, `05_Logging`.

## Что вырезано

| Удалено | Где было |
|---|---|
| `Pattern`, `PreparedPattern`, `RecordFilter`, `BasePatternEntity`, `PatternEnums`, `EmailAccount/EmailMessage`, `AppMaps` | Data |
| `PatternService`, `PreparedPatternService`, `RecordFilterService`, `EmailService`, `CallReportService`, `VodiaWebService`, `DatSoap/DatAlarmQueue`, `PatternDtos`, `CallReportDtos`, email-DTO | Logic |
| `ApiPatterns`, `ApiPreparedPatterns`, `ApiRecordFilters`, `ApiSettings`, `ApiEmail`, `ApiProxy` (Truckstop), `ApiDatSoap`, `CallReport`, `Email`, блоки Congestion/Emails на Home, views `CallReport/`, `Email/` | Server |
| `AdUser.AutoCall/SipLogin/SipPassword`, `SessionDto.AutoCall/Sip*/Email*`, `UserDto.SipLogin/DataSources`, колонки Data Source и AutoPhone в `/Admin/Users` | модель и админка |
| `AppSettings`: SMTP Gmail, Vodia, DatSoap; `Secrets:Vodia` | Common |
| Метрики DAT v2/v1, Truckstop, MicroSIP, AbtoPhone (`MetricTypeEnum` 31–46, `ClientMetricsDto.Sources/Calls/SipRegistered`, колонки `/Admin/Metrics`) | Logic, Server |
| Тесты `03_DatSoap`, `06_CallReport` | Test |

## Отличия от V3, записанные как решения

- **`UserSetting` оставлен** только для секции `Map` (настройки карты `/Map`: тип, трафик, погода, тема). План называл его в
  удаляемом; без него `MapController.SaveSettings` и `map_controller.ts` теряли бы сохранение. `Api/Settings` (секции App) убран,
  `UserSettingService` — только `GetMapAsync/SetMapAsync`.
- **`RoleEnum`** = роли V1 с модулем AutoDispatch: Agent 1, Warehouse 2, Dispatcher 3, Sales 4, Accounting 5, Owner 6, Admin 7
  (решение этапа 0). Seed создаёт их все; форма пользователя по умолчанию Dispatcher. Права по ролям — этапы 3–6.
- **Метрики**: остались Web CPU, DB CPU, Geocoding, Online, App exceptions; показатели dispatch — этап 8.
- **Окружение**: `ApplicationName` `AutoDispatch.Web`, cookie `AutoDispatch.Auth`, `Website` `https://dispatch.myautocall.net`,
  БД `autodispatch`/`autodispatch_log` (локально `autodispatch_local`/`autodispatch_log_local`, SQL Express, collation UTF8),
  `FilesPath` (`/files/`, локально `D:\Develop\AutoDispatch\Data\files\`), локальные порты 7092/7082 (V3 занимает 7091/7081),
  `DownloadApplicationFile` `AutoDispatch.application`, User-Agent App `AutoDispatch/`.
- **Префикс `Ad` вместо `Ac`** (решено 2026-11-13): `AdUser`, `AdRole`, `AdDbContext`, `AdLogDbContext`, `AdUserStore`, `AdClaimsPrincipalFactory` и т.д. — классы и файлы;
  имена таблиц (`IdentityUsers`, `Company`) не менялись.
- **Identity и `Company` на Guid** (решено 2026-11-13, [[plan]]): `AdUser : IdentityUser<Guid>`, `AdRole` и остальные Identity-типы `<Guid>`,
  `BaseEntity.Id`/`CreatedById`/`ModifiedById`, `UserSetting.UserId`, `UserLogin`/`UserActivity` в БД логов — Guid. Новые ключи — `Guid.NewGuid()`
  (`DbRandomHelper.NewInt32` удалён, `NewInt64` остался для БД логов). Системные Id: компания и System `…-000000001000`, Admin `…-000000001001`;
  Id роли = `RoleEnum.ToRoleId()` (`Enums/RoleEnumExtension.cs`), обратно — `ToRoleEnum()` в памяти (в SQL не переводится, `UserService.GetAllAsync`).
  Ссылка карты (`MapService`): в payload вместо int32 — 16 байт Guid. Маршруты админки `{id:guid}`, формы: `Id == Guid.Empty` — создание.
  Миграция `Init` от 2026-11-12 сделана под int — пересоздаётся пользователем.
- **Миграции не копировались** (их делает пользователь — CLAUDE.md): `Migrations/` пуст, при старте сервер пишет предупреждение
  и пропускает seed. Для `Log` таблицу создаёт sink сразу с колонками `Details`/`Session`, индексы — `DataBase/create log indexes.sql`
  после первого запуска (у V3 это делала миграция `Alter_Log`).
- `ExceptionFormatter` берёт кадры `AutoDispatch.*`.

## Deploy и скрипты

`Deploy/AutoDispatch.Web/{Dockerfile,appsettings.Production.json.example}` (+ volume `/files`), `Deploy/nginx/dispatch.myautocall.net.conf`
(порт 5005, `client_max_body_size 100m` — предел уточнить), `Deploy/server-setup.txt`, `Deploy/metric_cpu.sh`,
`Scripts/publish-autodispatch-web.ps1` (контейнер `autodispatch.web`, `-v /home/autodispatch/files:/files`), `Scripts/commands.txt`,
`DataBase/create autodispatch*.sql`, `grant metrics autodispatch.sql`, `create log indexes.sql`. Подробности — [[deploy]].

## Проверено 2026-11-11

`dotnet build` без ошибок; сервер стартует на `https://localhost:7092`, `GET /Test` → `{ok:true}`, `/Account/LogIn` 200,
`/Admin/Monitor` без cookie → 302 на логин; sink создал таблицу `Log` в `autodispatch_log_local`; офлайн-тесты `Logging_Test`
и `Metrics_Test` (8) проходят. Не проверено: вход админа, `/Admin/Monitor` и `Api/Account/SignIn` — ждут миграции `Init`
(пользователь); сборка фронта `npm run build` — Node под пользователем Nata недоступен (nvm стоит в профиле Administrator),
локально подложен `wwwroot/build` из V3 (gitignored, тот же `app.ts`); публикация контейнера — после миграций.
