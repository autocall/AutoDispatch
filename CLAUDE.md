# Claude Behavior Rules (Karpathy)

## 1. Think Before Coding
**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them.
- If a simpler approach exists, say so.
- If something is unclear, stop and name what's confusing.

## 2. Simplicity First
**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" that wasn't requested.
- No error handling for impossible scenarios.
- If 200 lines could be 50, rewrite it.

## 3. Surgical Changes
**Touch only what you must. Clean up only your own mess.**

- Don't "improve" adjacent code or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice dead code, mention it — don't delete it.

## 4. Goal-Driven Execution
**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests, then make them pass"
- "Fix the bug" → "Reproduce it in a test, then fix"
- "Refactor X" → "Ensure tests pass before and after"

## Проект AutoDispatch V3

AutoDispatch — диспетчерский модуль AutoCall: грузы, водители, техника, карриеры и их setup packet, инвойсы и
подтверждения, карта. В V1 (`D:\Develop\AutoCall`) он живёт внутри общего WPF-клиента, сайта WebV1 и БД `autocall`.
V3 = **AutoDispatch App** (WPF-клиент, копия V1 без модуля AutoCall) + **AutoDispatch Web** (новый сервер на Ubuntu
в отдельном Docker-контейнере на том же хосте, что AutoCall V3, на своём поддомене, со своей БД).
**Логика и функционал клиента и сервера не меняются** — переносятся. AutoCall (лоадборд, звонки, DAT) — вне рамок проекта.

Хаб знаний — `AI/AutoDispatch.md`, план и решения — `AI/wiki/plan.md`.

### Структура репозитория

| Папка | Что |
|---|---|
| `Sources/` | код: `App/AutoDispatch.sln` — клиент (копия `Sources/App` V1: `AutoCall.WPF`, `AutoDispatch.*`, `_Common`, `_Dtos`), `Web/AutoDispatch.Web.sln` — сервер (копия скелета `Sources/Web` AutoCall V3), `Shared/AutoDispatch.Dtos` — общий контракт; у каждой папки свой `Directory.Build.props` |
| `AI/` | база знаний: `AutoDispatch.md` (хаб), `wiki/`, `llm-wiki.md`, `Inbox/` (gitignored) |
| `DataBase/` | SQL-скрипты: создание `autodispatch_v3`, перенос из `autocall` |
| `Deploy/` | Ubuntu: Dockerfile, nginx, `server-setup.txt`, `appsettings.Production.json.example` |
| `Scripts/` | публикация Web и App, синхронизация файлов |

### Эталоны

- Всё делается **так же, как в AutoCall V3** — `D:\Develop\AutoCallV3\Repo`: архитектура сервера (`AI/wiki/web-architecture.md`:
  Common/Data/Logic/Server, ServicesHost, UnitOfWork, LinqToDB для запросов, EF Core для Identity и миграций), фронт
  (`AI/wiki/frontend.md`: Razor + htmx + Stimulus + Tailwind v4 + daisyUI), клиент (`AI/wiki/app-v3.md`: `BaseApi`, JWT, Serilog,
  ClickOnce), деплой (`AI/wiki/deploy.md`, `Deploy/`, `Scripts/`), логирование и мониторинг (`AI/wiki/logging.md`).
  Если решение в этом репозитории отличается от V3 — оно должно быть записано в `AI/wiki/plan.md` с причиной.
- Исходник кода — V1 `D:\Develop\AutoCall\Sources`: что именно относится к AutoDispatch — `AI/wiki/v1-inventory.md`,
  протокол App↔Web — `AI/wiki/protocol.md`. Код V1 переносится, а не переписывается.
- Боевая БД `db.myautocall.net`, read-only доступ — MCP `db-readonly` из репозитория AutoCall V3 (`AI/mcp/db-readonly/`).

### Границы

- Контракт `AutoDispatch.Dtos` (типы, имена методов, имя сборки и namespace) не менять — на нём держится совместимость
  клиента и сервера (`AI/wiki/protocol.md`).
- Ключи БД — Guid, как в V1 (отступление от V3, см. «Решить» в `plan.md`). Не переводить на int.
- Не переносить: `d_Message`/`ChatService`, `ReportSimilarLoads`/`SimilarLoadsDialog`, `Microsoft.ReportViewer`, модуль AutoCall
  в клиенте, данные неактивных компаний.
- EF-миграции не генерировать и не править — их делает пользователь.
- Решения, помеченные в wiki как «уточнить» / «решить», не принимать молча — спросить.

## LLM Wiki (Knowledge Base)

`AI/llm-wiki.md` defines the pattern for maintaining a persistent, compounding wiki for this project. Follow it strictly.

**Core principle:** The LLM writes and maintains all wiki files. You never write wiki content manually.

**Three layers:**
- `AI/raw/` — immutable source documents (articles, transcripts, data). Never modify.
- `AI/wiki/` — LLM-maintained markdown pages: entity pages, concept pages, summaries, comparisons.
- `AI/wiki/index.md` — content catalog (all pages with one-line summaries, organized by category). Update on every ingest.
- `AI/wiki/log.md` — append-only chronological record of ingests, queries, lint passes. Format: `## [YYYY-MM-DD] operation | description`.

**Operations:**
1. **Ingest** (`"добавь источник"` / `"запиши"`): Read source → extract key info → write/update wiki pages → update index → append to log. One source may touch 10-15 pages.
2. **Query** (`"найди"` / `"что говорит wiki о..."`): Read `AI/wiki/index.md` first → drill into relevant pages → synthesize answer with page citations. Good answers get filed back as new wiki pages.
3. **Lint** (`"проверь wiki"`): Find contradictions, stale claims, orphan pages, missing cross-references, data gaps. Report findings; fix on request.
4. **Remember** (`"запомни"`): When the user says "запомни" about project knowledge (architecture, decisions, patterns, data structures, code skeletons) — create or update a page in `AI/wiki/`, update `AI/wiki/index.md`, append to `AI/wiki/log.md`. Memory (`~/.claude/projects/`) is only for Claude behavior rules (code style, feedback on approach).

**Conventions:**
- All wiki pages in `AI/wiki/` directory, markdown format.
- Cross-links between pages using `[[PageName]]` Obsidian-style links.
- Each page has a short one-line lead summary at the top.
- Log entries start with `## [YYYY-MM-DD]` for grep-ability.
