-- Индексы таблицы Log (БД логов) для /Admin/Logs и /Admin/Monitor. Таблицу со всеми колонками создаёт Serilog sink при первом
-- старте (Details и Session уже в ColumnOptions), поэтому миграции AutoCall V3 Alter_Log здесь нет — индексы ставятся этим
-- скриптом один раз после первого запуска, на боевой и локальной БД логов.
if object_id('Log') is not null begin
    if not exists (select 1 from sys.indexes where name = 'IX_Log_Scope_Id') create index IX_Log_Scope_Id on Log (Scope, Id);
    if not exists (select 1 from sys.indexes where name = 'IX_Log_UserName_Id') create index IX_Log_UserName_Id on Log (UserName, Id);
    if not exists (select 1 from sys.indexes where name = 'IX_Log_TimeStamp') create index IX_Log_TimeStamp on Log (TimeStamp);
end;
