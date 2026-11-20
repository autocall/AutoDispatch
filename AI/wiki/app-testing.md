# Проверка App на локальной копии

Ручной прогон AutoDispatch App против локального Web (`https://localhost:7092`) на копии компании DDM Logistics
(`DataBase/copy company to local.sql`, пользователь Anton Gmail, Owner): что проверено, результат, найденные ошибки и исправления.
Управление — UI Automation + клики мышью из сессии Claude, документы — `E:\AutoDispatch\Files` (`Scripts/pull-local-files.ps1`).
Связано: [[app]], [[web-logic]], [[data-migration]].

Обозначения: ✅ работает · ❌ ошибка (с исправлением или без) · ⚠️ не ошибка переноса / как в V1 · ⏳ не проверено.

## Окружение

| | |
|---|---|
| Данные | DDM Logistics: 3 482 груза, 1 402 карриера, 1 026 setup packet, 58 водителей, 73 трака |
| Web | Debug из VS, `appsettings.Development.json` — ключ Google Maps JS из V3 (ключ AutoDispatch не разрешён для localhost) |
| Тестовые объекты | груз **9086** (Carrier, DDM LOGISTICS INC, Chicago, IL → Dallas, TX, Pro# CLAUDE-TEST-1, брокер C H ROBINSON, 40 kLbs, $1 500), тестовые грузы DDM «--TEST--» (9083 и др.) |

## Сессия 1 — 2026-11-20 (до перезапуска)

| # | Что | Результат |
|---|---|---|
| 1.1 | Вход, загрузка бордов | ✅ после `GeocodeAddress` LoadLoads 48 с (первый вход, адреса из Safer); до него — таймаут 100 с ([[plan]] «Кэш геокодинга») |
| 1.2 | Карта `/Map` из App | ✅ после замены ключа Google на ключ V3 (`RefererNotAllowedMapError` у ключа AutoDispatch) |
| 1.3 | Борды Carrier Dispatch, Broker Dispatch, Carrier Accounting | ✅ открываются, Carrier Accounting — 154 груза |
| 1.4 | New Load: выбор компании | ✅ 4 carrier-компании DDM с логотипами; без компании форма неактивна (как V1) |
| 1.5 | New Load: подсказки Origin/Destination (поиск по прошлым грузам) | ❌ не появлялись — нет пары AutoMapper `LocationDto ↔ LocationViewModel`, ошибка глоталась `catch {}`. **Исправлено** в `BllMappingProfile` |
| 1.6 | New Load: геокодинг города при уходе из поля («Chicago, IL») | ✅ галочка, координаты через Safer (PlaceAutocomplete + PlaceDetails) |
| 1.7 | New Load: Distance | ⚠️ после ввода Destination не считается — `CalcDistance` вызывается только из Origin, как V1 |
| 1.8 | New Load: проверки Broker, Weight | ✅ «Please enter a Broker», «Please enter a valid Weight» |
| 1.9 | New Load: подсказки брокера (census + справочник) | ✅ |
| 1.10 | New Load: сохранение | ✅ груз 9086: `Load`, `Event` тип 1, `HistoryObject`; появился в гриде без обновления (long-polling) |
| 1.11 | Attach Confirmation (загрузка PDF) | ✅ файл в `{FilesPath}/{EntityId}/confirmation.pdf`, `Load.ConfirmationFile` |
| 1.12 | Assign Driver без confirmation | ✅ «You can't continue without confirmation» (как V1) |
| 1.13 | Assign Driver | ✅ список водителей, груз → assigned, окно Trailer (водитель, трак 132 DDM, трейлер DDMK53960) |
| 1.14 | At The Pick Up | ❌ «There are no fields to update in the type 'BaseDispatchEntity'» — `_modifyLocationAsync` передавал базовый тип в LinqToDB. **Исправлено** (generic). Груз 9086 остался со статусом at the pick up без положения трейлера |
| 1.15 | Create Invoice (Carrier Accounting, груз 9083) | ❌ App падал `StringBuilder.Remove(-1)` в `ReportService.GenerateInvoice` — `Api/ActualCompany/List` отдавал компании без шаблонов, LogoBig и Customizes (вариант веб-грида). **Исправлено**: полный `SearchFor2Async`, как V1 `ActualCompaniesByUserId` |
| 1.16 | Пары AutoMapper App (все TypeMap) | ❌→✅ сломаны были `QueryLoad*` и `CarrierHistoryViewModel → RequestHistory2Dto` (поиск Carrier History); после исправления все проходят |
| 1.17 | Ошибка `confirmation.rtf … UNITED GLOBAL` при каждом входе | ⚠️ документы не были скопированы локально |

## Сессия 2 — 2026-11-20 (после перезапуска Web и App, документы скопированы)

| # | Что | Результат |
|---|---|---|
| 2.1 | Вход после перезапуска | ✅ ошибки `confirmation.rtf` нет — документы в `E:\AutoDispatch\Files` (4 535 каталогов) |
| 2.2 | New Load: подсказки Origin («Chic») | ✅ исправление 1.5 работает: компания / адрес / город прошлых грузов; выбор заполняет Shipper, Address, Origin с координатами |
| 2.3 | New Load: подсказки Destination («Redl») | ✅ заполняет Receiver, Address, Destination; по «Dal» подсказок нет — у DDM нет получателей в Dal* (проверено SQL) |
| 2.4 | New Load: Distance после выбора обеих точек | ⚠️ сервер: `Request distance: REQUEST_DENIED` — локальный ключ `GoogleMap:Api` не разрешён для Distance Matrix (настройка ключа, не код); проверить ключ на сервере до переключения |
| 2.5 | New Load: Close без сохранения | ✅ окно закрывается, груз не создаётся |
| 2.6 | Фоновый джоб ожидания (LoadWaitingJob) | ✅ груз 9086 после времени погрузки → `waiting` (Waiting1), как V1 |
| 2.7 | Окно Trailer → At The Pick Up → Confirm | ✅ исправление 1.14 работает: статус at the pick up, положение трейлера обновлено, ошибок сервера нет |
| 2.8 | Picked Up → окно Pickup (схема паллет) → Load All → OK | ✅ статус in transit, `Load.Layout` (varbinary, 30 паллет) сохранён |
| 2.9 | Схема трейлера в окне Trailer после погрузки | ⚠️ обновляется только после переоткрытия окна — сверить с V1 |
| 2.10 | At Delivery → Confirm | ✅ статус at delivery |
| 2.11 | Delivered → «PLEASE CHECK IF THERE WAS LUMPER OR DETENTION…» → OK | ✅ статус delivered, окно Trailer закрылось, груз ушёл с Carrier Dispatch на Carrier Accounting сам (Board 2) |
| 2.12 | Carrier Accounting: груз 9086 в списке | ✅ delivered, $1 500, Customer C H ROBINSON COMPANY, всего 155 |
| 2.13 | Create Invoice (груз 9086): генерация документа | ✅ исправление 1.15 работает: редактор Create Invoice (Telerik) — реквизиты DDM LOGISTICS INC, логотип, Bill To C H ROBINSON COMPANY, Net 30 |
| 2.14 | Create Invoice → OK (номер, PDF, загрузка) | ❌ сервер присвоил `InvoiceNumber` 9086, затем App: `NullReferenceException` в `ReportService.ToFileName(l.Company)` — ответ `SaveLoad` без имён компаний (`DatBrokerCompany`/`CustomerCompany` = null; V1 — навигации EF, V3 LinqToDB — нужен `IncludeCompaniesAsync`). PDF не создан. **Исправлено**: имена компаний дописываются в ответы `SaveLoad` (и через него FixLoad, трекеры CarrierView), `SaveLoads`, `LocationLoadChanged`. Тот же пробел гасил колонку Customer у груза после любого сохранения |

## Сессия 3 — 2026-11-20 (после перезапуска, ключ `GoogleMap:Api` поправлен)

| # | Что | Результат |
|---|---|---|
| 3.1 | Carrier Accounting: груз 9086 после сбоя 2.14 | ✅ в колонке Invoice «9086 edit» — номер присвоен, файла нет; на выделенной строке панель Create Invoice / Issues / Bond / Paid / Unpaid / Attach Check |
| 3.2 | Create Invoice → редактор | ✅ документ сгенерирован. Отладчик VS останавливался на перехваченном `NullReferenceException` в `ReportService` (стр. 724, `XamlFormatProvider.Import(load.Comments)` при пустых комментариях, `try/catch {}` как V1) — это настройка «останавливаться при выбросе», не ошибка; Continue |
| 3.3 | Create Invoice → OK | ✅ исправление 2.14 работает: `invoice_c_h_robinson_company.pdf` (374 КБ, PDF 1.7, 1 стр.) в `E:\AutoDispatch\Files\{EntityId}`, `Load.InvoiceFile`, `InvoiceDate`; ошибок сервера нет |
| 3.4 | Содержимое PDF | ✅ реквизиты и логотип DDM, Bill To C H ROBINSON COMPANY, Invoice Number 9086, Net 30, Due 12/20/2026, строка Chicago, IL → Dallas, TX $1 500 (в колонке Load# — Pro#), Make checks payable to DDM, Balance Due $1 500 |
| 3.5 | Строка груза после сохранения | ✅ Invoice «9086 edit send», Customer не очистился, на панели Send Invoice |

## Сессия 4 — 2026-11-20 (ключ `GoogleMap:Api` поправлен)

| # | Что | Результат |
|---|---|---|
| 4.1 | New Load: Distance (Chicago, IL 60652 → Redlands, CA) | ✅ 1 982 mi — Google Distance Matrix с новым ключом работает (2.4 закрыт) |
| 4.2 | Send Invoice: окно | ✅ адреса из контактов груза/брокера (у 9086 пусто), Save contacts, вложения Invoice / POD / Lumper / Confirmation / Merged, текст письма. Для теста — только `nata.kachina88@gmail.com`, Save contacts снят (адрес не пишется в контакты брокера) |
| 4.3 | Send Invoice → Send (DDM LOGISTICS INC) | ⚠️ письмо не ушло: у компании `SMTP_Email` ddmtest2019@gmail.com, `SMTP_Password` пуст и в V1 (MCP: `autocall.d_ActualCompany` тоже null) — отправка невозможна и в V1. App показал окно ошибки, статус и `EmailInvoiceSent` не изменились, сервер записал `InvoiceSend` в лог — обработка ошибки как V1 |
| 4.4 | То же с паролем GM EXPEDITED INC (тот же ящик ddmtest2019@gmail.com, временно в локальной БД, возвращён в NULL) | ⚠️ Gmail: `535 5.7.8 Username and Password not accepted` (проверено ручным SMTP-диалогом: EHLO/STARTTLS/TLS 1.3 проходят, AUTH LOGIN отклонён). В БД пароль 8 символов — пароль аккаунта, а Gmail для SMTP требует пароль приложения (16). Данные, не код; `SmtpClient` показывает это как «connection was closed» — сообщение в App малоинформативно (как V1) |
| 4.6 | Broker Dispatch: контекстное меню груза (13169, assigned) | ✅ полный набор V1: View Carrier, Create/Edit/Resend/Delete Confirmation, Create Bill of Lading, трекер CarrierView, Edit Load/Carrier/Line Haul/Expenses/Accessorial/Comments, Change Location, History, Show Trip, Post Load, Move to Accounting, Unassign Carrier, Undo |
| 4.7 | Create Bill of Lading → редактор → OK | ✅ шаблон BOL UNITED GLOBAL (логотип, номер 13169, shipper GB PACKAGING, consignee UNITED GLOBAL, bill to Accounting), PDF `bill_of_lading_pyh_express_inc.pdf` 423 КБ в документах груза, `Load.BOLFile` — исправление 1.15 (шаблоны в `Api/ActualCompany/List`) работает и для брокера |
| 4.8 | Edit Confirmation → «Show pickup address?» Yes → редактор → Close | ✅ собственный шаблон UNITED GLOBAL (`confirmation - UNITED GLOBAL.rtf` из скопированных документов), карриер PYH EXPRESS INC (MC, email, телефон, адрес), pickup/delivery; Close — `ConfirmationFile` не изменился |
| 4.9 | Справочник Drivers | ✅ список водителей с траками и колонками документов, Edit Driver (трак, слоты документов со сроками) — открывается, закрыт без изменений |
| 4.10 | Ссылка New Driver Application Package.pdf | ⚠️→✅ локально 404 — бланка не было в `DownloadPath`; скопирован из V1 `Sources/Web/Web/Download/` в `D:\Develop\AutoDispatch\Data\download\` — 200, 353 КБ. На сервере — тот же файл в `/home/autodispatch/download` ([[deploy]]) |
| 4.11 | Справочники Equipment, Warehouses, Yards | ✅ траки/трейлеры (тип, история, файлы), 5 складов, 2 стоянки; заголовок окна Yards «WAREHOUSES» — как в V1 |
| 4.12 | Loads History: Search без фильтров | ✅ исправление 1.16 (`CarrierHistoryViewModel → RequestHistory2Dto`) работает: 208 грузов, 3 страницы, ошибок нет |
| 4.13 | Settings | ✅ только вкладка Call Services (как решено), AutoPhone v.2, SIP User ID 0002 из `MicroSIPLogin` V1, пароль скрыт; Cancel |
| 4.14 | Map (кнопка в ленте) | ✅ WebView2, `/Map` с ключом V3: Google Maps, поиск адреса, слои, Route 0 mi |
| 4.15 | Show Trip (груз 13169) | ✅ ссылка с 2 точками, маршрут Safer 1 запрос / 1 с, 421 mi. Точка B — Greenfield, IN вместо Hodgkins, IL: координаты назначения неверны в самих данных (V1 `d_Load.DestinationLocation` те же 39.799, −85.729) |
| 4.16 | View History (13169) | ✅ 6 снимков: 5 перенесённых из V1 (Tony) и 1 записанный сервером V3 (Anton Gmail, после BOL) — оба формата `HistoryObject` читаются |
| 4.17 | Edit Load (13169) | ✅ все поля, Distance 420 mi, CarrierView Tracking, ставки, документы (BOL + confirmation); Customer пуст — у груза нет `CustomerId` (проверено SQL) |
| 4.18 | Broker Accounting | ✅ 3 049 грузов, ошибок нет |
| 4.5 | Отправка писем вообще | ⏳ не проверена до конца — нужен рабочий SMTP (пароль приложения Gmail у тестовой компании) или тестовая компания с другим SMTP. Учётку `carriers@unitedgts.com` (GTS / UNITED GLOBAL, реальный ящик клиента) для теста не использовал |

## Сессия 5 — 2026-11-20 (страница Dispatch Companies, SMTP nata.kachina88@gmail.com)

| # | Что | Результат |
|---|---|---|
| 5.1 | `/Admin/DispatchCompanies` → DDM LOGISTICS INC → Test SMTP (адрес и пароль приложения Gmail с формы) | ✅ лог сервера: `Email 'Test' sent from nata.kachina88@gmail.com` — пароль приложения рабочий |
| 5.2 | Save changes той же формы | ❌ в БД не сохранилось (`SMTP_Email` прежний, пароля нет, Save в логе нет): поле получателя теста было `type=email` со значением «test test» — браузер не отправил форму. **Исправлено**: поля теста — `type=text`, адрес проверяет сервер; ошибки формы ещё и списком сверху ([[dispatch-companies]]) |
| 5.3 | Send Invoice (9086 → nata.kachina88@gmail.com, Save contacts снят) | ⚠️ `InvoiceSend`: Gmail закрыл соединение — у компании всё ещё ddmtest2019@gmail.com без пароля (5.2); повторить после сохранения SMTP |
| 5.4 | Поле пароля на странице | по просьбе пользователя: задан — точки (маска `********`, сам пароль в страницу не попадает), маска или пусто при сохранении — прежний пароль; адреса и хосты SMTP обрезаются от пробелов (в `SMTP_Email` DDM сохранился пробел в конце) |
| 5.5 | Send Invoice после сохранения SMTP (9086 → nata.kachina88@gmail.com) | ✅ `Email 'Invoice #9086 Load #CLAUDE-TEST-1' sent from nata.kachina88@gmail.com`, статус delivered → billed, дата отправки в колонке Invoice |
| 5.6 | Undo (Carrier Accounting, 9086) | ✅ billed → delivered, `EmailInvoiceSent` снят, `HistoryId` на прежний снимок |
| 5.7 | Формат `Load.EmailInvoiceSent` | ❌→✅ V3 писал `…List`1[[System.String, mscorlib]], System.Private.CoreLib`, V1 — `…, mscorlib`: `MscorlibBinder` сравнивал полное имя сборки. **Исправлено** (простое имя), тест `Test03` ужесточён (падал до правки); `DispatchLogic` 13/13, `DispatchApi`+`PublicSite` 7/7. Нужен перезапуск Web |
| 5.8 | Parse Confirmation (PDF confirmation брокера) | ⚠️ OpenAI: «Incorrect API key» — в `appsettings.Development.json` ключ был с обёрткой C#-литерала `@"…"`; обёртка убрана (файл gitignored, ключ не менялся). App показал ошибку в окне — как V1. Повторить после перезапуска Web |
| 5.9 | Generate Load → OK → Attach Confirmation «Later» | ✅ груз 9087 (--TEST--, справочники), номер из Next Load#, ошибок нет |
| 5.10 | Split (9087, Edit Load → Split → OK → OK) | ❌ App: `ArgumentNullException` в `ExpensesConverter.ToDto` — у частей Expenses = null; V1 AutoMapper `TypeConverter` для null не вызывался. **Исправлено** null-проверкой в `ToDto` Expenses и Gains (остальные статические конвертеры null проверяют). Повторить после перезапуска App |

## Итог на 2026-11-20

Исправлено по результатам (сборка проверена, работа подтверждена прогоном): подсказки Origin/Destination и поиск Loads History
(AutoMapper `LocationDto ↔ LocationViewModel`), смена положения при статусах (`_modifyLocationAsync` generic), шаблоны/логотип/Customizes
в `Api/ActualCompany/List` (инвойс, BOL, confirmation), имена компаний в ответах `SaveLoad`/`SaveLoads`/`LocationLoadChanged` (сохранение инвойса, колонка Customer).

Открыто:
- Отправка писем (Send Invoice, confirmation, setup packet) — нет рабочего SMTP у тестовой компании (4.3–4.5).
- Бланк New Driver Application Package.pdf — локально положен (4.10), на сервере — при публикации.
- ~~Настройка компаний (SMTP, шаблоны, логотипы, нумерация) — интерфейса в V3 нет~~ — **перенесено 2026-11-20**: V1 `ActualCompany/Index` →
  `/Admin/DispatchCompanies` для Admin и Owner, проверено локально — [[dispatch-companies]]. SMTP DDM LOGISTICS INC (пароль приложения
  Gmail) теперь задаётся там, после этого — повторить Send Invoice (4.3).
- Сканер — не проверить: к машине не подключены сканеры.
- Схема трейлера в окне Trailer обновляется только после переоткрытия — сверить с V1 (2.9).
- Не проверены: Undo, Split / мультистоп, Parse Confirmation (OpenAI), Generate Load, трекер CarrierView, Post Load, setup packet из App,
  скан документов, софтфоны, Factoring Company, Issues/Bond/Paid/Unpaid, Move to History.
- Отладка в VS: включена остановка на всех исключениях при выбросе — App встаёт на перехваченных `NullReferenceException` кода V1 (3.2).
