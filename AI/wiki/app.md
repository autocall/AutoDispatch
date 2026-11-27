# AutoDispatch App — клиент (этап 7)

WPF-клиент AutoDispatch: копия V1 `App/Project` без модуля AutoCall, переведённая на .NET 10 и сервер AutoDispatch Web по образцу
AutoCall V3 (`AutoCallV3/Repo/AI/wiki/app-v3.md`). Сделано 2026-11-19; сквозная проверка — после миграции `AdUser.SipLogin/SipPassword`
и догона данных ([[plan]] этап 7, [[data-migration]] «Догон»).

## Проекты `Sources/App/AutoDispatch.sln`

| Проект | Из V1 | Что внутри |
|---|---|---|
| `AutoCall.Common` | `_Common/AutoCall.Common` | расширения, коллекции, хелперы; логгеры `_l`/`_webl` поверх Serilog (API V1 тот же); `AppSession`, `LogProperties/LogScopes`, `ExceptionFormatter` (V3); `Settings.cs` — `WebUrl` и учётка DEBUG (в `.gitignore`, шаблон `Settings.cs.example`). `BaseDto`, `DisplayAttribute`, `AutoCallEquipmentType`, `EnumExtensions` — в общем контракте `Sources/Shared/AutoDispatch.Dtos` (те же namespace), `GetDescriptionShort` — `EnumShortExtensions` |
| `AutoDispatch.Data` | `Data/AutoDispatch.Data` | `UserSession.Current` (одна сессия вместо двух V1), HTTP-клиенты V3: `Api/Base/BaseApi`, `SettingsApi`, `MapApi` (`Link`, `SelectLocationLink`, `OrderStopsLink`), `LogsApi`, `MetricsApi`, `Logs/ClientLogBatch`, `Metrics/ClientMetrics` (только `Exceptions` — как `ClientMetricsDto` сервера) |
| `AutoDispatch.Logic.Contracts` | `Logic/AutoDispatch.Logic.Contracts` | вьюмодели, модели data-сервисов, перечисления; папка `AutoCall/` — типы `AutoCall.Logic.Contracts` V1, нужные dispatch (настройки бордов и гридов, `BoardType`, вьюмодели входа/главного окна/настроек, софтфон: `ICallService`, `CallServiceSettings`, `MicroSIPSettingsViewModel`, `InboundCallViewModel`), namespace V1 сохранены; `ExpensesConverter`/`GainsConverter` (их вызывает конструктор `LoadViewModel`); `Helpers/StateZones` |
| `AutoDispatch.Logic` | `Logic/AutoDispatch.Logic` | `DispatchService`, `ReportService`, `LoginService`, `EngineManager`, `LoginManager`, data-сервисы на `BaseApi`, статические конвертеры `Converters/**`, `BllMappingProfile` (только автоматические пары), `Settings/SettingsService`, `CallServices/` (софтфоны из `AutoCall.Logic` V1: `MainManager`, `MicroSIPCallService`, `AbtoPhoneCallService`, `NoneCallService`, помощники), `Services/ServerLogSink` и `MetricsReporter` (V3) |
| `AutoPhoneV2` | `UI/AutoPhoneV2` | софтфон Abto SIPVoipSDK + NAudio; csproj как V3 (COMReference — сборка только VS MSBuild) |
| `AutoCall.WPF` | `UI/AutoCall.WPF` | `AssemblyName = AutoDispatch`, `RootNamespace`/namespace V1 `AutoCall.WPF`; pack-URI `/AutoDispatch;component/…` |

`Directory.Build.props` как в V3 (`net10.0-windows`, x86, AutoMapper 12); отступления: `Nullable`/`ImplicitUsings` выключены (код V1 со своими using),
`LangVersion 13` (в коде V1 переменные `field`). Сборка: `MSBuild.exe AutoDispatch.sln` из VS 18 (`dotnet build` не собирает `AutoPhoneV2`).
`Libraries/` (`Interop.SIPVoipSDK.dll`, `TwainDotNet.dll`) и `Resources/phone/SIPVoipSDK` — вне git, как в V3. Файлы V1, которых не было в его csproj
(`PhoneControl`, `LoadAddDestinationDialog`, `UserControls/ContactsControl`), и мёртвый `GMapControl` лежат как `None`, не компилируются.

## Как сделано (решения [[plan]] «Решить»)

- **DI** — `Microsoft.Extensions.DependencyInjection`, все регистрации в `App.xaml.cs` при старте; конкретные классы без интерфейсов
  (`IDispatchService`, `I*Manager`, `I*DataService`, `IServicesHost`, `IUserSession` удалены); в окнах и диалогах — `App.Services.GetRequiredService<T>()`.
- **AutoMapper 12** — `BllMappingProfile`: QueryLoad, HistoryObject, Census, CensusSearch, GridViewSettings↔GridSettingsViewModel, колонки,
  CarrierHistory→RequestHistory2Dto; `ViewMappingProfile`: double↔`GridViewLength` для ширины колонок. Остальное — статические `XConverter.ToVm/ToDto`
  (`Converters/<Entity>/<Entity>Converter.cs`, `BaseConverter.Fill*`; `ObjectConverter` без вызовов удалён 2026-11-26), `ConverterHelper` — таблица «тип DTO → конвертер» вместо `Map<T>`.
  Свои статические конвертеры AutoCall-частей: `CallServiceSettingsConverter`, `BoardSettingsConverter`, `PhoneContactConverter` (разбор телефона из `DatPowerRecordVmToVm`).
- **Сервер** — `BaseApi` V3 (HttpClient на `Settings.WebUrl`, Bearer `UserSession.Current`, `X-Request-Id`), маршруты [[protocol]]:
  `Api/Dispatch/Request|Response` (Newtonsoft с `$type`, как V1), `Api/Account/SignIn|ForgotPassword` (цепочка `CheckTokenAsync` → `Api/Account/Session` есть, но не вызывается — [[audit]] E2), `Api/Dispatch/Helpers/*`, `Api/DatCompany/Search`,
  `Api/Census/Search`, `Api/ActualCompany`, `Api/SetupPacket/FactoringCompanies`, `Api/Settings`, `Api/Maps/*`, `Api/Logs`, `Api/Metrics`.
- **Вход** — один `Api/Account/SignIn` (V1 — вход AutoCall + вход dispatch). Роли: имена сервера → битовая сумма `RoleName` (на ней App проверяет права).
  SIP-учётка — `SessionDto.SipLogin/SipPassword`. После входа — `SettingsService.LoadAsync`.
- **Настройки** — `SettingsService`: секции `Api/Settings` в кэше, запись только изменённых, при закрытии окна — с ожиданием ([[user-settings]]).
  Перенесённый JSON V1 читается Newtonsoft без разрешения `$type` (проверено 2026-11-19 на секциях `autodispatch.dbo.UserSetting`). Имя входа и сканер — локально
  `%AppData%\AutoDispatch\settings.json` (как `LocalSettingsService` V3).
- **Логи** — Serilog: файл `%AppData%\AutoDispatch\logs` (7 дней, как `DeleteOldLogs` V1) и `ServerLogSink` → `Api/Logs`; `_webl` — Scope `web`.
- **Карты** — WebView2 (`%LocalAppData%\AutoDispatch\WebView2`): маршрут — `MapWindow` + `MapRouteHelper` (`/Map`, вместо `MapRouteDialog` на EO.WebBrowser),
  выбор места — `EditLocationDialog` (`Map/SelectLocation`, результат `postMessage`), остановки — `EditStopsDialog` (`Map/OrderStops`, `window.reorder`) — [[public-site]].
- **Софтфоны** — AutoPhone (MicroSIP), AutoPhone v.2 (Abto), None; окно Settings — только Call Services. Под .NET 10 из V3: `Application.ExitThread` вместо
  `Thread.Abort`, COM SIPVoipSDK регистрируется для пользователя (HKCU). `AutoPhoneV2CallService` — синглтон DI, форма получает его в `ConfigurePhone`.
  `PhoneForm` — `ContextMenuStrip` вместо `ContextMenu` V1 (в .NET 10 бросает `PlatformNotSupportedException`, App падал при старте), без
  `#if DEBUG return false` V1. **Лог и метрики как в V3 (2026-11-24):** Scope `sip` — запуск/ошибки софтфонов (`MicroSIPHelper`, `AbtoPhoneHelper`),
  звонки (`CallService.OnCallChanged/OnCallFailed`, начало звонка с записью и компанией), смена регистрации SIP, сбой инициализации Abto с
  диагностикой COM в Details; `CreateAbtoPhone` проверяет регистрацию интерфейса и перерегистрирует (код регистрации — `AutoPhoneV2/Services/SipVoipSdk`,
  т. к. `AutoDispatch.Logic` ссылается на AutoPhoneV2); `IsCallError` (SIP 401/403/404/407/484/488/5xx кроме 603) → `abtophone_state.Failed`.
  `ClientMetrics`: `CallEnded/CallFailed` по `CallServiceType`, `SetSipRegistered` → сервер `MicroSip/AbtoPhone Call(Error)Counter`,
  `AbtoPhoneUnregisteredUsers` (номера 42–46 как V3), колонки MicroSIP и AbtoPhone на `/Admin/Metrics`.
- **Telerik R1 2022** — `RichTextBoxUI` слит в `Telerik.Windows.Controls.RichTextBox.dll`, иконки ленты редактора — `…RichTextBox;component/RichTextBoxUI/Images/MSOffice/`
  (`RichTextEditor.xaml`, 185 иконок сверены с ресурсами сборки 2026-11-19).
- **Сканер** — TwainDotNet по `HintPath` + log4net пакетом; `Process.Start` с `UseShellExecute = true` (Paint, файлы, ссылки — во всём App).
- **ClickOnce** — `Properties/PublishProfiles/ClickOnceProfile.pubxml` (`https://dispatch.myautocall.net/Download/`, рантайм .NET Desktop **x86** — App x86;
  в V3 указан x64), `Scripts/publish-autodispatch-app.ps1` (копия V3). Сертификат подписи — тот же, что у V3 (решение пользователя 2026-11-19), подробнее — [[deploy]].

## Что заменено (данные модуля AutoCall, которыми пользовался dispatch)

| V1 | Теперь |
|---|---|
| Колонка Equipment: `IDatPowerService.GetTypesByCodes` (справочник `EquipmentType` WebV1) | `Board_EquipmentTypeToCodeConverter` — таблица кодов, снятая с `autocall.dbo.EquipmentType` 2026-11-19 |
| Фильтр по зонам: `IStateRepository` + `IZoneRepository` (WCF) | `StateZones.StateToZone` — 101 штат, снято с `autocall.dbo.State/Zone` 2026-11-19 |
| Борд истории, поиск города: `IDatPowerStorage.GetLocationsAsync` (города DAT с координатами) | подсказки `Api/Dispatch/Helpers/PlaceAutocomplete`, координаты — `PlaceDetails` при выборе (как `PlaceApi` V3). Название подсказки — «город, штат» без страны (`main_text` + первая часть `secondary_text`), как у городов DAT: без радиуса сервер ищет `OriginCity LIKE 'Visalia, CA%'` (исправлено 2026-11-26, до этого уходило «Visalia, Ca, USA» и поиск без радиуса ничего не находил) |
| «Are you here?» (выход после 7 дней простоя): статические таймеры `DatPowerBoard` | `UserControls/Boards/ExitAppTimer` (код V1) |
| Copy в контекстном меню `valueBindableTextBlock`: `RecordBaseDetailsDialog.Copy_OnClick` | `Helpers/CopyMenuHelper` (код V1) |
| `Xceed MessageBox` Pallets/Length, `ByteUpDown` | `TaskDialog` WinForms, `RadNumericUpDown` Telerik (Extended WPF Toolkit под MS-PL — только .NET Framework) |
| `System.Windows.Interactivity` | `Microsoft.Xaml.Behaviors.Wpf` |

## Не перенесено (модуль AutoCall, решения [[plan]])

Борды и диалоги DatPower, LoadDetails, Patterns, RecordFilter (Blacklist/Favorites), QuickRateSearch (в т.ч. «Show Rates» в меню борда), SimilarLoads,
DataSources, NewVersionDialog, тумблер автодозвона, тип вида, громкость уведомлений, вкладки Sound/Hotkeys, Bria/Desktop и устаревшие софтфоны,
«Save Columns» (дефолты компании), чат, `ReportSimilarLoads`, конструкторы окна груза из записей лоадбордов.
Из `MainWindow.xaml` убраны `Loaded="OnWindowLoaded"` (глобальные горячие клавиши звонков) и `Activated="Window_Activated"` (сброс подсветки
иконки на панели задач, включалась новыми результатами лоадборда) — методы удалены вместе с модулем AutoCall. События корневого элемента
WPF привязывает при загрузке через reflection, поэтому отсутствие метода видно только при запуске (`XamlParseException`), не при сборке;
остальные XAML проверены на это 2026-11-20.

## Мёртвый код V1 — закомментирован с пометкой

DAT rate (`GetHybridRate`: окно груза, «Refresh» — показывает «DISABLED», как V1; тестовый груз), `ScanFileHelper.FindFile/RenameTempFile` и вызов в `DispatchActions`,
`EditBaseLoadControl.Phone_OnMouseUp` (XAML не ссылается), `PhoneControl` (не входил в сборку V1), проверка `DefaultPassword` во входе (`ChangePasswordDialog` удалён 2026-11-26, [[audit]] E3),
кнопка Details AutoPhone v.2 по записи DAT, `BinaryHelper.ByteArrayToObject/ObjectToByteArray` (BinaryFormatter), `DatCompanyDataService.SendSetupPocket`.

## Проверено 2026-11-19

`AutoDispatch.sln` собирается (VS MSBuild, 0 ошибок); App запускается до окна входа; конфигурация AutoMapper строится; `SettingsService` читает перенесённые
секции (BoardsSettings2, FiltersColumns, DefaultLoadSettings, Selected*Company, Scan*, CallServiceSettings) и пишет только изменённую; `LoadConverter` туда-обратно.
Не проверено на 2026-11-19: вход и работа с сервером, сканер, софтфоны, WebView2-карты, раскладки гридов Telerik 2021 → R1 2022. Позже проверено ([[app-testing]]):
вход и работа с сервером (сессии 1–8), софтфоны (7.21), карты (7.18, 8.3), раскладки гридов (8.18 — исправлено имя сборки). Сканер не проверить — нет устройств.

## Осталось

1. Миграция `AdUser.SipLogin varchar(10)`, `SipPassword varchar(50)` (пользователь), прогон шага 0.4/0.5 переноса.
2. Первая публикация; положить `New Driver Application Package.pdf` (V1 `Web/Download`, бланк для нового водителя —
   ссылка в справочнике Drivers) в `/home/autodispatch/download` — [[deploy]]. Адрес исправлен на `{WebUrl}Download/…`: V1 собирал `//Download`,
   IIS это принимал, `RequestPath /Download` на Kestrel — нет.
3. Сверка с V1 под одним пользователем (критерий этапа) — на хосте после догона.
