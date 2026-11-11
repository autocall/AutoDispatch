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
