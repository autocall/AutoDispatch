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
| Образ | `mcr.microsoft.com/dotnet/aspnet:10.0`, `ASPNETCORE_URLS=http://+:8080`, volume `/logs /keys /download` | + volume `/files` (документы, 150 ГБ, место есть — 2026-11-11) |
| Публикация Web | `Scripts/publish-autocall-web.ps1` | `Scripts/publish-autodispatch-web.ps1`: те же шаги, `-v /home/autodispatch/files:/files` |
| Публикация App | `Scripts/publish-autocall-app.ps1` (ClickOnce, MSBuild VS 18, `tar | ssh` в `/download`) | `Scripts/publish-autodispatch-app.ps1`, `AssemblyName = AutoDispatch`, `/Download` нового контейнера |
| Файлы | `Scripts/sync-dispatch-files.ps1` — первичная заливка и досинк ([[data-migration]]) |

## `server-setup.txt` (по образцу V3)

1. `mkdir -p /home/autodispatch/{logs,keys,config,download,files}`, `chmod 600` на `config`.
2. DNS A-запись `dispatch.myautocall.net` → `67.220.74.190`.
3. `Deploy/nginx/dispatch.myautocall.net.conf`: `proxy_pass http://127.0.0.1:5005` с `X-Forwarded-*`, `client_max_body_size` под загрузку
   документов (в V1 через App грузят PDF и сканы — размер **уточнить**, в V3 дефолт 1 МБ не подойдёт).
4. `certbot --nginx -d dispatch.myautocall.net`.
5. `appsettings.Production.json` из `Deploy/AutoDispatch.Web/appsettings.Production.json.example`.
6. `Scripts/publish-autodispatch-web.ps1`; проверка `curl /Test`, `docker logs`.

## Секреты `appsettings.Production.json`

`ConnectionStrings: Default, Log`; `Secrets: Jwt:PrivateKey, System:Password/AllowedIps, Mailgun, GoogleMap (Api + JsKey),
Map:Key, Metric:Token` — как в V3; dispatch-специфичные: `CarrierView`, `Sms` (Voxbone, Bandwidth), `OpenAI`, `SaferFmcsa`,
`Bnsf`, `DatRate`, `Keeppingin`, `GmailSmtp` (если письма компаний идут через Gmail, а не Mailgun — **уточнить**),
`SetupPacketVersion`, `FilesPath: /files`. `AllowedHosts` = `dispatch.myautocall.net`.

## Что меняется в `Program.cs` относительно V3

`UseStaticFiles` для `/Download` остаётся; `/files` не раздаётся статикой — документы отдаёт API (`FileLoad`) и страницы
setup packet (`DownloadFileMC/COI`) с проверкой доступа, как в V1. `DataProtection` — `/keys`. Long-polling `Response`:
таймаут Kestrel/nginx `proxy_read_timeout` должен быть больше времени ожидания в `EventNotificationAsync` (**уточнить** значение V1).

## Бэкап

Файлы в `/home/autodispatch/files` сейчас нигде не бэкапятся (на Windows они под бэкапом сервера) — **решить** в [[plan]].
БД — на `db.myautocall.net` по общей схеме.
