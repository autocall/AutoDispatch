# AutoDispatch V3

Диспетчерский модуль AutoCall (грузы, водители, техника, карриеры, инвойсы, setup packet карриера) выделяется из V1
в отдельный сервер на Ubuntu и отдельный WPF-клиент — тем же способом, что AutoCall V3. Логика и функционал не меняются.

- Репозиторий: `D:\Develop\AutoDispatch\Repo`; клиент — `Sources\App` (копия WPF V1 без модуля AutoCall);
  сервер — `Sources\Web\AutoDispatch.Web.sln` (копия скелета AutoCall V3)
- Эталон: `D:\Develop\AutoCallV3\Repo` (CLAUDE.md, `AI/wiki`, `Deploy/`, `Scripts/`, `DataBase/`)
- Исходник: V1 `D:\Develop\AutoCall` (прод, WebV1 + AppV1 + AutoDispatch в одном решении)
- Прод: хост `67.220.74.190` (там же `v3.myautocall.net`), БД `db.myautocall.net`, read-only доступ — MCP `db-readonly` из V3

## Страницы

- [[plan]] — этапы, оценка, решения
- [[v1-inventory]] — что именно переносим из V1 и что к dispatch не относится
- [[protocol]] — `Api/Dispatch/Request|Response`, конверт `EventRequest`, что сохраняем в V3
- [[data-migration]] — замеры, блоки переноса, файлы 147.5 ГБ
- [[deploy]] — контейнер, поддомен, volume `/files`, секреты

Индекс — `AI/wiki/index.md`, журнал — `AI/wiki/log.md`.
