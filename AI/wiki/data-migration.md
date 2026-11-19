# Перенос данных из `autocall`

Скрипт `DataBase/migrate from autocall dispatch.sql` по образцу `AutoCallV3/Repo/DataBase/migrate from autocall.sql`:
разовый T-SQL на `db.myautocall.net` под sa, `autocall.dbo.*` → новая БД, блоками, перезапуск перезаливает блок целиком.
Отличие от V3: ключи Guid сохраняются, в том числе `Users.Id` и `Company.EntityId` — Identity в [[web]] на Guid (решено 2026-11-13),
маппинга нет вообще: `CompanyId`, `CreatedById`, `LastModifiedById` в `d_*` копируются как есть. Замеры сняты 2026-11-11 на боевой `autocall`.

## Что переносим — решено 2026-11-11

Только компании с событиями за последние 6 месяцев, их пользователи, данные и документы:

```sql
select distinct CompanyId from autocall.dbo.d_Event where CreatedDate >= dateadd(month, -6, getdate())  -- 7 компаний
```

Не переносим: `d_Message` и `ChatService`, `RecyclePath`, данные неактивных компаний — остаются в V1 как архив.

В скрипте окно зафиксировано датой **2026-05-18** (6 месяцев до первого переноса 2026-11-18), чтобы догон не менял набор компаний — см. «Догон».

## Замеры

| Объект | Всего | Активные 7 компаний |
|---|---|---|
| `d_Load` | 164 178 | 89 384 |
| `d_Event` | 17 552 275 строк, 1.5 ГБ с индексами (~45 байт/строка) | 10 607 986, все с `HistoryId`, с 2018-11-27 |
| `d_HistoryObject` | 6 510 818 строк, 16 ГБ данных (~2.5 КБ/снимок) | 4 849 812 строк, 7.9 ГБ после отбора по ссылкам |
| Файлы `E:\Files` | 570 222 файла, 255.2 ГБ | 323 479 файлов, 147.5 ГБ (75 353 каталога из 115 731 сущности) |

Выводы: БД переносится за час, тяжёлое — только документы; место на хосте есть (проверено 2026-11-11).

## Блоки скрипта

| Блок | Что | Способ |
|---|---|---|
| 0 | 7 компаний, их пользователи только с ролями AutoDispatch (Agent, Warehouse, Dispatcher, Sales, Accounting, Owner, Admin) и статусом allowed, другие роли V1 не переносятся; шаг 0.5 — настройки App по секциям `UserSetting`, только dispatch ([[user-settings]]) | **написан 2026-11-12, переписан под Guid 2026-11-13**, шаг 0.5 — 2026-11-17 (см. ниже) |
| 1 | 19 таблиц объектов и справочников компаний (`DispatchCompany*`, `DatCompany`, `DatDriver`, `Driver`, `Truck`, `Trailer`, `Warehouse`, `Yard`, `SetupPacket*`, `Customer/ShipperReceiverCompany`, `CompanyCustomStatus`, `Defect*`) | **написан 2026-11-13**: одна транзакция, `delete` + `insert … select … where CompanyId in (#active)`; setup-packet контакты/агентства/рекомендации — по `#sp` активных компаний |
| 2 | `Load`, `HistoryLocationLoad/Truck/Trailer/Driver/DatDriver` | **написан 2026-11-13**: одна транзакция, `identity_insert` + reseed, история положений — по объектам активных компаний |
| 3 | `HistoryObject` | **написан 2026-11-13**: отбор в постоянную `dbo._MigrateHistoryKeep` (с 2026-11-18 дополняется при каждом запуске — догон), пакеты по 100 000 `Id` без общей транзакции, возобновляемо с `max(Id)`, затем докачка пропущенных ниже `max(Id)`, reseed |
| 4 | `Event` активных компаний с `HistoryId` среди перенесённых снимков (или null) | **написан 2026-11-13**: пакеты по 500 000 `Id`, возобновляемо, reseed |
| 5 | файлы | **написан 2026-11-13, исправлен 2026-11-18**: список каталогов, нужных App, по перенесённым строкам `autodispatch` (все сущности с файлами, Guid в нижнем регистре) — для проверки; сами файлы — весь `E:\Files` FileZilla (2026-11-19), см. «Файлы» |
| 6 | FK | на время блоков 1–5 `nocheck constraint all` (блоки идут не в порядке зависимостей: `HistoryId` → блок 3), в конце `dbcc checkconstraints` + `with check check constraint all` |

`Id` в блоках 3–4 сохраняются: на `d_HistoryObject.Id` ссылаются `HistoryId` в `d_Load`, `d_HistoryLocation*`, справочниках;
`d_Event.Id` — точка отсчёта `lastEventId` клиента и `max(Id)` сервера.

Схема приёмника — [[data-model]] (2026-11-13): таблицы без `d_` (`autocall.dbo.d_Load` → `autodispatch.dbo.Load`), `d_ActualCompany` →
`DispatchCompany`, `d_ActualCompanyCustom` → `DispatchCompanyCustom`, колонки `ActualCompanyId` → `DispatchCompanyId` (`Load`, `DatCompany`,
`DispatchCompanyCustom`); `geography` → пары: `OriginLocation.Lat / .Long` → `OriginLatitude/OriginLongitude`, так же `DestinationLocation`,
`Location` (`Warehouse`, `Yard`, `HistoryLocation*` → `Latitude/Longitude`), `CurrentLocation` → `CurrentLatitude/CurrentLongitude`.
Поля `*StateId/*CountryId` и `CreatedById/LastModifiedById/UserId` копируются как есть (FK нет). Остальные колонки — один в один.

### Блок 0 — компании и пользователи (написан 2026-11-12, переписан 2026-11-13, прогнан на хосте 2026-11-18 — см. «Пробный перенос на хосте»)

Отбор: компании с `d_Event` за 6 месяцев → их пользователи `IsDeleted = 0`, `VerifyId = 2` (allowed; waiting и blocked не нужны — в V3
статус один, `IsDeleted`, см. [[plan]]) с хотя бы одной ролью AutoDispatch (имена ролей
`Roles.Name` без пробелов = `RoleName` V1) **и входом за 6 месяцев** (`autocall_reference.dbo.Session.LastActivity`, как в V3 —
бывшие сотрудники без входа не нужны). Email, уже имеющийся в `IdentityUsers`, пропускается; компания с тем же `Name` переиспользуется.

Отличия от V3-скрипта: роли переносятся **все**, какие есть у пользователя (в V1 `RoleName` — флаги, клиент считает сумму `roles`,
[[protocol]]), `Admin` V1 остаётся `Admin` (в V3 Admin давался по списку email, остальные → Owner) — в [[web]] `GetRoleAsync`
берёт первую роль, форма `/Admin/Users` при сохранении оставит одну; это принять или поправить на этапах 3–6. Ключи сохраняются:
`Users.Id` → `IdentityUsers.Id`, `Company.EntityId` → `Company.Id`, Id ролей = `RoleEnum.ToRoleId()` (`00000000-0000-0000-0000-00000000000N`);
`Users.Signature` → `IdentityUsers.Signature` (подпись агента в confirmed-письме, `varchar(64)`, миграция `Alter_Dispatch` 2026-11-17; в `autocall`
nvarchar(max), заполнено у 10 пользователей, максимум 13 символов — `left(…, 64)`);
повторный запуск удаляет из `autodispatch` всех, чьи Id есть в `autocall`, и заливает заново. `UserName` = `Users.Name` (отображаемое имя,
пусто → email); `NormalizedUserName` уникален, поэтому к повторяющимся именам добавляется email. `TimeZoneId = 'UTC'`, как в V3.
В конце скрипт выводит итоги и список пользователей активных компаний с ролью AutoDispatch, которые **не** попали в перенос — для контроля.
`Company` — **все** компании `#active`, в том числе без переносимых пользователей (решено 2026-11-18): блоки 1–4 копируют данные по `#active`,
и раньше, когда `Company` брались только по `#user`, у UA LOGISTICS (0 пользователей allowed) в блоке 6 падал `FK_Truck_Company_CompanyId`.
Пользователей такой компании можно завести руками. При догоне `Company` не удаляется (на неё ссылаются FK данных) — добавляются только новые.

### Отбор `d_HistoryObject` — правило QuartzService наоборот

Берём снимки, на которые ссылается перенесённый объект, плюс все снимки грузов активных компаний не в статусе Paid (`Status <> 6`).
Запрос, которым получены 4 849 812 / 7 932 МБ:

```sql
declare @from datetime = dateadd(month, -6, getdate());
with c as (select distinct CompanyId from autocall.dbo.d_Event where CreatedDate >= @from),
     refs as (
       select HistoryId as Id from autocall.dbo.d_Load l join c on c.CompanyId = l.CompanyId where HistoryId is not null
       union select hl.HistoryId from autocall.dbo.d_HistoryLocationLoad hl join autocall.dbo.d_Load l on l.EntityId = hl.LoadId join c on c.CompanyId = l.CompanyId
       union select hl.HistoryWarehouseId from autocall.dbo.d_HistoryLocationLoad hl join autocall.dbo.d_Load l on l.EntityId = hl.LoadId join c on c.CompanyId = l.CompanyId
       union select hl.HistoryDatCompanyId from autocall.dbo.d_HistoryLocationLoad hl join autocall.dbo.d_Load l on l.EntityId = hl.LoadId join c on c.CompanyId = l.CompanyId where hl.HistoryDatCompanyId is not null
       union select HistoryId from autocall.dbo.d_Driver d join c on c.CompanyId = d.CompanyId
       union select HistoryId from autocall.dbo.d_Truck d join c on c.CompanyId = d.CompanyId
       union select HistoryId from autocall.dbo.d_Trailer d join c on c.CompanyId = d.CompanyId
       union select HistoryId from autocall.dbo.d_Warehouse d join c on c.CompanyId = d.CompanyId
       union select HistoryId from autocall.dbo.d_DatCompany d join c on c.CompanyId = d.CompanyId
       union select HistoryId from autocall.dbo.d_DatDriver d join c on c.CompanyId = d.CompanyId
       union select HistoryId from autocall.dbo.d_SetupPacket d join c on c.CompanyId = d.CompanyId
       union select HistoryId from autocall.dbo.d_HistoryLocationTruck
       union select HistoryId from autocall.dbo.d_HistoryLocationTrailer
       union select HistoryId from autocall.dbo.d_HistoryLocationDriver
       union select HistoryId from autocall.dbo.d_HistoryLocationDatDriver),
     keep as (
       select Id from autocall.dbo.d_HistoryObject where Id in (select Id from refs)
       union
       select h.Id from autocall.dbo.d_HistoryObject h
         join autocall.dbo.d_Load l on l.EntityId = h.ReferenceId
         join c on c.CompanyId = l.CompanyId
        where l.IsDeleted = 0 and l.Status <> 6)
select count(*) as rows_keep, sum(datalength(h.Data)) / 1024 / 1024 as mb_keep
  from autocall.dbo.d_HistoryObject h join keep k on k.Id = h.Id;
```

Для старых Paid-грузов история теряется — её и сейчас чистит Quartz через 30 дней, для пользователей это не изменение.

## Догон (решено 2026-11-18)

Перед сверкой этапа 7 и в окно переключения этапа 8 перенос догоняется до текущего состояния V1 тем же скриптом — **целиком, в одной сессии под sa**
(правки скрипта под догон от 2026-11-18 на боевой ещё не выполнялись — первый догон их и проверит):

1. Шапка — `#active` по `d_Event` с **2026-05-18**: дата зафиксирована, со скользящим окном UA LOGISTICS (последнее событие 2026-06-08)
   выпала бы 2026-12-08, а её снимки и события остались бы без `Company`.
2. Блок 0 — пользователи, роли, настройки удаляются по Id из `autocall` и заливаются заново; `Company` — только новые.
3. Шаг «записи нового сервера» — только если в `autodispatch` уже писал новый сервер (ниже).
4. Блоки 1–2 перезаливаются целиком.
5. Блок 3 — `_MigrateHistoryKeep` дополняется ссылками, появившимися после прошлого прогона; снимки копируются после `max(Id)`, затем докачиваются
   пропущенные ниже `max(Id)` (Undo, груз вышел из Paid).
6. Блок 4 — события после `max(Id)`. Старые события, чей снимок попал в отбор только сейчас, не докачиваются: полный проход по `d_Event`
   слишком дорог, теряется только их история.
7. Досинк файлов: `robocopy /MAXAGE` на V1 → FileZilla → регистр каталогов («Файлы», шаг 4).
8. Блок 6 — FK с проверкой.
9. Записать в таблицу прогонов `maxId` из итогов блоков 3 и 4.

**Записи нового сервера.** Сервер нумерует `Event` и `HistoryObject` после `max(Id)` переноса — из того же диапазона, что новые записи V1.
Если после догона App этапа 7 работал с `autodispatch`, перед следующим догоном его записи удаляются: в скрипте после `nocheck` раскомментировать
`delete from dbo.Event where Id > …` и `delete from dbo.HistoryObject where Id > …` с `maxId` прошлого прогона. Всё остальное, что он записал
(грузы, объекты, пользователи, настройки), перезальют блоки 0–2; его файлы в `/files` остаются (мелочь; при желании — каталоги, изменённые после начала проверки).
Альтернатива — перенос заново в пустую БД (дольше: 7.9 ГБ снимков).

**Порядок для этапа 7:** файлы FileZilla (можно заранее) → догон БД → досинк файлов → записать `maxId` → сразу сверка бордов V1 и App
под одним пользователем (V1 продолжает работать, данные расходятся с каждым часом) → проверка цикла груз → … → история в App.

| Прогон | Снимок V1 | `HistoryObject` maxId | `Event` maxId |
|---|---|---|---|
| 2026-11-18, первый перенос | последнее событие 10:13 UTC | 6 578 853 | 17 587 102 |

## Файлы — весь `E:\Files` через FileZilla (решено 2026-11-19)

**Решение пользователя 2026-11-19:** документы переносятся **все** (весь `E:\Files`, 255 ГБ на 2026-11-11, а не 147.5 ГБ активных компаний) —
FileZilla по уже настроенным подключениям в промежуточный `/home/safer/temp` (уточнено 2026-11-19), оттуда на хосте —
`Scripts/copy-dispatch-files.sh`. Отступление от «данные неактивных компаний не переносим»: их строки в БД не переносятся,
поэтому документы неактивных компаний лежат на хосте, но App и сервер до них не доходят (каталог ищется только по `EntityId` перенесённой строки).
Место на хосте есть: `/` 878 ГБ, свободно 765 ГБ (2026-11-19) — хватает на две копии (255 ГБ в `temp` + 255 ГБ в `files`), обе на `/dev/md2`.
Цель — `/home/autodispatch/files/{EntityId}/…` (volume `/files`). Служебные `_recycle`, `_temp` (создал сервер) и `_fonts` скрипт не трогает.
`/home/safer/temp` (`drwxrwxrwx`, на 2026-11-19 в нём только `input.html`, `output.pdf` от 2026-03-19; ни cron, ни контейнеры его не чистят)
используется как приёмник; скрипт берёт из него только каталоги с именем-Guid.

Каталог = `EntityId` сущности ([[v1-inventory]]): груз, карриер (`DatCompany`), setup packet, водитель, трак, трейлер, дефект и ремонт, шаблоны
`DispatchCompany` (V1 `FileGetFullPath`, сервер `DispatchFileHelper`). Сервер строит путь через `Guid.ToString()` — **нижний регистр**, а на Linux
регистр важен. Блок 5 скрипта — список каталогов, которые нужны App (по перенесённым строкам `autodispatch`, `IsDeleted = 0`, нижний регистр);
на 2026-11-18 — 120 832 сущности: Load 87 594, DatCompany 23 797, SetupPacket 4 532, Trailer 4 433, Truck 135, Driver 127, Defect 93,
DefectRepair 73, DispatchCompany 48. Раньше список брал только грузы, карриеров и setup packet — замер 147.5 ГБ сделан по ним.

1. **Первичная заливка** FileZilla: `E:\Files\*` → `/home/safer/temp/` (или вся папка `E:\Files` → `/home/safer/temp/Files` — скрипт берёт её сам).
   Если на каком-то участке FTP, а не SFTP, — тип передачи Binary (в Auto FileZilla меняет концы строк у `.txt`/`.html`). Записать дату начала —
   от неё считается досинк.
2. **Копирование в `files`** — `Scripts/copy-dispatch-files.sh` на хосте под root (написан 2026-11-19, проверен на хосте на тестовых каталогах в `/tmp`):
   rsync всех каталогов-Guid из `/home/safer/temp` в `/home/autodispatch/files`, имя каталога в нижнем регистре (каталоги уже в нижнем — одним
   rsync по списку, остальные по одному с переименованием), владелец root, права 755/644; повторный запуск докопирует только новое.
   Скрипт и `dirs.txt` в рабочей копии с CRLF — на хост через `sed 's/\r$//'` (команды в шапке скрипта).
   ```sh
   bash /root/copy-dispatch-files.sh --check /root/dirs.txt
   ```
3. **Проверка** — `--check dirs.txt` (блок 5): сколько нужных App каталогов нет в `files`, список — `/home/autodispatch/files-missing.txt`
   (у части сущностей файлов нет — отсутствие сверяется с `E:\Files`, а не считается ошибкой).
4. **Досинк** (перед сверкой этапа 7 и в окно переключения): на V1 `robocopy E:\Files D:\files-delta /S /MAXAGE:YYYYMMDD` (дата начала прошлой
   передачи; V1 пишет файл заново, поэтому дата изменения = дата загрузки) → FileZilla `D:\files-delta\*` → `/home/safer/temp/` с заменой →
   снова `copy-dispatch-files.sh` → очистить `D:\files-delta`. Файлы, удалённые в V1 после заливки (перенос в `RecyclePath`), на хосте остаются
   и будут видны в App — принято (мелочь).
5. `/home/safer/temp` с копией удалить после переключения (этап 8), не раньше: он источник досинка.

`Scripts/sync-dispatch-files.ps1` (`tar | ssh` пачками по `dirs.txt`, 2026-11-18) — **не используется**: на сервере V1 нет `tar` и `ssh`
(Server 2016 или старше), выбран FileZilla. Оставлен как запасной вариант (параметры `-Tar`, `-Ssh`, проверен локально с заглушкой ssh).
Замер размеров 2026-11-11 — `files-size.ps1` (HashSet по `ids.txt`, регистр Guid в нижний).

## Окно переключения

Остановить запись в V1 → догон (весь скрипт, «Догон»; если App этапа 7 писал в `autodispatch` — сначала удалить его `Event`/`HistoryObject`)
→ досинк файлов («Файлы», шаг 4: дельта FileZilla в `/home/safer/temp` + `copy-dispatch-files.sh`) → ссылка ClickOnce нового App пользователям 7 компаний → после проверки удалить
`dbo._MigrateHistoryKeep` и `/home/safer/temp` с копией `E:\Files`. Укладывается в один вечер; DNS не трогается, у нового сервера свой поддомен.

## Проверка на боевой БД — 2026-11-13 (MCP `db-readonly`, `sqlcmd`)

- **Колонки**: все колонки 28 таблиц `d_*` есть в `autodispatch` (с учётом переименований). Отличия типов — только расширяющие: в V1 часть строк
  `varchar` (`Load`, `DatCompany`, `SetupPacket` — там, где стоял `DataType("VARCHAR")` и миграция была поправлена руками), у нас везде `nvarchar`;
  `DefectRepair.Date`, `Event.CreatedDate`, `Load.LastStatusChangedDate/BindDate/AssignedTrailerDate`, `DispatchCompany.BnsfLastDone`,
  `SetupPacket.EmailSent/WorkersCompensationSignatureDate` — `datetime` → `datetime2`. Оставлено как есть, `insert … select` проходит без конвертаций.
- **Ссылки**: по всем FK `Load` и остальных таблиц для активных компаний «висячих» нет, кроме трёх удалённых грузов (`IsDeleted = 1`) одной из активных компаний,
  ссылающихся на `DatCompany`, `Shipper/Receiver` и `ActualCompany` неактивной компании `82C1221B…` — блок 6 обнуляет.
  `HistoryId` всех объектов и `HistoryLocation*` есть в `d_HistoryObject`; `Location`/`OriginLocation` не null; координаты влезают в `decimal(9,6)`.
- **Замеры** (6 месяцев на 2026-11-13): `d_Load` 164 284 / 89 490 активных, `d_DatCompany` 53 973 / 24 502, `d_ShipperReceiverCompany` 65 067 / 36 054,
  `d_CustomerCompany` 10 938 / 5 938, `d_SetupPacket` 12 882 / 4 531, `d_Trailer` 6 906 / 4 706, `d_DatDriver` 1 502 / 1 332, `d_CacheLoadIndex` 67 917 / 38 656 (не переносится, 2026-11-16),
  `d_ActualCompany` 150 / 52, `d_Driver` 298 / 147, `d_Truck` 330 / 149, `d_Warehouse` 51 / 17, `d_Yard` 14 / 5, `d_SetupPacketFactoringCompany` 188 (все).
- **Активность 7 компаний** (события за 6 мес / создано грузов): TORPEDO 517 979 / 2 701, Baltic 24 561 / 1 478, DDM 15 568 / 272, BEST SOURCE 120 / 10,
  LEVSKI 8 / 4, UA LOGISTICS 2 / 1, BEST AMERICAN 1 / 0. Две последние фактически неактивны, все их пользователи blocked/deleted — переносятся данные без пользователей.
- **Пользователи, правило «вход за 6 месяцев» не годится**: `autocall_reference.dbo.Session` пишется только сайтом (User-Agent браузера), входы App
  там не отражены. У TORPEDO пользователь `billing` (6 334 правки грузов за полгода) и у Baltic `Vlad` (94) по Session «не входили» и выпали бы.
  **Решено 2026-11-13**: признак входа = `Users.LastModifiedDate >= 12 мес` (окно год, у компаний 6 месяцев) (обновляется при работе из App: у `billing` без веб-сессий — 2026-11-06,
  у всех кандидатов заполнен `AppVersion`) **или** `Session.LastActivity`. Кандидатов (`IsDeleted = 0`, `VerifyId = 2`, роль) 19, с входом за год 17:
  выпадают `Nina` DDM (2023) и один из TORPEDO; за 12 месяцев событий у 8 компаний против 7 за 6 — окно компаний не трогаем. Дублей отображаемых имён нет, пустых `Users.Name` нет.
- Системный `Support` (Admin, компания вне активных) правит грузы всех компаний — в V3 его роль выполняет штатный Admin из Seed.

## Пробный перенос на хосте — 2026-11-18 (проверено через MCP `db-readonly`)

Пользователь создал `autodispatch` на `db.myautocall.net` (compat 150, `Latin1_General_100_CI_AI_SC_UTF8`), применил миграции
`20261113085341_Init` и `20261118085359_Dispatch` и прогнал скрипт на данных 2026-11-18. Это база для проверки этапа 7 ([[plan]] «Решить»)
и репетиция окна переключения. Логин `ClaudeReadonly` читает `autodispatch`, `autodispatch_log` — нет.

- **Количество строк совпадает с `autocall` по компаниям `autodispatch.Company`**: `Load` 89 568, `DatCompany` 24 504, `ShipperReceiverCompany` 36 096,
  `CustomerCompany` 5 938, `SetupPacket` 4 532, `Trailer` 4 706, `DatDriver` 1 332, `Driver` 147, `Truck` 149, `DispatchCompany` 52, `Warehouse` 17, `Yard` 5.
  Остальное: `Event` 10 515 680, `HistoryObject` 3 938 126, `HistoryLocationLoad` 6 573 359, `HistoryLocationTrailer` 3 556 313,
  `HistoryLocationDatDriver` 166 478, `SetupPacketContact` 20 342, `SetupPacketInsuranceAgency` 13 172, `UserSetting` 201.
- **Компании и пользователи**: 8 компаний (7 активных + системная), 19 пользователей (17 перенесённых + System и Admin из Seed), у всех есть роли,
  у всех 17 есть настройки. UA LOGISTICS — без пользователей (решение выше). Грузов: BEST AMERICAN 51 349 (последний 2025-08-27), TORPEDO 20 392,
  Baltic 11 503, DDM 3 632, UA LOGISTICS 2 092, LEVSKI 590, BEST SOURCE 10.
- **FK**: все включены и доверенные (`is_disabled = 0`, `is_not_trusted = 0`) — блок 6 прошёл.
- **Снимок**: последнее событие 2026-11-18 10:13 UTC. V1 продолжает работать — через несколько часов в `autocall` уже 144 новых события
  активных компаний и 8 изменённых грузов. Перед сверкой этапа 7 — догон (решено 2026-11-18, «Догон»).
- Осталась служебная `dbo._MigrateHistoryKeep` (3 938 126 Id) — нужна для возобновления блоков 3–4; удалить после переключения (этап 8).
- Файлы не синхронизированы — переносятся FileZilla (решено 2026-11-19), «Файлы».
