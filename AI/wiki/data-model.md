# Модель данных AutoDispatch Web (этап 2)

28 сущностей dispatch V1 (`Entities/Dispatch`, без `d_Message`) перенесены в `AutoDispatch.Web.Data/Entities/Dispatch` как есть:
Guid `EntityId`, `CreatedDate/CreatedById/LastModifiedDate/LastModifiedById`, `IsDeleted`, int identity у журналов.
Маппинг — `Mappings/Dispatch/DispatchMap.cs` (EF Core для миграций, LinqToDB для запросов), проверка без БД — тест `07_DispatchModel`.
Решения — [[plan]] («Решить»), перенос данных — [[data-migration]], скелет — [[web]].

## Сущности

| Группа | Классы (= имена таблиц) | База |
|---|---|---|
| Компания dispatch | `DispatchCompany` (V1 `d_ActualCompany`), `DispatchCompanyCustom` (`d_ActualCompanyCustom`) | `BaseEntity` + `CompanyId` |
| Справочники компании | `CustomerCompany`, `ShipperReceiverCompany`, `CompanyCustomStatus`, `Defect`, `DefectRepair` | `BaseEntity` + `CompanyId` |
| Карриеры | `DatCompany`, `DatDriver`, `SetupPacket`, `SetupPacketContact`, `SetupPacketInsuranceAgency`, `SetupPacketReference`, `SetupPacketFactoringCompany` (int `EntityId`) | `BaseObjectEntity` / `BaseDispatchEntity` / без базы |
| Техника и люди | `Driver`, `Truck`, `Trailer`, `Warehouse`, `Yard` | `BaseDispatchEntity` / `BaseObjectEntity` |
| Грузы | `Load` (self-reference `Parent/Children1`, `Shipment/Children2`), `CacheLoadIndex` (int identity, `CreatedDate` default `getdate()`) | `BaseDispatchEntity` |
| История | `HistoryObject` (int identity, `Data varbinary`), `Event` (int identity), `HistoryLocationLoad/Truck/Trailer/Driver/DatDriver` (int identity, `EventId` индекс) | `BaseHistoryLocationEntity` |

Базовые классы — `Entities/Dispatch/Base`: `BaseEntity` → `BaseObjectEntity` (+ `HistoryId` → `HistoryObject`, `CompanyId` → `Company` V3)
→ `BaseDispatchEntity` (+ `CurrentLatitude/CurrentLongitude`); `BaseHistoryEntity` → `BaseHistoryLocationEntity` (+ `Latitude/Longitude`).
Имена те же, что в V1; пространство имён `AutoDispatch.Web.Data.Entities.Dispatch` отдельно от `Entities.BaseEntity` V3 (Company, UserSetting).

## Отличия от V1 (все — решения 2026-11-13, [[plan]])

- **Таблицы без префикса `d_`**: имя таблицы = имя класса (`Load`, `Driver`, `Event`, …) — своя БД `autodispatch`.
- **`ActualCompany` → `DispatchCompany`**, `ActualCompanyCustom` → `DispatchCompanyCustom`; вместе с ними FK-колонки
  `ActualCompanyId` → `DispatchCompanyId` в `Load`, `DatCompany`, `DispatchCompanyCustom` и навигации `DispatchCompany1`/`DispatchCompany`.
  В контракте `AutoDispatch.Dtos` остаётся `ActualCompanyId` (8 DTO) — маппинг DTO↔модель на этапе 3 задаёт соответствие явно.
- **Координаты**: `geography` → пары `decimal(9,6)`: `Load.OriginLatitude/OriginLongitude/DestinationLatitude/DestinationLongitude` (not null),
  `Warehouse`/`Yard` `Latitude/Longitude` (not null), `CurrentLatitude/CurrentLongitude` у `BaseDispatchEntity` и `Latitude/Longitude`
  у `BaseHistoryLocationEntity` (null). Радиус в `LoadHistory2` — гаверсинус в сыром SQL (этап 3).
- **Нет FK на пользователей**: `CreatedById/LastModifiedById` (все), `BaseHistoryEntity.CreatedById`, `DatDriver.UserId` — просто Guid-колонки,
  навигаций `CreatedBy/LastModifiedBy/User` нет (как у `Company` V3): авторы могли не попасть в перенос. `CompanyId` → `Company` V3 с FK.
- **Нет FK на `State`/`Country`**: `SetupPacket.Physical/Mailing/W9StateId`, `Physical/MailingCountryId`, `SetupPacketFactoringCompany.StateId/CountryId`
  — byte-ключи справочника в коде (этап 3), навигации `PhysicalState` и т.п. убраны.
- **Все FK `DeleteBehavior.NoAction`** (в V1 снята `OneToManyCascadeDeleteConvention`): иначе SQL Server отказывает из-за нескольких путей каскада.
- **Строки — `varchar`** (решено 2026-11-13): `DispatchMap.Configure` ставит `SetIsUnicode(false)` всем string-свойствам dispatch; в V1 было вперемешку
  (`varchar` там, где `DataType("VARCHAR")` подхватили руками в миграциях, остальное `nvarchar`). БД `autodispatch` в UTF-8 collation, поэтому `varchar`
  хранит любой текст. `[MaxLength]` сохранён, `[DataType]` убран. Миграция `Dispatch` создала `nvarchar` — нужна миграция поверх.
  `Load.Background/Foreground` — `binary(4)`, даты `BOLDate/PODDate/InvoiceDate/CheckDate/ConfirmationDate/ConfirmedDate/QuikPay` — `datetime2(0)` (из V1 `LoadMap`).
- Связи и индексы — атрибутами в сущностях (`[ForeignKey]`, `[InverseProperty]`, `[Index]`); несколько ссылок на один тип
  (`Load` → `Warehouse` ×3, `Trailer` ×3, `DatCompany` ×3, `DatDriver` ×2, `ShipperReceiverCompany` ×2; `HistoryLocationLoad` → `HistoryObject` ×2;
  `SetupPacket` → `SetupPacketContact` ×5, `SetupPacketInsuranceAgency` ×4; `Driver.Load`) EF Core не разводит сам — заданы явно в `DispatchMap.Configure`.
- `tmpLoad` (класс в `Load.cs` V1) и `Message` не перенесены.

## LinqToDB

`DispatchMap.LinqToDbConfigure`: имя таблицы, `HasPrimaryKey`, `HasIdentity` для int-журналов, `CreatedDate.HasSkipOnInsert()` у `CacheLoadIndex`.
Навигационные свойства LinqToDB колонками не считает (проверено тестом `Test03_LinqToDbColumns`), отдельного `Ignore` не нужно.
