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
    create login autodispatch with password = 'XuNg8SwUPKtufWdoyiT1sffk', check_policy = on;
end;
go
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



--USE [master];

--DECLARE @kill varchar(8000) = '';  
--SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'  
--FROM sys.dm_exec_sessions
--WHERE database_id  = db_id('autodispatch_log')

--EXEC(@kill);

--drop database autodispatch_log



-- REMOTE_DB_PASSWORD=F9Q641I_6-bw