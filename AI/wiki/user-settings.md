# Настройки пользователя App (`UserSetting` по секциям)

Настройки AutoDispatch App хранятся на сервере по секциям, как в AutoCall V3 (`AutoCallV3/Repo/AI/wiki/user-settings.md`): строка
`UserSetting(UserId, Section, Json)` на каждое свойство `UserSettings` клиента, плюс секция `Map` для карты ([[public-site]]).
Решено пользователем 2026-11-17: текущие настройки пользователей переносятся, `Settings.Binary` V1 как есть не переносится —
только свойства, относящиеся к dispatch. См. [[plan]] («Решить»), [[data-migration]], [[protocol]].

## Как было в V1

App V1 читал и писал настройки через WCF `svc/SettingsService` WebV1 (`AutoCall.Data/Repositories/SettingsRepository` →
`GetSettingsBinary/SetSettingsBinary(userId)`), не через `Api/Dispatch/Request` — поэтому их не было в списке эндпоинтов [[v1-inventory]].
Таблица `autocall.dbo.Settings(UserId, Data, Binary)`; `Binary` = gzip (`ZipExtension`) JSON Json.NET с `$type`: словарь
«имя свойства `UserSettings` → значение» (`SettingsToDto`/`DtoToSettings` в `AutoCall.Logic/Converters/Settings`).
`UserSettings` общий для модулей AutoCall и AutoDispatch (`AutoCall.Logic.Contracts/Settings`). Пишется целиком при каждом `Set*`.

## Секции dispatch (переносятся)

| Секция | Содержимое | Где используется в V1 |
|---|---|---|
| `BoardsSettings2` | `Dictionary<BoardType, BoardSettings2>` — только ключи `DispatchCarrier`, `DispatchBroker`, `DispatchCarrierAccount`, `DispatchBrokerAccount`, `DispatchCarrierHistory` (колонки, зум, сплиттер, включён ли борд); ключи `DatPower*` отбрасываются | `Board`, `MainWindow` |
| `DockingLayout` | `byte[]` — XML раскладки `RadDocking` (в нём и панели DatPower — App при загрузке пропускает неизвестные) | `MainWindow` |
| `FiltersColumns` | `Dictionary<byte, FiltersColumnsSettings>` — именованные фильтры бордов | `DispatchBoardBase`, `FiltersDialog` |
| `DefaultLoadSettings` | значения нового груза (оборудование, длина, температура…) — `GetDefaultPatternSettings(Dispatch*)` | `DispatchActions` |
| `DatCompaniesDialogGridView` | `byte[]` — раскладка грида Telerik (`PersistenceManager`) | `DatCarriersDialog`, `DatBrokersDialog`, `SearchDatCompanyDialog`, `HistoryLoad*Dialog` |
| `EquipmentsDialogGridView{Drivers,Trucks,Trailers,Warehouses,Yards}` | `byte[]` — раскладки гридов диалога оборудования | диалог Equipments |
| `SelectedCarrierCompany`, `SelectedBrokerCompany` | `byte[]` 16 байт — Guid выбранной `DispatchCompany` | борды dispatch |
| `ScanEmail`, `ScanBW`, `ScanADF` | настройки сканирования | `ScanSchemeSendDialog`, `ScannerDialog` |
| `SearchTrailerWarehouseMode` | bool | `SearchTrailerWarehouseDialog` |
| `CallServiceSettings` | софтфон: тип (None / AutoPhone = MicroSIP / AutoPhone v.2 = AbtoPhone), Single/Multi call, линии, `MicroSIPSettings` (контакты и шорткаты) — добавлено 2026-11-19, у 13 из 17 пользователей; учётка SIP — не здесь, а `SessionDto.SipLogin/SipPassword` | `SettingsDialog` (вкладка Call Services), `MainManager.InitializeCallService` — [[plan]] «Софтфоны в App» |

Не переносятся (модуль AutoCall): `DataSourceSettings`, `SoundSettings`, `HotKeySettings`, `ConnectionSettings`,
`ZoomSettings`, `BoardsSettings` (старый формат), `DefaultPost/Search*PatternSettings`, `PatternTabs`, `PatternsDialogGridView`,
`ViewType`, `Volume`, `VolumeMute` (звук и хоткеи — уведомления DAT и автодозвона, `ViewType` — только `DatPower*Board`).
Локальные `Properties.Settings` (`UserName` логина, `Scanner`) остаются на машине и не переносятся.

## Формат перенесённых строк

Шаг 0.5 блока 0 `DataBase/migrate from autocall dispatch.sql`: `decompress(Binary)` → `openjson` → строка на свойство из списка.
- `byte[]` — JSON-строка base64 (обёртка Json.NET `{"$type":"System.Byte[], mscorlib","$value":…}` снята);
- объекты — JSON Json.NET как есть, с `$type` (`AutoCall.Logic.Contracts…`, `AutoDispatch.Logic.Contracts…`) и `$values` у списков:
  App на этапе 7 читает их без разрешения `$type` (имена сборок клиента меняются), как V3 после переноса;
- `BoardsSettings2` пересобирается из ключей `Dispatch*` без внешнего `$type`; `null`-свойства не пишутся (нет секции → дефолт App).
Проверено select'ом на боевой `autocall` 2026-11-17: 17 пользователей, 201 строка, весь JSON валиден; самая большая секция —
`BoardsSettings2` 160 КБ.

## Дефолты компании — не переносятся (решено 2026-11-17)

Новый пользователь без настроек получает дефолты кода App; `Get/SetDefaultColumnSettings`, `Get/SetDefaultFilterSettings` и пункты
админа в `MainWindow` в App не переносятся. Как было в V1:

`autocall.dbo.Company.DefaultColumnSettings/DefaultFilterSettings` — JSON дефолтных `BoardsSettings2`/`FiltersColumns` для пользователя
без настроек; пишет админ из `MainWindow` (`SetDefaultColumnSettings/SetDefaultFilterSettings` через тот же WCF). Из 7 компаний
заполнено только у BEST AMERICAN (75 КБ колонок, в том числе `DatPower*`). В `Company` V3 таких полей нет — см. [[plan]] «Решить».

## Этап 7 (сделано 2026-11-19)

- Сервер: `ApiSettingsController` (`GET Api/Settings` — все секции одним объектом, `PUT Api/Settings/{section}`) и
  `UserSettingService.GetAllJsonAsync/SetJsonAsync` — копия V3, рядом `GetMapAsync/SetMapAsync` для карты [[web]].
- App: `AutoCall.Logic.Settings.SettingsService` — секции в кэше (`JToken`), запись только изменённых, `SaveSettingsAsync` при закрытии окна;
  публичные `Get*/Set*` V1 не менялись. JSON V1 с `$type`/`$values` читается Newtonsoft без разрешения типов — проверено на перенесённых секциях.
  `UserName` входа и параметры сканера — локальный `%AppData%\AutoDispatch\settings.json` — [[app]].
- Раскладки гридов (`*GridView`, `PersistenceManager`), сохранённые V1 (Telerik 2016/2021, .NET Framework), загружаются в Telerik R1 2022 под .NET 10.
  Помешало только имя сборки: прокси-типы записаны как `…ColumnProxy, AutoCall.WPF, Version=…`, а App V3 — `AutoDispatch`; `SetSettings`
  заменяет имя перед загрузкой (2026-11-24, все 355 раскладок локальной копии V1 применяются) — [[app-testing]] 8.18.
