-- Локальная БД для Debug (AppSettings.Environment.Local.cs, appsettings.Development.json).
-- Подключение Windows-аутентификацией к (local)\SQLEXPRESS — отдельный логин не нужен.
-- Таблицы создаёт EF-миграция Init (dotnet ef migrations add Init / database update в AutoDispatch.Web.Data, Scripts/commands.txt).
-- Collation UTF8 на уровне БД, как у боевой: varchar хранит кириллицу без nvarchar в модели.

use master;
if db_id('autodispatch_local') is null begin
    create database autodispatch_local collate Latin1_General_100_CI_AI_SC_UTF8;
end;
