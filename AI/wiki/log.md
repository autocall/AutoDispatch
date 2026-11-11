# Wiki Log

## [2026-11-11] ingest | старт wiki AutoDispatch V3

Создана база знаний нового репозитория по итогам анализа V1 (`D:\Develop\AutoCall`) и AutoCall V3 (`D:\Develop\AutoCallV3\Repo`).
Страницы: [[plan]] (этапы, оценка, решения), [[v1-inventory]] (что переносим), [[protocol]] (конверт `EventRequest`, reflection,
`TypeNameHandling`, long-polling), [[data-migration]] (замеры боевой БД и файлов, блоки скрипта), [[deploy]] (хост, контейнер, volume).
Решения пользователя 2026-11-11: переносить 7 компаний с событиями за 6 месяцев, `d_Event` для них переносить, `d_Message`
и `ChatService` не переносить, места под 147.5 ГБ файлов на хосте хватает. Установлено: `Microsoft.ReportViewer` к dispatch
не относится (`Web/Reports/*.aspx` AutoCall и `SimilarLoadsDialog` лоадборда).
