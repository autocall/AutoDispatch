-- Read-only login for the Claude MCP server db-readonly (AI/mcp/db-readonly). Run on db.myautocall.net.
-- Replace the password and put it into AI/mcp/db-readonly/.env (gitignored). The login can only read:
-- db_datareader + db_denydatawriter; the MCP server additionally rejects anything except a single select/with.

use master;
create login ClaudeReadonly with password = '<password>', check_policy = on;

use autocall;
create user ClaudeReadonly for login ClaudeReadonly;
alter role db_datareader add member ClaudeReadonly;
alter role db_denydatawriter add member ClaudeReadonly;
