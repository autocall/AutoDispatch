-- Боевые БД AutoDispatch на db.myautocall.net и логин приложения (решено 2026-11-11: autodispatch + autodispatch_log, без суффикса _v3).
-- Пароль — только в appsettings.Production.json на сервере (/home/autodispatch/config).
-- Требует SQL Server 2019+ (collation *_UTF8). Таблицы создают EF-миграции при старте приложения и Serilog sink (Log).

use master;
if db_id('autodispatch') is null begin
    create database autodispatch collate Latin1_General_100_CI_AI_SC_UTF8;
end;
if db_id('autodispatch_log') is null begin
    create database autodispatch_log collate Latin1_General_100_CI_AI_SC_UTF8;
end;
if suser_id('autodispatch') is null begin
    create login autodispatch with password = '<password>', check_policy = on;
end;

use autodispatch;
if database_principal_id('autodispatch') is null begin
    create user autodispatch for login autodispatch;
end;
alter role db_owner add member autodispatch; -- миграции создают таблицы; после стабилизации схемы сузить до datareader/datawriter

use autodispatch_log;
if database_principal_id('autodispatch') is null begin
    create user autodispatch for login autodispatch;
end;
alter role db_owner add member autodispatch;
