# Setup packet на daisyUI + Stimulus (этап 6): CarrierRegistration

Мастер онбординга карриера `CarrierRegistration` (V1 `SetupPacketController` + 18 knockout-views) переписан на Razor-формы + Tailwind/daisyUI
с тремя маленькими Stimulus-контроллерами; URL как в V1 (ссылки в письмах и в App). Сделано 2026-11-17, тесты `11_SetupPacket` (2)
против локального сервера. Серверная часть (`SetupPacketService`, PDF, письма) — с этапа 3 ([[web-logic]]); план и решения — [[plan]];
публичный layout — [[public-site]].

## Маршруты (как в V1)

| URL | Кто | Что |
|---|---|---|
| `GET /CarrierRegistration/?CarrierId=` | карриер по ссылке из письма, без логина | текущий шаг packet'а; без packet'а — шаг 0; `SetupPacketCompleted` — `Finish` |
| `GET /CarrierRegistration/View?CarrierId&step=` | агент под логином сайта (`[Authorize]`) | просмотр шага: `<fieldset disabled>`, без кнопок, навигация по шагам слева |
| `GET /CarrierRegistration/Edit?CarrierId&step=` | агент | правка шага, кнопка Save, остаётся на шаге (V1); из App — `OpenWebSetupPacketAction` |
| `POST /CarrierRegistration/StepNN?CarrierId=` | форма шага (antiforgery, hidden `Step`, `Mode`) | модель шага → `SetupPacketDto` (AutoMapper) → `SetupPacketService.SaveAsync`; User — `Step = Next(step)` и редирект на Index |
| `POST /CarrierRegistration/PrevNN?CarrierId=` | кнопка «previous step» (`formaction`, `formnovalidate`) | сохранить без валидации и шаг назад |
| `GET /CarrierRegistration/DownloadFileMC|COI?CarrierId=` | без логина | файл из `FilesPath/{carrierId}` |
| `GET /Print/SetupPacket/Agreement?CarrierId&version=2`, `…/InsuranceInstructions?CarrierId` | без логина | `PrintController`, `_PublicLayout`, кнопка `window.print()` |

Контроллер — `CarrierRegistrationController` (на классе нет `[AllowAnonymous]` — он перекрыл бы `[Authorize]` у View/Edit; fallback-политики
нет, остальные действия анонимны). Общий ход: `LoadAsync` (карриер → компания → packet) → `NextAsync<T>` (валидация → `Mapper.Map(model, dto)`
→ `SaveAsync` → редирект) / `PrevAsync<T>` / `StepViewAsync` (модель шага из DTO, справочники `StateHelper`, даты подписей PST по умолчанию,
контакты и агентства по умолчанию как в knockout-моделях V1). Шаг 0 без packet'а — `CreateSetupPacketAsync` (карточка FMCSA и сертификат MC
через Safer) и `SaveDatCompany` с `SetupPacketId`. Шаг 14 — `SetupPacketCompleted = true` → `SaveDatCompany` → письмо с PDF; в режиме User
страница шлёт форму сама при загрузке (V1 auto-next), ошибка отправки — на той же странице. Ошибки валидации — та же страница с сообщениями
у полей (`ModelState`, правила из knockout.validation V1: обязательные поля, контакты/агентства только при включённом чекбоксе, SSN/EIN по
формату, подписи). Логи — Scope `setup-packet`.

## Views (`Views/CarrierRegistration`)

`_Layout` (поверх `_PublicLayout`: карточка карриера и меню шагов для агента, `Wide`), `_StepHeaderPart` («3/9 complete», заголовок),
`_StepButtonsPart` (hidden `Step`/`Mode`, antiforgery, ошибка, Next/Save/previous), `_ContactPart` (×5: Confidant, Corporate, Sales, Dispatch,
AfterHour), `_InsuranceAgencyPart` (×4, у Workers Compensation `HiddenPartial` = `_WorkersCompensationWaiverPart` — отказ с подписью при снятом
чекбоксе), `_Agreement02Part`, `_InsuranceInstructionsPart`; шаги `Step00`, `Step01`, `Step02`, `Step04_02`, `Step05`, `Step06_02`, `Step07`,
`Step13` (multipart, `IFormFile fileMC/fileCOI` → `SetupPacketService.SaveStepFileAsync`: `MC_yyyy_MM_dd.ext`, занятое имя — `UniqueFileFullPath`),
`Step14`, `Finish`, `Error`. Модели шагов — `Logic/Dtos/Dispatch/SetupPacketStepModels.cs` (V1 `AutoCall.Logic.Contracts/Models`),
маппинг в `DispatchMappingProfile`; страница — `Models/SetupPacketViewModels.cs` (`SetupPacketViewModel`: `Title/Enabled/Next/Prev/StepDisplay`
как в V1, модели партиалов).

## Замена инструментов V1 (решено 2026-11-17, вопросов к пользователю не потребовало)

| V1 | Теперь |
|---|---|
| knockout-viewmodels, `$.ajax` JSON → `{Success, Url}` → `location.href` | обычные формы POST + редирект; htmx не нужен (файлы, редиректы) |
| knockout.validation (83 `required`) | `ModelState` в контроллере + HTML5 `required`/`maxlength`; `previous step` — `formnovalidate` |
| `knockout-file-bindings` (base64 в JSON) | `multipart/form-data`, `IFormFile` |
| jquery.maskedinput (телефоны, SSN, EIN) | `mask_controller.ts` (`data-mask-pattern-value`) |
| «same as», «Pay To = Main», выбор факторинговой компании (`eval` в TS) | `copy_fields_controller.ts` (`copy`, `mirror`, `fill` по `name` полей; факторинговые компании — `<option data-*>` из `GetFactoringCompaniesAsync`, 188 строк) |
| knockout `visible`/`enable` по чекбоксам | `toggle_controller.ts` (`switch`, `content`, `hidden`, `lock`) |
| W-9: тип компании показывает классификацию, C Corp закрывает SSN | `w9_controller.ts` |
| подпись `Brush Script MT` (`BRUSHSCI.ttf`, Monotype) | класс `.signature` — `Dancing Script` (OFL, `wwwroot/fonts`, тот же шрифт, что в PDF) |
| select2 | только картинка спиннера в V1, сами select'ы — обычные; daisyUI `select` |
| bootstrap 3 grid | Tailwind grid, daisyUI `form-control`/`input`/`checkbox`/`menu`/`card` |

## Отступления от V1 (решены пользователем 2026-11-17, записаны в [[plan]])

- **Выключенные шаги не перенесены**: `Enabled` V1 пропускает 3 (Questionnaire), 8 (Carrier Profile), 9 (Equipment), 10 (References),
  11 (W8 — 64 чекбокса штатов), 12 (Diversity); карриер их не видит, агент мог открыть через `Edit?step=`. Модели и маппинг есть, views — нет:
  `Edit?step=8` даст ошибку view. В боевых данных `d_SetupPacketReference` — 9 строк.
- **Views версии 1 не перенесены** (`Step04`, `Step06`, `Agreement` — чекбокс AgreementAgree вместо подписей): шаги 4 и 6 всегда открываются
  views версии 2; у 3 619 старых packet'ов в режиме View договор версии 2 с пустыми подписями.
- **Карточка FMCSA недоступна** (Safer 502, карриер не найден): packet создаётся без предзаполнения, карриер вводит данные на шаге 2;
  в V1 исключение прерывало регистрацию. Сертификат MC и так был необязателен.
- Маркер маршрута и прочее — см. [[public-site]].

## Проверка

`11_SetupPacket`: `Test01_Wizard` — сброс packet'а тестового карриера `C0000000-…0001` (SQL по `ConnectionStrings:DefaultConnection`), все
включённые шаги по порядку с antiforgery и редиректами, ошибки валидации (пустой контакт, без подписи, EIN не по формату), `Prev02`, загрузка
COI и `DownloadFileCOI`, шаг 14 (`Setup packet completed` в логе; письмо локально не уходит), печать; `Test02_AgentPages` — View без логина →
`/Account/LogIn`, под cookie-логином — `<fieldset disabled>`, меню шагов, Edit с Save. Тестовый карриер — MC 27804 / DOT 3008501
(реальный, для Safer); при прогоне 2026-11-17 Safer `api/v3/company` и `geo/google/*` отдавали 502 — предзаполнение не проверено,
`02_Places` и `09.Test04` падали по той же причине. Попутно исправлен `09.Test03`: id события — `first.EventId` (конверт), у `EmptyEventDto`
поле `[JsonIgnore]`, с пустой БД совпадало случайно. В браузере страницы не смотрели — только HTML в тестах.
