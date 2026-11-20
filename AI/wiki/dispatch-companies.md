# Dispatch Companies — настройка компаний dispatch на сайте (Admin, Owner)

`/Admin/DispatchCompanies` — перенос админской страницы V1 `ActualCompany/Index` («Dispatch Companies»): SMTP, шаблоны
confirmation/BOL/invoice, логотипы, нумерация, BNSF, CarrierView, кастомизации нумерации. Перенесена 2026-11-20 по решению
пользователя (в плане этапа 5 её не было — найдено при проверке App, [[app-testing]]). Решение — [[plan]] «Страница Dispatch Companies».

## Таблицы: переименование при переносе

V1 `autocall.dbo.d_ActualCompany` → V3 `autodispatch.dbo.DispatchCompany`, `d_ActualCompanyCustom` → `DispatchCompanyCustom`,
колонка `ActualCompanyId` → `DispatchCompanyId` (`Load`, `DatCompany`, `DispatchCompanyCustom`) — [[data-model]], [[data-migration]].
Контракт `AutoDispatch.Dtos` не меняется: `ActualCompanyDto`, `ActualCompanyCustomDto.ActualCompanyId`, `Api/ActualCompany/List` для App
([[protocol]]); соответствие DTO ↔ таблица — `DispatchCompanyConverter` и `DispatchMappingProfile`. На странице и в коде сайта — имя
Dispatch Company, сервис `DispatchCompanyService` (V1 `ActualCompanyService`).

## Доступ (как V1 `ActualCompaniesLookup` и `Menu.cshtml`)

| Роль | Видит | Может |
|---|---|---|
| Admin | все компании dispatch всех `Company`, колонка Company | создать (`+ Add`), выбрать Company, править, удалить |
| Owner | компании своей `Company` (у DDM Logistics их 5) | править, удалить; новую не создаёт, Company подставляется из входа |

Ссылка «Dispatch Companies» в шапке у Admin и Owner. Чужая компания, файл не из карточки — 404, `Create` у Owner — AccessDenied.

## Устройство

- `AdminDispatchCompaniesController`: `Index`, `Create`, `Edit/{id}`, `Save` (обычный POST multipart), `Delete/{id}` (IsDeleted),
  htmx: `LoadFmcsa`, `SendTestEmail`, `SendTestSchemeEmail`, `SendTestSms`; `Download/{id}?fileName=` — текущий шаблон RTF.
- Views `AdminDispatchCompanies/{Index,Edit,_DispatchCompanyFormPart}`; модалка V1 стала страницей — форма длинная, с файлами.
- Форма `DispatchCompanyFormViewModel` ↔ `ActualCompanyDto` — статический `DispatchCompanyFormConverter`; при сохранении форма
  накладывается на DTO из БД, поэтому `NextLoadExtra`, `FactoringCompanyJson`, `Bnsf*` не затираются.
- Stimulus: `board_controller` (поля только брокера / карриера — V1 knockout `visible`), `rows_controller` (строки Customizes,
  привязка через `Customizes.Index`), `mask_controller` для телефонов SMS.
- `LogoGenerator` (Logic/Helpers/Dispatch) — V1 `ImageGenerator` на SkiaSharp вместо System.Drawing: логотипы по Legal Name.
- Дефолты новой компании — как V1 `ActualCompanyModel.ts`: `smtp.gmail.com:587`, номера с 1, логотипы `wwwroot/images/dispatch/`.

## Отличия от V1

- Пароли SMTP, Scheme SMTP и BNSF не выводятся в форму (V1 показывал текстом): задан — в поле точки (маска `********`), не задан — пусто с подсказкой «not set»; маска или пусто — остаётся
  прежний (`DispatchCompanyService.OnUpdate`, как и в V1 пустая строка → null).
- Test SMS — Bandwidth (`SmsWebService`), V1 слал через Voxbone, который в V1 не работал и не переносится.
- Remove у Customizes работает: `SaveAsync` удаляет строки, которых нет на форме (в V1 удалялись только все сразу при `Customizes == null`).
- Размер логотипов (64x64, 1024x256) проверяет сервер; V1 проверял на клиенте и второй проверкой смотрел не тот `img`.
- `RemoveCompanyCache` после сохранения не вызывается: в V1 он чистил кэш событий по Id компании dispatch / пользователя — ключи не
  совпадали, вызов ничего не делал.
- Test SMTP для новой компании без пароля не падает NRE (`dbCompany?.SMTP_Password`).
- После сохранения V1 писал «Please restart AutoCall» — на странице постоянная подсказка: изменения видны в App после перезапуска.

## Проверено локально (2026-11-20)

Временный пользователь Owner, потом Admin DDM Logistics в `autodispatch_local` (после проверки удалён):
- Owner: список из 5 компаний без колонки Company и без `+ Add`; `Create` → AccessDenied; чужой Id и подменённое имя файла → 404.
- Сохранение формы без изменений: логотипы совпадают с `autocall.d_ActualCompany` (SHA-256), пароль BNSF и пустой SMTP сохранились.
- Customizes: добавление и удаление строки. Load FMCSA по MC 600182: поля и логотипы. Test SMTP без пароля — текст ошибки Gmail.
- Шаблон invoice: загрузка и скачивание побайтно; логотип 500x100 вместо малого — ошибка «must be lesser than 64x64».
- Admin: колонка Company, `+ Add`; создание (дефолты V1) и удаление — тестовая строка удалена из БД.
- В браузере страница глазами не смотрелась — проверено через HTTP (форма разбиралась скриптом).
