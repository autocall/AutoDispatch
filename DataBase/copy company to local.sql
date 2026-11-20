-- Копия одной компании из пробного переноса (autodispatch на db.myautocall.net) в локальную autodispatch_local — для проверки App
-- под настоящим пользователем (AI/wiki/data-migration.md «Локальная копия компании»). Выполняется в SSMS на (local)\SQLEXPRESS под Windows-учёткой,
-- источник читается через linked server DB.MYAUTOCALL.NET (OPENQUERY: фильтр по компании выполняется на сервере).
--
-- Что копируется: компания пользователя @email и всё её содержимое (DispatchCompany, справочники, техника, водители, грузы, setup packet,
-- события, Location-история, снимки HistoryObject, на которые есть ссылки или ReferenceId объектов компании), сам пользователь с ролями
-- и настройками App (UserSetting), каталог SetupPacketFactoringCompany целиком. Другие пользователи компании не копируются.
-- SipLogin/SipPassword — из autocall.dbo.Users (в пробном переносе колонок ещё нет). Колонки — общие у локальной и серверной таблицы.
-- Пароль: @passwordFrom — email локального пользователя, чей пароль поставить (по умолчанию admin), null — оставить пароль V1.
--
-- Повторный запуск заменяет копию: в autodispatch_local держится одна скопированная компания; служебные Company/System/Admin не трогаются,
-- всё остальное (данные dispatch, пользователи не служебной компании, их роли и настройки) удаляется.
-- Транзакции нет: чтение linked server в пользовательской транзакции требует MSDTC. Документы (/files) не копируются — FileLoad их не найдёт.
-- Объём: LEVSKI INC — 590 грузов / 1 тыс. событий, DDM Logistics — 3.6 тыс. / 450 тыс., BEST AMERICAN — 51 тыс. / 7.7 млн (долго).

use autodispatch_local;
set nocount on;
set xact_abort on;

declare @email        nvarchar(256) = N'pilipenko.anton@gmail.com'; -- пользователь из autodispatch на сервере (тестовый, Owner DDM Logistics)
declare @passwordFrom nvarchar(256) = N'admin@myautocall.net';   -- null — пароль V1
declare @service      uniqueidentifier = '00000000-0000-0000-0000-000000001000';  -- служебная компания AutoDispatch.Web (System, Admin)

declare @sql nvarchar(max), @q nvarchar(max);

-------- пользователь и компания на сервере --------
if object_id('tempdb..#src') is not null drop table #src;
create table #src (UserId uniqueidentifier, CompanyId uniqueidentifier);
set @q = N'select Id, CompanyId from autodispatch.dbo.IdentityUsers where Email = ''' + replace(@email, '''', '''''') + N'''';
set @sql = N'insert into #src select * from openquery([DB.MYAUTOCALL.NET], ''' + replace(@q, '''', '''''') + N''')';
exec (@sql);
if (select count(*) from #src) <> 1 begin raiserror('Пользователь %s не найден в autodispatch на сервере', 16, 1, @email); return; end;

declare @u nvarchar(36) = (select cast(UserId as nvarchar(36)) from #src);
declare @c nvarchar(36) = (select cast(CompanyId as nvarchar(36)) from #src);
print 'user ' + @u + ', company ' + @c;

-------- колонки серверных таблиц (берутся только общие с локальными) --------
if object_id('tempdb..#rcol') is not null drop table #rcol;
create table #rcol (TableName sysname collate database_default, ColumnName sysname collate database_default);  -- tempdb в collation сервера, autodispatch_local — UTF8
insert into #rcol
select * from openquery([DB.MYAUTOCALL.NET], 'select TABLE_NAME, COLUMN_NAME from autodispatch.INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = ''dbo''');

-------- что копировать: таблица и фильтр на сервере ({c} — компания, {u} — пользователь) --------
declare @ad nvarchar(50) = N'autodispatch.dbo.';
if object_id('tempdb..#copy') is not null drop table #copy;
create table #copy (Ord int identity, TableName sysname, Filter nvarchar(max));
insert into #copy (TableName, Filter) values
('Company',                     'Id = ''{c}'''),
('IdentityUsers',               'Id = ''{u}'''),
('IdentityUserRoles',           'UserId = ''{u}'''),
('UserSetting',                 'UserId = ''{u}'''),
('CompanyCustomStatus',         'CompanyId = ''{c}'''),
('DispatchCompany',             'CompanyId = ''{c}'''),
('DispatchCompanyCustom',       'DispatchCompanyId in (select EntityId from autodispatch.dbo.DispatchCompany where CompanyId = ''{c}'')'),
('CustomerCompany',             'CompanyId = ''{c}'''),
('ShipperReceiverCompany',      'CompanyId = ''{c}'''),
('SetupPacketFactoringCompany', '1 = 1'),
('SetupPacketContact',          'EntityId in (select x.Id from autodispatch.dbo.SetupPacket p cross apply (values (p.AfterHourContactId), (p.ConfidantContactId), (p.CorporateContactId), (p.DispatchContactId), (p.SalesContactId)) x(Id) where p.CompanyId = ''{c}'')'),
('SetupPacketInsuranceAgency',  'EntityId in (select x.Id from autodispatch.dbo.SetupPacket p cross apply (values (p.AutoInsuranceAgencyId), (p.CargoInsuranceAgencyId), (p.GeneralLiabilityInsuranceAgencyId), (p.WorkersCompensationInsuranceAgencyId)) x(Id) where p.CompanyId = ''{c}'')'),
('SetupPacket',                 'CompanyId = ''{c}'''),
('SetupPacketReference',        'SetupPacketId in (select EntityId from autodispatch.dbo.SetupPacket where CompanyId = ''{c}'')'),
('DatCompany',                  'CompanyId = ''{c}'''),
('DatDriver',                   'CompanyId = ''{c}'''),
('Yard',                        'CompanyId = ''{c}'''),
('Warehouse',                   'CompanyId = ''{c}'''),
('Trailer',                     'CompanyId = ''{c}'''),
('Truck',                       'CompanyId = ''{c}'''),
('Driver',                      'CompanyId = ''{c}'''),
('DefectRepair',                'CompanyId = ''{c}'''),
('Defect',                      'CompanyId = ''{c}'''),
('Load',                        'CompanyId = ''{c}'''),
('Event',                       'CompanyId = ''{c}'''),
('HistoryLocationTrailer',      'TrailerId in (select EntityId from autodispatch.dbo.Trailer where CompanyId = ''{c}'')'),
('HistoryLocationTruck',        'TruckId in (select EntityId from autodispatch.dbo.Truck where CompanyId = ''{c}'')'),
('HistoryLocationDriver',       'DriverId in (select EntityId from autodispatch.dbo.Driver where CompanyId = ''{c}'')'),
('HistoryLocationDatDriver',    'DatDriverId in (select EntityId from autodispatch.dbo.DatDriver where CompanyId = ''{c}'')'),
('HistoryLocationLoad',         'LoadId in (select EntityId from autodispatch.dbo.[Load] where CompanyId = ''{c}'')'),
-- снимки: на которые ссылаются скопированные строки, и все снимки объектов компании (LoadHistory2 и др. читают по ReferenceId)
('HistoryObject',               'Id in (
    select HistoryId from autodispatch.dbo.[Load] where CompanyId = ''{c}''
    union select HistoryId from autodispatch.dbo.Driver where CompanyId = ''{c}''
    union select HistoryId from autodispatch.dbo.Truck where CompanyId = ''{c}''
    union select HistoryId from autodispatch.dbo.Trailer where CompanyId = ''{c}''
    union select HistoryId from autodispatch.dbo.Warehouse where CompanyId = ''{c}''
    union select HistoryId from autodispatch.dbo.Yard where CompanyId = ''{c}''
    union select HistoryId from autodispatch.dbo.DatCompany where CompanyId = ''{c}''
    union select HistoryId from autodispatch.dbo.DatDriver where CompanyId = ''{c}''
    union select HistoryId from autodispatch.dbo.SetupPacket where CompanyId = ''{c}''
    union select HistoryId from autodispatch.dbo.Event where CompanyId = ''{c}''
    union select h.HistoryId from autodispatch.dbo.HistoryLocationTrailer h join autodispatch.dbo.Trailer x on x.EntityId = h.TrailerId where x.CompanyId = ''{c}''
    union select h.HistoryId from autodispatch.dbo.HistoryLocationTruck h join autodispatch.dbo.Truck x on x.EntityId = h.TruckId where x.CompanyId = ''{c}''
    union select h.HistoryId from autodispatch.dbo.HistoryLocationDriver h join autodispatch.dbo.Driver x on x.EntityId = h.DriverId where x.CompanyId = ''{c}''
    union select h.HistoryId from autodispatch.dbo.HistoryLocationDatDriver h join autodispatch.dbo.DatDriver x on x.EntityId = h.DatDriverId where x.CompanyId = ''{c}''
    union select v.Id from autodispatch.dbo.HistoryLocationLoad h join autodispatch.dbo.[Load] x on x.EntityId = h.LoadId
        cross apply (values (h.HistoryId), (h.HistoryDatCompanyId), (h.HistoryWarehouseId), (h.HistoryShipmentId)) v(Id) where x.CompanyId = ''{c}''
) or ReferenceId in (
    select EntityId from autodispatch.dbo.[Load] where CompanyId = ''{c}''
    union select EntityId from autodispatch.dbo.Driver where CompanyId = ''{c}''
    union select EntityId from autodispatch.dbo.Truck where CompanyId = ''{c}''
    union select EntityId from autodispatch.dbo.Trailer where CompanyId = ''{c}''
    union select EntityId from autodispatch.dbo.Warehouse where CompanyId = ''{c}''
    union select EntityId from autodispatch.dbo.Yard where CompanyId = ''{c}''
    union select EntityId from autodispatch.dbo.DatCompany where CompanyId = ''{c}''
    union select EntityId from autodispatch.dbo.DatDriver where CompanyId = ''{c}''
    union select EntityId from autodispatch.dbo.SetupPacket where CompanyId = ''{c}''
)');

-------- FK выключены на время копии (циклы Driver <-> Load, Truck <-> Trailer) --------
exec sp_MSforeachtable 'alter table ? nocheck constraint all';

-------- очистка прошлой копии --------
declare @t sysname;
declare del cursor local fast_forward for
    select TableName from #copy where TableName not in ('Company', 'IdentityUsers', 'IdentityUserRoles', 'UserSetting') order by Ord desc;
open del; fetch next from del into @t;
while @@fetch_status = 0 begin
    exec (N'delete from dbo.' + N'[' + @t + N']');
    fetch next from del into @t;
end;
close del; deallocate del;
delete from dbo.UserSetting        where UserId in (select Id from dbo.IdentityUsers where CompanyId <> @service);
delete from dbo.IdentityUserRoles  where UserId in (select Id from dbo.IdentityUsers where CompanyId <> @service);
delete from dbo.IdentityUserClaims where UserId in (select Id from dbo.IdentityUsers where CompanyId <> @service);
delete from dbo.IdentityUserLogins where UserId in (select Id from dbo.IdentityUsers where CompanyId <> @service);
delete from dbo.IdentityUserToken  where UserId in (select Id from dbo.IdentityUsers where CompanyId <> @service);
delete from dbo.IdentityUsers      where CompanyId <> @service;
delete from dbo.Company            where Id <> @service;

-------- копирование --------
declare @filter nvarchar(max), @cols nvarchar(max), @identity bit, @start datetime2 = sysdatetime();
declare cp cursor local fast_forward for select TableName, Filter from #copy order by Ord;
open cp; fetch next from cp into @t, @filter;
while @@fetch_status = 0 begin
    select @cols = string_agg(cast(quotename(c.name) as nvarchar(max)), ',') within group (order by c.column_id)
    from sys.columns c
    where c.object_id = object_id(N'dbo.' + quotename(@t))
      and exists (select 1 from #rcol r where r.TableName = @t and r.ColumnName = c.name);
    set @identity = objectproperty(object_id(N'dbo.' + quotename(@t)), 'TableHasIdentity');

    set @q = N'select ' + @cols + N' from ' + @ad + quotename(@t) + N' where '
           + replace(replace(@filter, '{c}', @c), '{u}', @u);
    set @sql = case when @identity = 1 then N'set identity_insert dbo.' + quotename(@t) + N' on; ' else N'' end
             + N'insert into dbo.' + quotename(@t) + N' (' + @cols + N') select ' + @cols
             + N' from openquery([DB.MYAUTOCALL.NET], ''' + replace(@q, '''', '''''') + N''');'
             + N' print ''' + @t + N': '' + cast(@@rowcount as varchar(20));'
             + case when @identity = 1 then N' set identity_insert dbo.' + quotename(@t) + N' off;' else N'' end;
    if len(@q) > 8000 begin raiserror('OPENQUERY для %s длиннее 8000 символов', 16, 1, @t); return; end;
    exec (@sql);
    fetch next from cp into @t, @filter;
end;
close cp; deallocate cp;

-------- SIP-учётка из V1 и локальный пароль --------
if object_id('tempdb..#sip') is not null drop table #sip;
create table #sip (SipLogin varchar(10), SipPassword varchar(50));
set @q = N'select left(nullif(ltrim(rtrim(MicroSIPLogin)), ''''), 10) as SipLogin, left(nullif(MicroSIPPassword, ''''), 50) as SipPassword from autocall.dbo.Users where Id = ''' + @u + N'''';
set @sql = N'insert into #sip select * from openquery([DB.MYAUTOCALL.NET], ''' + replace(@q, '''', '''''') + N''')';
exec (@sql);
update u set SipLogin = s.SipLogin, SipPassword = s.SipPassword
from dbo.IdentityUsers u cross join #sip s
where u.Id = @u and u.SipLogin is null;

if @passwordFrom is not null
    update dbo.IdentityUsers
    set PasswordHash = (select PasswordHash from dbo.IdentityUsers where Email = @passwordFrom),
        LockoutEnd = null, AccessFailedCount = 0
    where Id = @u;

-------- FK обратно, с проверкой; ссылка за пределы компании выводится и FK остаётся непроверенным --------
declare @fkTable sysname;
declare fk cursor local fast_forward for select name from sys.tables where is_ms_shipped = 0;
open fk; fetch next from fk into @fkTable;
while @@fetch_status = 0 begin
    begin try
        exec (N'alter table dbo.' + N'[' + @fkTable + N'] with check check constraint all');
    end try
    begin catch
        print 'FK ' + @fkTable + ': ' + error_message();
        exec (N'alter table dbo.' + N'[' + @fkTable + N'] check constraint all');
    end catch;
    fetch next from fk into @fkTable;
end;
close fk; deallocate fk;

print 'done in ' + cast(datediff(second, @start, sysdatetime()) as varchar(20)) + ' s; вход: ' + @email
    + case when @passwordFrom is null then ' (пароль V1)' else ' (пароль ' + @passwordFrom + ')' end;
