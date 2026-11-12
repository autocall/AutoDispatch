# Wiki Index

## AutoDispatch V3

- [plan](plan.md) — этапы переноса AutoDispatch из V1 на Ubuntu по образцу AutoCall V3 (репозиторий → скелет Web → модель данных → серверная логика → API для App → публичный сайт → клиент → деплой и переключение → контроль), оценка 3–4 месяца, таблица решений (решено 2026-11-11: Guid-ключи, `dispatch.myautocall.net`, БД `autodispatch`, своя Identity с ролями AutoDispatch; что не переносим)
- [v1-inventory](v1-inventory.md) — что такое AutoDispatch в V1: контракт `AutoDispatch.Dtos`, проекты App и 213 файлов WPF, эндпоинты WebV1, BLL-сервисы, внешние сервисы, настройки, Setup Packet (15 шагов), 29 таблиц `d_*`, файлы `FilePath\{EntityId}`; что к dispatch не относится (`ReportViewer`, `SimilarLoadsDialog`)
- [protocol](protocol.md) — протокол App↔Web: один эндпоинт `Api/Dispatch/Request` с конвертом `EventRequest {Name, Data}`, вызов метода `IDispatchService` по имени через reflection, `TypeNameHandling.All`, long-polling `Response`; решения для V3 (контракт и имя сборки как есть, таблица `Name → handler`, `SerializationBinder`), список 74 методов
- [data-migration](data-migration.md) — перенос из `autocall`: замеры 2026-11-11 (7 активных компаний, 89 тыс. грузов, 4.85 млн снимков / 7.9 ГБ, 10.6 млн событий, 147.5 ГБ документов), блоки скрипта, отбор `d_HistoryObject` по ссылкам, `tar | ssh` для файлов, окно переключения
- [deploy](deploy.md) — тот же хост `67.220.74.190`, контейнер `autodispatch.web` на `127.0.0.1:5005`, поддомен `dispatch.myautocall.net`, БД `autodispatch` + `autodispatch_log`, каталоги `/home/autodispatch/*`, volume `/files`, секреты, отличия `Program.cs` и nginx от V3
- [web](web.md) — скелет AutoDispatch Web (этап 1, 2026-11-11): что скопировано из AutoCall V3, что вырезано, отличия (UserSetting для Map, RoleEnum, метрики, окружение, миграции не копировались), Deploy/Scripts/DataBase, что проверено и что ждёт миграций

## Эталоны (не копируются, читаются по месту)

- AutoCall V3 — `D:\Develop\AutoCallV3\Repo\AI\wiki\{web-architecture,frontend,deploy,logging,app-v3,data-model}.md`, `CLAUDE.md`
- V1 (прод) — `D:\Develop\AutoCall`, wiki `D:\Develop\AutoCall\AI` (домен, БД `autocall` — `08-database.md`)
