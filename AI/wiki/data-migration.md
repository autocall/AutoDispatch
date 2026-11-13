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
| 0 | 7 компаний, их пользователи только с ролями AutoDispatch (Agent, Warehouse, Dispatcher, Sales, Accounting, Owner, Admin) и статусом allowed, другие роли V1 не переносятся | **написан 2026-11-12, переписан под Guid 2026-11-13** (см. ниже) |
| 1 | 19 таблиц объектов и справочников компаний (`DispatchCompany*`, `DatCompany`, `DatDriver`, `Driver`, `Truck`, `Trailer`, `Warehouse`, `Yard`, `SetupPacket*`, `Customer/ShipperReceiverCompany`, `CompanyCustomStatus`, `Defect*`) | **написан 2026-11-13**: одна транзакция, `delete` + `insert … select … where CompanyId in (#active)`; setup-packet контакты/агентства/рекомендации — по `#sp` активных компаний |
| 2 | `Load`, `CacheLoadIndex`, `HistoryLocationLoad/Truck/Trailer/Driver/DatDriver` | **написан 2026-11-13**: одна транзакция, `identity_insert` + reseed, история положений — по объектам активных компаний |
| 3 | `HistoryObject` | **написан 2026-11-13**: отбор в постоянную `dbo._MigrateHistoryKeep`, пакеты по 100 000 `Id` без общей транзакции, возобновляемо с `max(Id)`, reseed |
| 4 | `Event` активных компаний с `HistoryId` среди перенесённых снимков (или null) | **написан 2026-11-13**: пакеты по 500 000 `Id`, возобновляемо, reseed |
| 5 | файлы | **написан 2026-11-13**: запрос списка каталогов; копирование — см. ниже |
| 6 | FK | на время блоков 1–5 `nocheck constraint all` (блоки идут не в порядке зависимостей: `HistoryId` → блок 3), в конце `dbcc checkconstraints` + `with check check constraint all` |

`Id` в блоках 3–4 сохраняются: на `d_HistoryObject.Id` ссылаются `HistoryId` в `d_Load`, `d_HistoryLocation*`, справочниках;
`d_Event.Id` — точка отсчёта `lastEventId` клиента и `max(Id)` сервера.

Схема приёмника — [[data-model]] (2026-11-13): таблицы без `d_` (`autocall.dbo.d_Load` → `autodispatch.dbo.Load`), `d_ActualCompany` →
`DispatchCompany`, `d_ActualCompanyCustom` → `DispatchCompanyCustom`, колонки `ActualCompanyId` → `DispatchCompanyId` (`Load`, `DatCompany`,
`DispatchCompanyCustom`); `geography` → пары: `OriginLocation.Lat / .Long` → `OriginLatitude/OriginLongitude`, так же `DestinationLocation`,
`Location` (`Warehouse`, `Yard`, `HistoryLocation*` → `Latitude/Longitude`), `CurrentLocation` → `CurrentLatitude/CurrentLongitude`.
Поля `*StateId/*CountryId` и `CreatedById/LastModifiedById/UserId` копируются как есть (FK нет). Остальные колонки — один в один.

### Блок 0 — компании и пользователи (написан 2026-11-12, переписан 2026-11-13, на боевой БД не прогонялся)

Отбор: компании с `d_Event` за 6 месяцев → их пользователи `IsDeleted = 0`, `VerifyId = 2` (allowed; waiting и blocked не нужны — в V3
статус один, `IsDeleted`, см. [[plan]]) с хотя бы одной ролью AutoDispatch (имена ролей
`Roles.Name` без пробелов = `RoleName` V1) **и входом за 6 месяцев** (`autocall_reference.dbo.Session.LastActivity`, как в V3 —
бывшие сотрудники без входа не нужны). Email, уже имеющийся в `IdentityUsers`, пропускается; компания с тем же `Name` переиспользуется.

Отличия от V3-скрипта: роли переносятся **все**, какие есть у пользователя (в V1 `RoleName` — флаги, клиент считает сумму `roles`,
[[protocol]]), `Admin` V1 остаётся `Admin` (в V3 Admin давался по списку email, остальные → Owner) — в [[web]] `GetRoleAsync`
берёт первую роль, форма `/Admin/Users` при сохранении оставит одну; это принять или поправить на этапах 3–6. Ключи сохраняются:
`Users.Id` → `IdentityUsers.Id`, `Company.EntityId` → `Company.Id`, Id ролей = `RoleEnum.ToRoleId()` (`00000000-0000-0000-0000-00000000000N`);
повторный запуск удаляет из `autodispatch` всех, чьи Id есть в `autocall`, и заливает заново. `UserName` = `Users.Name` (отображаемое имя,
пусто → email); `NormalizedUserName` уникален, поэтому к повторяющимся именам добавляется email. `TimeZoneId = 'UTC'`, как в V3.
В конце скрипт выводит итоги и список пользователей активных компаний с ролью AutoDispatch, которые **не** попали в перенос — для контроля.

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

## Файлы — 147.5 ГБ

Каталог = `EntityId` груза, карриера или setup packet ([[v1-inventory]]), поэтому список для копирования берётся из БД:

```sql
declare @from datetime = dateadd(month, -6, getdate());
with c as (select distinct CompanyId from autocall.dbo.d_Event where CreatedDate >= @from)
select EntityId from autocall.dbo.d_Load        where CompanyId in (select CompanyId from c) and IsDeleted = 0
union select EntityId from autocall.dbo.d_DatCompany where CompanyId in (select CompanyId from c) and IsDeleted = 0
union select EntityId from autocall.dbo.d_SetupPacket where CompanyId in (select CompanyId from c) and IsDeleted = 0;
```

На Windows rsync нет; приём из `AutoCallV3/Scripts/publish-autocall-app.ps1` — `tar | ssh … tar -xf`. Файлы после записи
не меняются, поэтому синхронизируется список, а не содержимое:

1. Первичная заливка за несколько дней до переключения, в рабочее время, по списку каталогов (`Scripts/sync-dispatch-files.ps1`):
   `cmd /c "tar -C E:\Files -cf - -T dirs.txt | ssh root@67.220.74.190 ""tar -C /home/autodispatch/files -xf -"""`.
   Время — по каналу Windows-сервера: 100 Мбит ≈ 4 ч, 20 Мбит ≈ сутки; замерить на одном каталоге ~1 ГБ, при слабом канале
   бить по первому символу Guid с возобновлением.
2. Досинк в окно переключения: списки файлов с обеих сторон (`Get-ChildItem -Recurse -File` / `find . -type f`), разница → тот же `tar -T`. Минуты.
3. Замер размеров — `files-size.ps1` (HashSet по `ids.txt`, регистр Guid в нижний): так получены цифры выше.

## Окно переключения

Остановить запись в V1 → блоки 0–2 → хвосты блоков 3–4 с последнего `Id` → досинк файлов → reseed → ссылка ClickOnce
нового App пользователям 7 компаний. Укладывается в один вечер; DNS не трогается, у нового сервера свой поддомен.

## Проверка на боевой БД — 2026-11-13 (MCP `db-readonly`, `sqlcmd`)

- **Колонки**: все колонки 28 таблиц `d_*` есть в `autodispatch` (с учётом переименований). Отличия типов — только расширяющие: в V1 часть строк
  `varchar` (`Load`, `DatCompany`, `SetupPacket` — там, где стоял `DataType("VARCHAR")` и миграция была поправлена руками), у нас везде `nvarchar`;
  `DefectRepair.Date`, `Event.CreatedDate`, `Load.LastStatusChangedDate/BindDate/AssignedTrailerDate`, `DispatchCompany.BnsfLastDone`,
  `SetupPacket.EmailSent/WorkersCompensationSignatureDate` — `datetime` → `datetime2`. Оставлено как есть, `insert … select` проходит без конвертаций.
- **Ссылки**: по всем FK `Load` и остальных таблиц для активных компаний «висячих» нет, кроме трёх удалённых грузов (`IsDeleted = 1`) одной из активных компаний,
  ссылающихся на `DatCompany`, `Shipper/Receiver` и `ActualCompany` неактивной компании `82C1221B…` — блок 6 обнуляет.
  `HistoryId` всех объектов и `HistoryLocation*` есть в `d_HistoryObject`; `Location`/`OriginLocation` не null; координаты влезают в `decimal(9,6)`.
- **Замеры** (6 месяцев на 2026-11-13): `d_Load` 164 284 / 89 490 активных, `d_DatCompany` 53 973 / 24 502, `d_ShipperReceiverCompany` 65 067 / 36 054,
  `d_CustomerCompany` 10 938 / 5 938, `d_SetupPacket` 12 882 / 4 531, `d_Trailer` 6 906 / 4 706, `d_DatDriver` 1 502 / 1 332, `d_CacheLoadIndex` 67 917 / 38 656,
  `d_ActualCompany` 150 / 52, `d_Driver` 298 / 147, `d_Truck` 330 / 149, `d_Warehouse` 51 / 17, `d_Yard` 14 / 5, `d_SetupPacketFactoringCompany` 188 (все).
- **Активность 7 компаний** (события за 6 мес / создано грузов): TORPEDO 517 979 / 2 701, Baltic 24 561 / 1 478, DDM 15 568 / 272, BEST SOURCE 120 / 10,
  LEVSKI 8 / 4, UA LOGISTICS 2 / 1, BEST AMERICAN 1 / 0. Две последние фактически неактивны, все их пользователи blocked/deleted — переносятся данные без пользователей.
- **Пользователи, правило «вход за 6 месяцев» не годится**: `autocall_reference.dbo.Session` пишется только сайтом (User-Agent браузера), входы App
  там не отражены. У TORPEDO пользователь `billing` (6 334 правки грузов за полгода) и у Baltic `Vlad` (94) по Session «не входили» и выпали бы.
  **Решено 2026-11-13**: признак входа = `Users.LastModifiedDate >= 12 мес` (окно год, у компаний 6 месяцев) (обновляется при работе из App: у `billing` без веб-сессий — 2026-11-06,
  у всех кандидатов заполнен `AppVersion`) **или** `Session.LastActivity`. Кандидатов (`IsDeleted = 0`, `VerifyId = 2`, роль) 19, с входом за год 17:
  выпадают `Nina` DDM (2023) и один из TORPEDO; за 12 месяцев событий у 8 компаний против 7 за 6 — окно компаний не трогаем. Дублей отображаемых имён нет, пустых `Users.Name` нет.
- Системный `Support` (Admin, компания вне активных) правит грузы всех компаний — в V3 его роль выполняет штатный Admin из Seed.
