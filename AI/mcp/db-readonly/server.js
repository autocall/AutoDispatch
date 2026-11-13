import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import dotenv from "dotenv";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

const __dirname = dirname(fileURLToPath(import.meta.url));
dotenv.config({ path: join(__dirname, ".env") });

// Two targets, each exposed as its own tool so the database is visible at the call site:
//   query        -> main AutoCall database, db.myautocall.net (DB_*)
//   query_remote -> second server (REMOTE_DB_*), registered only when configured
//
// DB_TRUSTED=true -> Windows authentication over the ODBC driver (local named instance, no SQL login
// and no static TCP port required). The remote target is always a SQL login, so it uses tedious.
const trusted = process.env.DB_TRUSTED === "true";
const sqlDefault = (await import("mssql")).default;
const sqlTrusted = trusted ? (await import("mssql/msnodesqlv8.js")).default : null;

// App name на подключении — чтобы отличить эти запросы от остального трафика на том же сервере
// (Extended Events фильтр по client_app_name).
const APP_NAME = "AutoCallClaudeDbReadonly";

function localConfig() {
  return trusted
    ? {
        connectionString:
          `Driver={${process.env.DB_ODBC_DRIVER ?? "ODBC Driver 17 for SQL Server"}};` +
          `Server=${process.env.DB_SERVER};Database=${process.env.DB_NAME};` +
          `Trusted_Connection=yes;TrustServerCertificate=yes;App=${APP_NAME};`,
      }
    : {
        server: process.env.DB_SERVER,
        database: process.env.DB_NAME,
        user: process.env.DB_USER,
        password: process.env.DB_PASSWORD,
        options: { trustServerCertificate: true, appName: APP_NAME },
      };
}

function remoteConfig() {
  return {
    server: process.env.REMOTE_DB_SERVER,
    database: process.env.REMOTE_DB_NAME,
    user: process.env.REMOTE_DB_USER,
    password: process.env.REMOTE_DB_PASSWORD,
    options: { trustServerCertificate: true, encrypt: process.env.REMOTE_DB_ENCRYPT !== "false", appName: APP_NAME },
    // прод за сетью: не ждать вечно на недоступном хосте
    connectionTimeout: Number(process.env.REMOTE_DB_TIMEOUT ?? 15000),
    requestTimeout: Number(process.env.REMOTE_DB_REQUEST_TIMEOUT ?? 60000),
  };
}

// Пул создаётся при первом запросе, а не на старте: недоступный или несконфигурированный remote
// не должен мешать серверу подняться и обслуживать локальные запросы.
function lazyPool(driver, buildConfig) {
  let connecting = null;
  return () => {
    if (connecting == null) {
      const pool = new driver.ConnectionPool(buildConfig());
      connecting = pool.connect().catch((e) => {
        connecting = null; // дать следующему вызову попробовать снова
        throw e;
      });
      connecting = connecting.then(() => pool);
    }
    return connecting;
  };
}

// With a db_datareader-only login SQL Server rejects writes itself, but a trusted connection runs as
// the developer's Windows account (sysadmin), so the statement is checked here as well.
const forbidden = /\b(insert|update|delete|merge|drop|alter|create|truncate|exec|execute|grant|revoke|backup|restore|into)\b/i;

function assertReadOnly(sqlText) {
  const text = sqlText.trim().replace(/;+\s*$/, "");
  if (/^(select|with)\b/i.test(text) == false) throw new Error("Only SELECT / WITH statements are allowed.");
  if (text.includes(";")) throw new Error("Only a single statement is allowed.");
  const match = text.match(forbidden);
  if (match != null) throw new Error(`Statement contains a forbidden keyword: ${match[0]}`);
}

const server = new McpServer({ name: "db-readonly", version: "1.1.0" });

function registerQueryTool(name, description, getPool) {
  server.tool(name, description, { sql: z.string().describe("SQL SELECT statement to run") }, async ({ sql: sqlText }) => {
    assertReadOnly(sqlText);
    const pool = await getPool();
    const result = await pool.request().query(sqlText);
    return { content: [{ type: "text", text: JSON.stringify(result.recordset, null, 2) }] };
  });
}

registerQueryTool(
  "query",
  `Run a read-only SQL query against the AutoCall database ${process.env.DB_NAME} on ${process.env.DB_SERVER} ` +
    "(single SELECT / WITH statement; writes are rejected).",
  lazyPool(trusted ? sqlTrusted : sqlDefault, localConfig)
);

if (process.env.REMOTE_DB_SERVER) {
  registerQueryTool(
    "query_remote",
    `Run a read-only SQL query against the SECOND database ${process.env.REMOTE_DB_NAME} on ${process.env.REMOTE_DB_SERVER} ` +
      "(single SELECT / WITH statement; writes are rejected). Use it only when the question is about " +
      "that server; the main database is the `query` tool.",
    lazyPool(sqlDefault, remoteConfig)
  );
}

const transport = new StdioServerTransport();
await server.connect(transport);
