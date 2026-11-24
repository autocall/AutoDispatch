# Протокол App ↔ Web

Как клиент AutoDispatch разговаривает с сервером в V1 и что из этого сохраняется в V3. Внешний контракт не меняется:
те же URL, конверты и 170 типов данных; меняется только реализация внутри контроллера. Источник — `DispatchApiController.cs`,
`_Dtos/AutoDispatch.Dtos`, `App/.../AutoDispatch.Logic/DataServices/EngineDataService.cs`.

## Конверт

Один эндпоинт `POST Api/Dispatch/Request` на все операции. Клиент шлёт `EventRequest`:

```csharp
public class EventRequest {
    public string Name { get; set; }               // имя метода IDispatchService, клиент ставит nameof(...)
    public BaseEventRequestData Data { get; set; } // конкретный *RequestData, в нём UserId
}
```

Ответ `EventResponse { Status (Success/Failed), Data (*ResponseData), ErrorMessage, EventId }`.
`RequestZip/ResponseZip` — то же с gzip тела, используется для `LoadLoadsZip`.

## Как сервер вызывает метод (V1, `DispatchApiController.cs:94-143`)

```csharp
var wrapper = JsonExtension.DeserializeObject<EventRequest>(requestString);
if (wrapper.Data.UserId != userId) throw new AccessDeniedException();   // userId из токена
// VerifyId пользователя должен быть Allow
var method = typeof(IDispatchService).GetMethod(wrapper.Name);         // reflection по имени
var attr = method.GetCustomAttributes(typeof(MinAccessAttribute))...   // роль из атрибута метода
var result = method.Invoke(_service, new object[] { wrapper.Data });
if (result is Task) { await task; result = task.GetType().GetProperty("Result").GetValue(task); }
```

Полиморфизм `Data` — Newtonsoft `TypeNameHandling.All` (`_Common/AutoCall.Common/Extensions/JsonExtension.cs:15`):
в JSON уезжает `$type` с полным именем типа и сборки, например
`AutoDispatch.Dtos.Data.LocationDriverChangedRequestData, AutoDispatch.Dtos`.

## Long-polling — `Response`

Клиент держит открытый запрос `Api/Dispatch/Response` с `lastEventId`; сервер в `EventNotificationAsync` ждёт новых строк
`d_Event` компании (статические `CurrentTasks`/`UserInProcess` по userName, новое ожидание отменяет предыдущее) и отдаёт
события с `EventId`. При старте сервер берёт `max(Id)` по компании (`DispatchService.cs:130`). Чат (`d_Message`) шёл тем же
каналом — в V3 не переносится.

## Решения для V3

- **Контракт как есть.** `AutoDispatch.Dtos` — общий проект `Sources/Shared`, **имя сборки и namespace не меняются**: иначе
  `$type` старого клиента и нового сервера не совпадут. Сервер — Newtonsoft (`AddNewtonsoftJson` уже есть в V3), не System.Text.Json.
  Исключения (только добавление свойств, по решению пользователя): `SetupPacketDto.W9ForeignPartnersHas` — 2026-11-23, форма W-9 2024 ([[plan]] «Форма W-9 2024»).
- **`SerializationBinder`** на входе: `TypeNameHandling.All` позволяет клиенту назвать любой тип; ограничить типами из сборки
  `AutoDispatch.Dtos`. Контракт не меняет.
- **Таблица `Name → handler` вместо reflection на каждый запрос** (рекомендация): при старте один раз пройти `IDispatchService`
  и собрать по методу тип данных, роль из `MinAccessAttribute` и делегат `Func<BaseEventRequestData, Task<BaseEventResponseData>>`.
  Контроллер сводится к `handlers[wrapper.Name](data)`. Плюсы: неизвестное имя → 400 сразу, роли без `GetCustomAttributes`
  на каждый вызов, исключения без `TargetInvocationException`, те же имена — метрики `Request` по методам в `/Admin/Metrics`.
  Вариант «оставить reflection как в V1» допустим, переносится 1:1.
- **`Response`** — по образцу `Api/DatSoap/AlarmNotification` V3 (отмена предыдущего ожидания пользователя).
- **Авторизация** — JWT Bearer V3 вместо OWIN `Api/Token`; `CheckToken` = `Api/Account/Session`; `UserId` в `Data` сверяется
  с claims, как в V1.

## Реализация V3 (2026-11-17)

`Controllers/ApiDispatchController.cs` — `[Route("Api/Dispatch/[action]")]` на `ApiBaseController` (JWT Bearer). Все четыре действия — POST
(V1-клиент шлёт и `Response` POST-ом с `EventRequest` в теле); тело читается из потока, а не `[FromBody]` — клиент ставит
`Content-Type: application/x-www-form-urlencoded`. Ответ всегда 200 с `EventResponse`, ошибка — `Status = Failed` + `ErrorMessage` (как V1);
`InfoException` (V1 `DispatchException`) в лог не пишется. `RequestZip/ResponseZip` — `ZipExtension` (gzip + UTF-16 JSON), `application/octet-stream`.

- `Logic/Services/Dispatch/DispatchRequestHandlers.cs` — `All: Name → DispatchRequestHandler {RequestType, MinAccess, Invoke}`, строится один раз
  из методов `IDispatchService` с одним параметром `*RequestData`; `Invoke` — скомпилированный Expression (`Task<T>` и синхронные методы приводятся
  к `Task<BaseEventResponseData>`), исключения метода приходят без `TargetInvocationException`. Неизвестное имя или тип `Data` не того класса → `Failed`.
- Проверки на каждом запросе: `Data.UserId == NameIdentifier` токена; `AdUser.IsDeleted` через `UserManager` (V1 — `VerifyId`); роли из claims
  `ClaimTypes.Role` → `RoleEnum`, `HasAccess` = каждая роль пользователя ≥ `MinAccess.Role` по порядку `RoleEnum` (совпадает с `Display(order)` V1:
  Agent < Warehouse < Dispatcher < Sales < Accounting < Owner < Admin) или входит в `ExtraRoles`; без ролей — разрешено (как цикл V1).
- `Logic/Services/Dispatch/DispatchPollRegistry.cs` — singleton (`Program.cs`): `Begin(userId, RequestAborted)` отменяет предыдущее ожидание
  того же пользователя (V1 `CurrentTasks`), `End` снимает своё; отменённый запрос получает `Failed` с `EventId` = входящий, без записи в лог. `Count` — для метрик (этап 9).
- `DispatchJson.MscorlibBinder.BindToType` — `$type` только из сборок `AutoDispatch.Dtos`, `System.Private.CoreLib`, `System.Collections`
  (рекурсивно по generic-аргументам и массивам), иначе `JsonSerializationException`.
- `[DisableRequestSizeLimit]` на `Request/RequestZip` (V1 `maxRequestLength` ≈ без ограничения, `FileSave` несёт файл в JSON); в nginx нужен `client_max_body_size`.
- Тест `AutoDispatch.Web.Test/09_DispatchApi.cs`: таблица и роли без сервера; whitelist binder; HTTP против поднятого сервера — SignIn, `GetModules`,
  чужой `UserId`, неизвестное имя, `RequestZip`, `Response` без `EventId` (текущий Id сразу), второй опрос отменяет первый, пустой ответ по `EventWaitTimeout`.

## Остальные эндпоинты App (маршруты V3, решено 2026-11-17)

JWT Bearer, ответ — данные как есть через `JsonSuccess` (camelCase), ошибка — `ExceptionHandler` 400 `{error}`. App V1 ходил на старые MVC-адреса
(`Dispatch/SearchLoads`, `ActualCompany/ActualCompaniesByUserId`, `Home/SearchCensusCompanies`, `CarrierRegistration/FactoringCompanies`, `api/DatCompany/Search`)
с конвертами `{Success, Loads|Companies|ActualCompanies|CensusCompanies|Data|Fields, Message}` — `DataServices` клиента переписываются на `BaseApi` на этапе 7.

| V3 | Метод и тело | Ответ | V1 |
|---|---|---|---|
| `Api/Dispatch/Helpers/SearchLoads` | POST `{"City": "Dal"}` или `{"Company": "Wal"}` | `QueryLoadDto[]` (top 5) | `Dispatch/SearchLoads`, `CacheService.SearchLoadsAsync` |
| `Api/Dispatch/Helpers/SearchCustomers` | POST `"текст"` | `CustomerCompanyDto[]` | `Dispatch/SearchCustomers` |
| `Api/Dispatch/Helpers/PlaceAutocomplete?search=` | GET | `{predictions, status}` legacy Google (JSON, без gzip) | `Dispatch/GetPlaceAutocomplete` (gzip) |
| `Api/Dispatch/Helpers/PlaceDetails?placeId=` | GET | `{result, status}` | `Dispatch/GetPlaceDetails` (gzip) |
| `Api/Dispatch/Helpers/Distance?lat1&lng1&lat2&lng2` | GET | `{distance, duration}` (Google Distance Matrix, `Secrets:GoogleMap:Api`) | `Dispatch/GetDistance` |
| `Api/Dispatch/Helpers/ExtractLoadFromPdf?board=` | POST multipart `file` | `LoadDto` (OpenAI по PNG страниц из `/Temp`) | `Dispatch/ExtractLoadFromPdf` |
| `Api/Dispatch/Helpers/ExtractLoadFromPdf2?loadId=&board=` | GET | `LoadDto` по сохранённому confirmation | `Dispatch/ExtractLoadFromPdf2` |
| `Api/DatCompany/Search?dot=` или `?mc=` | GET | словарь полей FMCSA, ключи как есть (`Legal Name:`), без camelCase | `api/DatCompany/Search` |
| `Api/Maps/SelectLocationLink` | POST `MapLatLngDto` или `null` | `{url}` страницы `Map/SelectLocation?r=` ([[public-site]]) | `Dispatch/SelectLocation?latlng=` в EO.WebBrowser с Bearer |
| `Api/Maps/OrderStopsLink` | POST `{trailerId}` | `{url}` страницы `Map/OrderStops?r=` ([[public-site]]) | `Dispatch/OrderStops?TrailerId=` в EO.WebBrowser с Bearer |
| `Api/ActualCompany` | GET | `ActualCompanyDto[]` компании из токена | `ActualCompany/ActualCompaniesByUserId` |
| `Api/Census/Search` | POST `CensusCompanySearchDto` | `CensusCompanyDto[]` | `Home/SearchCensusCompanies` |
| `Api/SetupPacket/FactoringCompanies` | GET | `SetupPacketFactoringCompanyDto[]` по имени | `CarrierRegistration/FactoringCompanies` |
| `Api/Account/SignIn` | POST `{email, password}` | `SessionDto` + `hasBnsfRailway`, `customStatuses` (как OAuth V1), `sipLogin`, `sipPassword` (софтфон, 2026-11-19) | `Api/Token` + `MicroSIPLogin/Password` из входа AutoCall |
| `Api/Account/Session` | GET | `SessionDto` без токена | `Api/Account/Check` (на сервере V1 не было) |
| `Api/Settings` (этап 7) | GET; `PUT Api/Settings/{section}`, тело — JSON секции | все секции одним объектом или `null` ([[user-settings]]) | WCF `svc/SettingsService` |

Клиентская сторона (этап 7) — data-сервисы App на `BaseApi` V3 ([[app]]).

Не переносятся: `api/DatCompany/SendSetupPocket` (на сервере V1 отсутствует), `Api/Account/Logout` (JWT без серверной сессии).
`/Temp/{guid}.png` — статика из `AppSettings.TempPath` без авторизации, как `Web/Temp` V1 (OpenAI забирает картинки по URL).

## Методы `IDispatchService` (V1: 77; V3: 74)

GetUsers GetModules Location*Changed(5) LoadHistory2 LoadHistoryLocation*(5) LoadHistoryChangesLoad LoadDrivers LoadTrucks
LoadTrailers LoadWarehouses LoadYards LoadLoads LoadLoadsZip LoadHistoryObjects LoadTotalHistoryLoads LoadDatCompanies
LoadDatDrivers LoadSetupPackets LoadShipperReceiverCompanies LoadCustomerCompanies Save*(Driver, Truck, Trailer, Warehouse, Yard,
Load, Loads, DatCompany, DatDriver, SetupPacket, FactoringCompany) FixLoad Delete*(Driver, Truck, Trailer, Warehouse, Yard, Load,
Loads, DatCompany, DatDriver, SetupPacket, Defect, DefectRepair) LoadDefects EventNotificationAsync FilesLoad FileLoad FileSave
InsuranceLoad BindLoads InvoiceSend InvoiceMerge ConfirmationSend ConfirmedSend SetupPacketInviteSend SetupPacketEditSend
UndoPreview UndoApply GetStatistics GetDatCompanyStatictics SendSms SendSchemeEmail BnsfRefresh
Enable/Disable/SyncCarrierViewTracker.

Не переносятся: GetTodayMessages, SendMessage (чат, решено 2026-11-11), ReportSimilarLoads (лоадборд AutoCall, рекомендация).
