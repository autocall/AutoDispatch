# Деплой AutoDispatch V3 — тот же хост, свой контейнер

Повторяет `AutoCallV3/Repo/AI/wiki/deploy.md` и `Deploy/`: Docker runtime-образ, сборка через удалённый docker context
`ububuilder`, nginx + Let's Encrypt на хосте, секреты только в `appsettings.Production.json` на хосте, публикация руками.
Отличия от AutoCall V3 — поддомен, порт, каталоги и volume `/files`.

## Целевая схема

| | AutoCall V3 (есть) | AutoDispatch V3 |
|---|---|---|
| Хост | `67.220.74.190`, Ubuntu 24.04, Docker 28, nginx, certbot | тот же |
| Домен | `v3.myautocall.net` | `dispatch.myautocall.net` (решено 2026-11-11) |
| Контейнер / порт | `autocall.web` → `127.0.0.1:5004` | `autodispatch.web` → `127.0.0.1:5005` |
| Каталоги хоста | `/home/autocall/{logs,keys,config,download}` | `/home/autodispatch/{logs,keys,config,download,files}` |
| БД | `db.myautocall.net`: `autocall_v3`, `autocall_v3_log` | `autodispatch`, `autodispatch_log`, логин `autodispatch` (`DataBase/create autodispatch.sql`; решено 2026-11-11) |
| Образ | `mcr.microsoft.com/dotnet/aspnet:10.0`, `ASPNETCORE_URLS=http://+:8080`, volume `/logs /keys /download` | + volume `/files` (документы — весь `E:\Files` V1, 255 ГБ; свободно 765 ГБ — 2026-11-19) |
| Публикация Web | `Scripts/publish-autocall-web.ps1` | `Scripts/publish-autodispatch-web.ps1`: те же шаги, `-v /home/autodispatch/files:/files` |
| Публикация App | `Scripts/publish-autocall-app.ps1` (ClickOnce, MSBuild VS 18, `tar | ssh` в `/download`) | `Scripts/publish-autodispatch-app.ps1`, `AssemblyName = AutoDispatch`, `/Download` нового контейнера |
| Файлы | FileZilla: весь `E:\Files` → `/home/safer/temp`, затем `Scripts/copy-dispatch-files.sh` → `/home/autodispatch/files` (каталоги в нижнем регистре); досинк — `robocopy /MAXAGE` + FileZilla + тот же скрипт (решено 2026-11-19, [[data-migration]] «Файлы») |

## `server-setup.txt` (по образцу V3)

1. `mkdir -p /home/autodispatch/{logs,keys,config,download,files}`, `chmod 600` на `config`.
2. DNS A-запись `dispatch.myautocall.net` → `67.220.74.190`.
3. `Deploy/nginx/dispatch.myautocall.net.conf`: `proxy_pass http://127.0.0.1:5005` с `X-Forwarded-*`, `client_max_body_size 100m` под загрузку
   документов (проверено 2026-11-18: App V1 не пускает файл больше 20 МБ — `FileSizeHelper.Check20MBSize`; в конверте `Request` это ~27 МБ base64;
   на сервере `Request`/`ExtractLoadFromPdf` с `DisableRequestSizeLimit`, так что предел задаёт nginx; в V3 дефолт 1 МБ не подойдёт).
   Лимит по IP (2026-11-26, [[audit]] D10, отличие от V3 — [[plan]] «Лимит входа и сброса пароля (D10)»): POST на вход (`Account/LogIn`,
   `Api/Account/SignIn`) и на сброс пароля (`Account/ForgotPassword`, `Api/Account/ForgotPassword`) — по 20 за 5 минут, отдельный счётчик на
   каждую пару; дальше 429 `{ error }`, отказы — `limiting requests` в `/var/log/nginx/error.log`. На хост — вручную в файл после certbot
   (`server-setup.txt`).
4. `certbot --nginx -d dispatch.myautocall.net`.
5. `appsettings.Production.json` из `Deploy/AutoDispatch.Web/appsettings.Production.json.example`.
6. `Scripts/publish-autodispatch-web.ps1`; проверка `curl /Test`, `docker logs`.

## Первая публикация — 2026-11-13

Выполнено по `server-setup.txt`: БД `autodispatch`/`autodispatch_log` и логин созданы, индексы логов созданы, nginx + certbot
(сертификат до 2027-02-11), контейнер `autodispatch.web:2026.11.13-2` на `127.0.0.1:5005`, `/Test` отвечает 200.
Грабли и уточнения:

- `appsettings.Production.json` скопирован с V3 — осталось `AllowedHosts: v3.myautocall.net`, приложение отвечало 400
  «The request hostname is invalid» (HostFiltering). Проверять при копировании конфига.
- **`/health`** (2026-11-27): без авторизации, 200 `OK` — процесс отвечает и обе БД доступны, иначе 503. Внешнего мониторинга
  и оповещений нет — решено не делать, логи и метрики смотрят явно ([[plan]]).
- **Метрика CPU**: у V3 скрипт стоит не на сервере БД, а на самом веб-хосте `67.220.74.190` — `/usr/local/bin/metric_cpu_2.sh`,
  cron root раз в минуту (`>> /var/log/cron`). Для AutoDispatch сделано так же: `/usr/local/bin/metric_cpu_3.sh`. То есть
  «DB CPU» (type 2) фактически измеряет CPU веб-хоста. `error code: 502` в `/var/log/cron` — от старого `metric_cpu.sh`
  (brokersnapshot.com), не наш.
  **Проверено 2026-11-27 по `autocall_v3_log.Metric`:** в V3 DB CPU не работал ни в одном варианте. 27.10–08.11 (SQL-замер
  `100 - SystemIdle` из ring buffer) — ровно 100 каждую минуту; с 09.11 (скрипт на веб-хосте) совпадает с Web CPU в 93–99 % минут
  (оба 0–11 %). Сервер БД `db.myautocall.net` = `3.141.158.251` (AWS), это не веб-хост `67.220.74.190`; на нём SQL Server 2019
  Standard **on Linux** (Ubuntu 20.04, `sys.dm_os_host_info`) — вероятно, поэтому ring buffer давал SystemIdle 0 и 100 %
  (у readonly-логина нет VIEW SERVER STATE, сам буфер не проверен). Скрипт `metric_cpu.sh` читает `/proc/stat` и рассчитан на
  Linux-сервер БД (так и написано в его шапке у V3) — чтобы DB CPU стал настоящим, его нужно поставить в cron на `3.141.158.251`
  (для V3 и AutoDispatch, токены каждого) и убрать с веб-хоста. Нужен SSH на сервер БД — решение пользователя.
  До этого колонка DB CPU на `/Admin/Metrics` скрыта (закомментирована в `Index.cshtml`), сбор работает.
- Публикация с машины разработчика под пользователем, отличным от Administrator: Node стоит через nvm в профиле Administrator,
  `C:\Program Files\nodejs` — symlink туда. Нужен доступ `nata:(RX)` на `C:\Users\Administrator`, `\AppData`, `\AppData\Roaming`
  и рекурсивно на `\AppData\Roaming\nvm` (`icacls`), иначе `npm` падает с EPERM при realpath. Временный обход —
  `NODE_OPTIONS=--preserve-symlinks --preserve-symlinks-main`.
- Образ без `npm run build` работает (API, `/Test`), но страницы без `wwwroot/build` — без стилей.

## Секреты `appsettings.Production.json`

`ConnectionStrings: Default, Log`; `Secrets: Jwt:PrivateKey, System:Password/AllowedIps, Mailgun, GoogleMap (Api + JsKey),
Map:Key, Metric:Token` — как в V3; dispatch-специфичные: `CarrierView`, `Sms` (Voxbone, Bandwidth), `OpenAI`, `SaferFmcsa`,
`Bnsf`, `DatRate`, `Keeppingin`, `Vodia` (FTP CDR для Call Report, как AutoCall V3 — без него Release не стартует), `GmailSmtp` (если письма компаний идут через Gmail, а не Mailgun — **уточнить**),
`SetupPacketVersion`, `FilesPath: /files` (подкаталоги `_recycle`, `_temp`, `_fonts` — последний для `wingding.ttf`, которого нет в репозитории, см. [[plan]] «Шрифты в репозитории»). `AllowedHosts` = `dispatch.myautocall.net`.

**OpenAI (проверено 2026-11-27):** ключ `sk-oaRqV…IMYO` — тот же, что в исходниках V1, — OpenAI отклоняет (`Incorrect API key provided` в `logs/openai` от 2026-11-20 и 2026-11-22). Разбор rate confirmation из PDF не работает, пока в `Secrets:OpenAI:ApiKey` не будет действующий ключ. Проверять только на сервере: OpenAI забирает страницы PDF по URL `{Website}/Temp/…`, с `localhost` он их не получит.

## Что меняется в `Program.cs` относительно V3

`UseStaticFiles` для `/Download` остаётся; `/files` не раздаётся статикой — документы отдаёт API (`FileLoad`) и страницы
setup packet (`DownloadFileMC/COI`) с проверкой доступа, как в V1. `DataProtection` — `/keys`. Long-polling `Response`:
таймаут Kestrel/nginx `proxy_read_timeout` должен быть больше времени ожидания в `EventNotificationAsync` (**уточнить** значение V1).

## Публикация App (этап 7, 2026-11-19)

`Sources/App/AutoCall.WPF/Properties/PublishProfiles/ClickOnceProfile.pubxml`: `InstallUrl https://dispatch.myautocall.net/Download/`,
локально `D:\Publish\AutoDispatch_Publish\`, рантайм-пререквизит .NET Desktop 10 **x86** (App x86; в V3 — x64). `Scripts/publish-autodispatch-app.ps1` —
копия V3: MSBuild VS 18, `tar | ssh` в `/home/autodispatch/download`. Подпись манифестов — сертификат AutoCall V3 (решено 2026-11-19):
`AutoCall.Publish_3_TemporaryKey.pfx` скопирован из `AutoCallV3/Repo/Sources/App/AutoCall.View` в `Sources/App/AutoCall.WPF` (в `.gitignore`),
отпечаток `F3C16FA5…FFF9E`, `CN=AutoCall`, самоподписанный до 2126; для подписи должен стоять в `CurrentUser\My` машины, с которой публикуют.
Проверено 2026-11-19 пробным `msbuild /t:Publish` без выгрузки. Тот же ключ нужен для всех обновлений — хранить копию `.pfx`. В `/home/autodispatch/download` вручную положить `New Driver Application Package.pdf` из V1 `Web/Download`
(353 КБ, 2020): бланк пакета документов для приёма нового водителя, который диспетчер скачивает ссылкой в шапке справочника
Drivers (`DriversDialog`) — App берёт его с `{WebUrl}/Download/`. `fw9.pdf` из той же папки V1 не нужен: W-9 сервер берёт из `DocumentsPath`. Подробнее — [[app]].

## Переключение с V1 — внешние настройки

- **Webhook CarrierView**: в настройках аккаунта CarrierView (компания UNITED GLOBAL — единственная с `CarrierViewToken`) сменить
  адрес webhook с сайта V1 (`…/Dispatch/UpdateLocation` на домене V1) на `https://dispatch.myautocall.net/Dispatch/UpdateLocation`.
  Путь в V3 тот же (`CarrierViewController`, перенесён 2026-11-22), меняется только домен. Пока адрес не сменён, позиции и статусы
  трекеров приходят в V1, а V3 обновляет их только по Sync Tracker. Проверка после смены: `CarrierViewLastUpdatedDate` у грузов с
  трекером в `autodispatch` растёт, в логе `UpdateLocation: …` ([[app-testing]] 6.14).

## Бэкап

Файлы в `/home/autodispatch/files` сейчас нигде не бэкапятся (на Windows они под бэкапом сервера) — **решить** в [[plan]].
БД — на `db.myautocall.net` по общей схеме.
