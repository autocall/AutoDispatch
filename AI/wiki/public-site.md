# Публичный сайт (этап 5): Confirmation и карты для App

Страницы без логина: `/Confirmation` (карриер подписывает rate confirmation по ссылке из письма) и две карты для App —
`Map/SelectLocation` (выбор места) и `Map/OrderStops` (остановки трейлера) по схеме `/Map` V3 (ссылка с шифрованным токеном `r`).
Сделано 2026-11-17, тесты `10_PublicSite` (3) против локального сервера. План — [[plan]], сервер — [[web]], [[web-logic]], эндпоинты App — [[protocol]].

## Confirmation (V1 `ConfirmationController`, URL как в V1)

- `GET /Confirmation/?LoadId=` — `Index` (не подписан) или `Confirmed` (подписан, кнопка Download); груз не найден, удалён, без карриера
  или компании — `Error` «Something went wrong...» с кодом 200, подробности в логе (Scope `confirmation`); без `LoadId` — 400 (в V1 JSON «робот»).
- `POST /Confirmation` — форма htmx (`_SignFormPart`, antiforgery): имя 4–24 символа → `DispatchService.ConfirmedSend` от пользователя System
  (V1 `AutoCallAdmin`): подписанный PDF, письмо, событие; успех — `HX-Redirect` на ту же страницу; ошибка (`InfoException`, уже подписан) — фрагмент с текстом.
- `GET /Confirmation/Download/{loadId}` — `FilesPath/{loadId}/{ConfirmedFile}`, иначе `Error`.
- Разбивка ставки как в V1 `Index.cshtml`: доплаты (`Gains` → `GainViewModel`, `TypeDescription`), CarrierView tracking и line haul — только если
  `DispatchHelper.GetRate` ≠ line haul; телефон карриера — `DatCompanyService.GetFieldsAsync` (Safer), **необязателен**: карриер не найден или Safer
  недоступен — страница без телефона (V1 падала бы исключением), warning в лог.
- Layout `Views/Shared/_PublicLayout.cshtml`: без навигации сайта, в шапке и подвале логотип и `LegalName` компании (`ViewData["Company"]`),
  favicon — `LogoSmall`; тема — как в `_Layout` (localStorage/системная) с переключателем. На этапе 6 его же берёт `CarrierRegistration`.
- `ConfirmationController` — `[AllowAnonymous]`, `[Route("Confirmation")]`; модель `ConfirmationViewModel` (`Models/ConfirmationViewModels.cs`)
  с готовыми строками (валюта, оборудование, вес, окна pickup/delivery) — view без логики.

## Карты для App (замена `Dispatch/SelectLocation|OrderStops|RouteLocation` V1)

`RouteLocation`/`RouteLocation2` заменяет готовый `/Map` ([[map]] V3; ссылка `Api/Maps/Link`). Две новые страницы — по той же схеме:
`[AllowAnonymous]`, пропуск — токен `r` в каждом запросе, `embedded=true` для WebView2 (`_EmbeddedLayout`, тема из настроек карты),
без параметра — `_Layout` сайта; настройки карты пользователя (тип, traffic) — `UserSetting` секция `Map`. Общий код страниц —
`Client/map_common.ts` (карта по настройкам, маркеры-кружки, `readJson`, `postToApp`); `map_controller.ts` `/Map` не трогался.

| Страница | Ссылка для App | Токен | Запросы страницы |
|---|---|---|---|
| `Map/SelectLocation?r` | `POST Api/Maps/SelectLocationLink` тело `MapLatLngDto` или `null` → `{url}` | формат `/Map`: userId + точка kind `Location = 4` (или без точек) | `GET Map/Location?r&lat&lng` → `MapLocationDto` (404 — не найден); `Map/Places`, `Map/Places/{id}` как у `/Map` |
| `Map/OrderStops?r` | `POST Api/Maps/OrderStopsLink` тело `{trailerId}` → `{url}` | payload userId + trailerId (32 байта): `ParseLink` его не примет (16 % 9 ≠ 0), `ParseOrderStopsLink` не примет токен `/Map` — проверено тестом | `POST Map/OrderStops/Route {r, points}` → `MapRouteDto` (отрезки Record между соседними, лимит `MapMaxStops`) |

**SelectLocation** (`select_location_controller.ts`): начальная точка из ссылки — маркер и зум 18 (V1 `?latlng=`), клик по карте или подсказка поиска
(Safer, как `/Map`) → `Map/Location`: `MapService.GetLocationAsync` берёт из Safer `geo/google/latlng` готовые `address` (улица) и `city`
(«City, ST ZIP» — как собирал `GeocodeHelper.getCityAndAddress` V1 из address_components). Каждый выбор уходит в App
`window.chrome.webview.postMessage({ type: "location", latitude, longitude, formattedAddress, address, city })` — вместо разбора HTML
по id `address/city/latlng` через `GetHtml()` EO.WebBrowser в V1 `EditLocationDialog`; OK в App берёт последнее сообщение (этап 7).

**OrderStops** (`order_stops_controller.ts`): остановки считает сервер `MapService.GetOrderStopsAsync` как V1 `DispatchController.OrderStops` —
грузы Carrier-борда компании пользователя с этим трейлером, AtPickUp → pickup (PickupOrder) и delivery, InTransit → delivery, сортировка по Order;
`MapStopDto`: loadId, index (`IndexString` — V1 подписывал маркер первыми 4 символами Guid), type, order, координаты, адрес, окно (`Arrive`),
цвета груза `Background/Foreground` (ARGB → `#rrggbb`, V1 `Base64ToColor`). Маркер — кружок цветов груза с номером, панель слева со списком и
длиной маршрута (мили, `geometry.spherical`). Порядок меняет App drag&drop-ом в своём списке и вызывает `window.reorder([{loadId, type}…])`
(V1 — `reorder(stops)` через ExecuteScript): страница переставляет остановки и пересчитывает маршрут.

## Попутно найдено и исправлено

- `Expenses_DtoToVm.Deserialize` (Gains/Expenses из JSON клиента): подмена `$type` на серверные типы (как в V1) после whitelist `MscorlibBinder`
  этапа 4 отбивалась — все доплаты терялись (`Deserialize Gains` в логе). Теперь `$type` не разрешается, элементы читаются из `$values` по
  именам полей. Тест `08_DispatchLogic.Test10_GainsFromV1Json` на формате V1 (`GainViewModel[], AutoDispatch.Logic.Contracts`).
- `DispatchBoardType.Carrier = 0`, `CarrierAccount = 2` (комментарий в `LoadHistory2` читается в обратном порядке).

## Тестовые данные локально

`autodispatch_local` пуст (миграции без данных). Для проверки страниц вставлены компания `D0000000-…0001` («Test Dispatch LLC»), карриер `C0000000-…0001`,
трейлер `AA000000-…0001` и грузы `BB000000-…0001/0002` (AtPickUp, Board Carrier, порядок остановок, у первого Gains в формате V1 и `ConfirmationFile`);
скрипт — в scratchpad сессии, повторяем (DELETE + INSERT, строки `HistoryObject` для FK). Подпись (POST) локально не проверялась: нужен PDF
`ConfirmationFile` на диске и SMTP компании.

## Не перенесено

`Dispatch/Dispatcher` (веб-борд на карте, ~4 800 строк knockout) — решено не переносить; `RouteLocation2`, `InitializeRouteLocation` (прогрев
EO.WebBrowser) — не нужны с `/Map` и WebView2.
