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
