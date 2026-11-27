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
чекбоксе), `_Agreement02Part`, `_InsuranceInstructionsPart`; шаги `Step00`, `Step01`, `Step02`, `Step04_02`, `Step05`, `Step06_02`, `Step07` (W-9 2017, версии до 3), `Step07_03` (W-9 2024, версия 3),
`Step13` (multipart, `IFormFile fileMC/fileCOI` → `SetupPacketService.SaveStepFileAsync`: `MC_yyyy_MM_dd.ext`, занятое имя — `UniqueFileFullPath`),
`Step14`, `Finish`, `Error`. Модели шагов — `Logic/Dtos/Dispatch/SetupPacketStepModels.cs` (V1 `AutoCall.Logic.Contracts/Models`),
маппинг в `DispatchMappingProfile`; страница — `Models/SetupPacketViewModels.cs` (`SetupPacketViewModel`: `Title/Enabled/Next/Prev/StepDisplay`
как в V1, модели партиалов).

## Замена инструментов V1 (решено 2026-11-17, вопросов к пользователю не потребовало)

| V1 | Теперь |
|---|---|
| knockout-viewmodels, `$.ajax` JSON → `{Success, Url}` → `location.href` | обычные формы POST + редирект; htmx не нужен (файлы, редиректы) |
| knockout.validation (83 `required`) | `ModelState` в контроллере, сообщение красным под полем (`This field is required.`, как V1); формы шагов `novalidate` — атрибут `required` только помечает поле, всплывающей подсказки браузера на его языке нет (решено пользователем 2026-11-23, [[app-testing]] 6.27); `maxlength` работает |
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
  11 (W8 — 64 чекбокса штатов), 12 (Diversity); карриер их не видит, агент мог открыть через `Edit?step=`. Views не перенесены, а действия `Step/Prev 03, 08–12`, модели шагов и маппинг
  удалены 2026-11-26 ([[audit]] E1); `Edit?step=8` по-прежнему даёт ошибку view. В боевых данных `d_SetupPacketReference` — 9 строк.
- **Views версии 1 не перенесены** (`Step04`, `Step06`, `Agreement` — чекбокс AgreementAgree вместо подписей): шаги 4 и 6 всегда открываются
  views версии 2; у 3 619 старых packet'ов в режиме View договор версии 2 с пустыми подписями.
  **Найден баг, не исправлен (2026-11-26, решение пользователя — оставить как есть):** `CarrierRegistrationController.Step04` для packet'а версии 0–1 требует галочку `AgreementAgree`, а форма `Step04_02` её не отправляет — сохранение шага 4 молча не проходит (ошибка «You must accept the agreement» в форме не выводится, страница открывается заново); в тексте договора у такого packet'а дата «ERROR!!!» (`Agreement_02Date` не заполняется). Затронуты: карриер с незавершённым packet'ом 2021–2022 гг. по старой ссылке (на боевой 15 packet'ов версии 0 на шаге ≤ 4, правки не позже 2022-12) и агент в `Edit?step=4` старого packet'а (~1 700). Повторная отправка setup packet'а из App поднимает версию до текущей и шаг до 4 (`DispatchService.Emails.cs`) — дальше работает проверка версии 2. Варианты исправления: проверять шаг 4 всегда по правилам версии 2 (~10 строк + тест) или для версии 0–1 запрещать сохранение с понятным сообщением ([[audit]] E1).
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

**2026-11-23** (проверка App, [[app-testing]] 6.20–6.23): пройдено по HTTP целиком для packet'а из V1 и нового карриера; исправлены
`ConfidantContactId` на шаге 0 (не передавался — ссылка обнулялась), сохранение от `CreatedById` неперенесённого пользователя
(AccessDenied на шагах 0/14), W-9 PDF на `iTextSharp.LGPLv2.Core` (null в `SetField`, незакрытый `BeginText`). Accept в App шлёт письмо с PDF
контактному лицу. `Test01_Wizard` проверяет ссылку на контакт и завершение packet'а; тестовый карриер после загрузки копии компании
восстанавливается скриптом из scratchpad (клон строки карриера).

### Форма W-9 Rev. March 2024 (2026-11-23)

**Версия packet'а 3** (`AppSettings.SetupPacketVersion = 3`, механизм версий V1): новые packet'ы — W-9 2024, packet'ы версий 0–2 — форма V1,
которую карриер заполнял, и в PDF, и на шаге 7 (просмотр и правка агентом). По версии выбираются view `Step07`/`Step07_03`
(`CarrierRegistrationController.ViewName`), шаблон `fw9.pdf` (V1: сам PDF — Rev. December 2014, страница V1 пишет «Rev. November 2017»)
/ `fw9_03.pdf` (`DispatchFileHelper.GetFileByVersion` — теперь ближайший `_NN` не новее версии: setup packet версии 3 берёт `_02.rtf`)
и код заполнения (`ReportService.GenerateW9Pdf` → `GenerateW9Pdf2017` — код V1 / `GenerateW9Pdf2024`). Как в V1 при смене версии:
отправка setup packet'а агентом карриеру со старой версией (`SendSetupPacket`) переводит packet на версию 3, шаг 4, снимает Accept —
карриер заново подписывает договор и заполняет W-9 2024. Версии на боевой БД (активные компании, 2026-11-23): 0 — 286, 1 — 1 424, 2 — 2 283.

Шаблон W-9 2024 — `Resources/Documents/fw9_03.pdf` = `FormW9.pdf` из Safer (Rev. March 2024), текст шага 7 версии 3 —
по Safer `CarrierOnboarding/Step06.cshtml` (3a/3b, Part I/II, General Instructions 2024). Новое поле **3b** — `W9ForeignPartnersHas`
(контракт `SetupPacketDto`, сущность, конвертеры сервера и App, `SetupPacketStep07Model`, чекбокс на шаге 7) — нужна EF-миграция пользователя
([[plan]] «Форма W-9 2024»). `ReportService.GenerateW9Pdf`: новые имена полей (`f1_01…`, `Boxes3a-b_ReadOrder[0].c1_1[0-6]/c1_2[0]/f1_03/f1_04`,
`Address_ReadOrder[0].f1_07/08`, SSN/EIN — `f1_11…f1_15` прямо в `Page1`), галочки — чекбоксы формы по состоянию (`SetCheckbox`, как Safer),
подпись и дата на y=195. Проверено генерацией через настоящий `ReportService` (scratch) и просмотром страницы: все поля, LLC + P, 3b, EIN/SSN,
подпись, дата; пустые необязательные поля не роняют. **Замечание по Safer**: там SSN/EIN пишутся в `SSN[0].f1_11[0]`/`EmployerID[0].f1_14[0]` —
в форме 2024 таких полей нет, SSN/EIN в W-9 Safer, вероятно, пустые (проверить в Safer).

Раскладка шага 7 (замечания пользователя 2026-11-23, [[app-testing]] 6.26–6.27): галочка 3b — в конце последней строки текста; справа от 3a —
пункт 4, каждая подпись и её выпадающий список с новой строки (в daisyUI 5 нет `form-control`, `label` строчный — нужны `block`);
«Requester’s name and address» — справа от 5–6. Поле 2 (Business name) обязательное, как в V1 (решено пользователем). SSN и EIN вместе не принимаются — «Enter only SSN or EIN — not both», как Safer, отступление от V1 ([[plan]] «W-9: SSN или EIN, не оба»).

## Справочник Factoring Companies (`/Admin/FactoringCompanies`, 2026-11-24)

Перенос страницы V1 `FactoringCompany/Index` (меню Admin): справочник `SetupPacketFactoringCompany` (188 записей), из которого карриер
выбирает Pay To на шаге 2 (`copy-fields#fill` по data-атрибутам option) и App — Factoring Company (`Api/SetupPacket/FactoringCompanies`).
Общий для всех компаний. Контроллер `AdminFactoringCompaniesController`, сервис — уже перенесённый `FactoringCompanyService`
(`SimpleDispatchServices.cs`, V1 `LookupAsync`/`GetByIdAsync`/`SaveAsync`/`DeleteAsync`): таблица Name, Address, City, State, Zip Code,
Email с поиском по подстроке во всех колонках и штате и сортировкой по колонке (номер колонки — как DataTables V1; страниц нет — строк ≈190),
добавление и правка в модалке htmx (как `/Admin/Companies`), удаление с подтверждением. Обязательные — Name, Address, City, State
(V1 `FactoringCompanyModel.ts`), длины — колонки; формата email V1 не проверял — и здесь нет. Новый Id = max + 1 (ключ без identity, как V1);
страны в форме нет — `CountryId` сохраняется как был (у всех 0). Удаление, как V1: packet'ы держат `FactoringCompanyId` без FK и свои копии Pay To.
**Доступ — только Admin (решено 2026-11-24, [[plan]])**: в V1 пункт меню был у Admin, но контроллер без авторизации — править и удалять мог
любой со ссылкой. Тест `12_FactoringCompanies` (без логина — LogIn, список = число строк, сортировка, обязательный штат, добавление max + 1,
поиск, правка, удаление; тестовая запись убирается).
