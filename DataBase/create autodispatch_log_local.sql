-- Локальная БД логов для Debug: Serilog (таблица Log создаётся sink-ом), UserLogin, UserActivity и Metric (миграции AdLogDbContext).
use master;
if db_id('autodispatch_log_local') is null begin
    create database autodispatch_log_local collate Latin1_General_100_CI_AI_SC_UTF8;
end;
