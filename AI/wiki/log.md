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
Ждёт пользователя: миграции `Init` (`AcDbContext`, `AcLogDbContext`), сборка фронта (Node), первая публикация. Страница [[web]].

## [2026-11-12] build | блок 0 переноса — компании и пользователи

Написан `DataBase/migrate from autocall dispatch.sql` (только блок 0): активные компании по `d_Event`, пользователи с ролями AutoDispatch и входом за 6 месяцев, все роли, постоянные таблицы маппинга `_MigrateCompany`/`_MigrateUser`. На боевой БД не прогонялся (нет доступа из сессии). [[data-migration]] дополнена.
