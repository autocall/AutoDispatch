# Перенос данных из `autocall`

Скрипт `DataBase/migrate from autocall dispatch.sql` по образцу `AutoCallV3/Repo/DataBase/migrate from autocall.sql`:
разовый T-SQL на `db.myautocall.net` под sa, `autocall.dbo.*` → новая БД, блоками, перезапуск перезаливает блок целиком.
Отличие от V3: ключи Guid сохраняются, маппинга `EntityId` нет; маппятся только `CompanyId/UserId` → новые Identity-идентификаторы
(**решить** в [[plan]]: Guid остаются). Замеры сняты 2026-11-11 на боевой `autocall`.

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
| 0 | 7 компаний, их пользователи с ролями dispatch (Agent, Warehouse, Dispatcher, Sales, Accounting, Owner, Admin) | список в скрипте, как список Admin по email в V3; временные таблицы маппинга `CompanyId/UserId` |
| 1 | справочники: 25 `d_*` без `d_Message`, `d_Load`, `d_Event`, `d_HistoryObject`, `d_HistoryLocation*` | `insert … select … where CompanyId in (активные)`, одной транзакцией на таблицу |
| 2 | `d_Load` + `d_HistoryLocationLoad/Truck/Trailer/Driver/DatDriver` | то же |
| 3 | `d_HistoryObject` | пакеты по 100 000 `Id` в транзакции (~50 пакетов), `set identity_insert on`, возобновляемо с последнего `Id`, в конце `dbcc checkident reseed` |
| 4 | `d_Event` активных компаний с `HistoryId` среди перенесённых снимков | пакеты по 500 000 `Id` (~20), `identity_insert`, reseed |
| 5 | файлы | см. ниже |

`Id` в блоках 3–4 сохраняются: на `d_HistoryObject.Id` ссылаются `HistoryId` в `d_Load`, `d_HistoryLocation*`, справочниках;
`d_Event.Id` — точка отсчёта `lastEventId` клиента и `max(Id)` сервера.

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
