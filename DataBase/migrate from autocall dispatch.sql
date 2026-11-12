-- Перенос данных AutoDispatch из autocall (V1) -> autodispatch. Выполняется под sa на db.myautocall.net, блоками (AI/wiki/data-migration.md).
-- По образцу AutoCallV3/Repo/DataBase/migrate from autocall.sql. Таблицы autodispatch должны существовать (миграция EF применена сервером).
-- Сейчас здесь только блок 0 — компании и пользователи; блоки 1–5 (справочники, d_Load, d_HistoryObject, d_Event, файлы) добавляются позже.
--
-- Блок 0. Кого переносим:
--   * компании с событиями d_Event за последние 6 месяцев (на 2026-11-11 — 7 компаний);
--   * их не удалённые пользователи, у которых есть хотя бы одна роль с модулем AutoDispatch (Permissions.GetModulesByRole V1):
--     Agent, Warehouse, Dispatcher, Sales, Accounting, Owner, Admin — остальные роли V1 (User, Driver, Gps Support) не переносятся;
--   * и которые входили в систему за последние 6 месяцев (autocall_reference.dbo.Session, как в V3) — бывшие сотрудники без входа не нужны.
-- Роли переносятся все, какие есть у пользователя (в V1 RoleName — флаги, клиент считает сумму), Admin V1 остаётся Admin.
-- Пароли (PasswordHash Identity v2) и SecurityStamp копируются как есть — Identity Core проверяет старый формат и перехэширует при входе.
-- Маппинг Guid V1 -> int Identity хранится в постоянных таблицах dbo._MigrateCompany / dbo._MigrateUser: по ним блоки 1–5 подставляют
-- CompanyId и CreatedById/ModifiedById в d_* (ключи самих d_* остаются Guid).
-- Повторный запуск перезаливает блок целиком (шаг 0.0 удаляет строки своих диапазонов Id); пользователи, заведённые в autodispatch
-- руками (Id вне диапазонов), остаются и по Email пропускаются.

use autodispatch;
set nocount on;
set xact_abort on;

declare @since datetime2(0) = dateadd(month, -6, sysutcdatetime());
declare @systemUserId int = 1000;
-- диапазоны int-ключей переноса (DbRandomHelper генерирует случайные серии от 10 000, пересечение маловероятно)
declare @companyBase int = 10000000, @userBase int = 20000000;

if object_id('dbo._MigrateCompany') is null
    create table dbo._MigrateCompany (LegacyId uniqueidentifier not null primary key, NewId int not null unique);
if object_id('dbo._MigrateUser') is null
    create table dbo._MigrateUser (LegacyId uniqueidentifier not null primary key, NewId int not null unique, NewCompanyId int not null);

begin tran;

-------- 0.0 очистка прошлого переноса (служебные Company 1000 и пользователи 1000/1001 не задеты) --------
delete from dbo.UserSetting       where UserId between 20000000 and 29999999;
delete from dbo.IdentityUserRoles where UserId between 20000000 and 29999999;
delete from dbo.IdentityUsers     where Id     between 20000000 and 29999999;
delete from dbo.Company           where Id     between 10000000 and 19999999;
delete from dbo._MigrateUser;
delete from dbo._MigrateCompany;

-------- 0.1 активные компании --------
select distinct e.CompanyId as LegacyId
into #active
from autocall.dbo.d_Event e
where e.CreatedDate >= @since;

-------- 0.2 пользователи с ролями AutoDispatch --------
-- имена ролей V1 в таблице Roles совпадают с RoleName без пробелов ("Gps Support" -> GpsSupport), Id роли в autodispatch = RoleEnum
select ur.UserId as LegacyUserId,
       case replace(r.Name, ' ', '')
           when 'Agent'      then 1
           when 'Warehouse'  then 2
           when 'Dispatcher' then 3
           when 'Sales'      then 4
           when 'Accounting' then 5
           when 'Owner'      then 6
           when 'Admin'      then 7
       end as RoleId
into #role
from autocall.dbo.UserRoles ur
join autocall.dbo.Roles r on r.Id = ur.RoleId
where replace(r.Name, ' ', '') in ('Agent', 'Warehouse', 'Dispatcher', 'Sales', 'Accounting', 'Owner', 'Admin');

select
    u.Id as LegacyId,
    cast(@userBase + row_number() over (order by u.CreatedDate, u.Id) as int) as NewId,
    u.CompanyId as LegacyCompanyId,
    cast(null as int) as NewCompanyId,
    u.UserName, u.Email, u.PasswordHash, u.SecurityStamp, u.PhoneNumber, u.LockoutEnabled, u.LockoutEndDateUtc,
    u.CreatedDate, u.LastModifiedDate
into #user
from autocall.dbo.Users u
where u.IsDeleted = 0
  and u.CompanyId in (select LegacyId from #active)
  and exists (select 1 from #role r where r.LegacyUserId = u.Id)
  and exists (select 1 from autocall_reference.dbo.Session s where s.CreatedById = u.Id and s.LastActivity >= @since)
  and not exists (select 1 from dbo.IdentityUsers x where x.NormalizedEmail = upper(u.Email) collate database_default);

-------- 0.3 компании отобранных пользователей (удалённые восстанавливаются активными) --------
select c.EntityId as LegacyId, cast(@companyBase + row_number() over (order by c.CreatedDate, c.EntityId) as int) as NewId,
       c.CompanyName, c.CreatedDate, c.LastModifiedDate
into #company
from autocall.dbo.Company c
where c.EntityId in (select distinct LegacyCompanyId from #user)
  and not exists (select 1 from dbo.Company x where x.Name = c.CompanyName collate database_default and x.IsDeleted = 0);

-- компания уже есть в autodispatch (заведена руками) — берём её Id
update u set NewCompanyId = coalesce(nc.NewId, xc.Id)
from #user u
left join #company nc on nc.LegacyId = u.LegacyCompanyId
left join autocall.dbo.Company lc on lc.EntityId = u.LegacyCompanyId
left join dbo.Company xc on xc.Name = lc.CompanyName collate database_default and xc.IsDeleted = 0;

insert into dbo.Company (Id, Name, CreatedDateTime, CreatedById, ModifiedDateTime, ModifiedById, IsDeleted)
select NewId, left(CompanyName, 100), CreatedDate, @systemUserId, LastModifiedDate, @systemUserId, 0
from #company;

-------- 0.4 IdentityUsers + роли --------
insert into dbo.IdentityUsers (Id, CompanyId, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp,
    ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, TimeZoneId,
    CreatedDateTime, ModifiedDateTime, CreatedId, ModifiedId, IsDeleted)
select u.NewId, u.NewCompanyId, u.UserName, upper(u.UserName), u.Email, upper(u.Email), 1, u.PasswordHash, u.SecurityStamp,
    lower(convert(varchar(36), newid())), u.PhoneNumber, 0, 0, u.LockoutEndDateUtc, u.LockoutEnabled, 0, 'UTC',
    u.CreatedDate, u.LastModifiedDate, @systemUserId, @systemUserId, 0
from #user u;

insert into dbo.IdentityUserRoles (UserId, RoleId)
select distinct u.NewId, r.RoleId
from #user u join #role r on r.LegacyUserId = u.LegacyId;

-------- 0.5 маппинг для блоков 1–5 --------
-- компании, которые уже были в autodispatch, тоже попадают в маппинг (через #user.NewCompanyId)
insert into dbo._MigrateCompany (LegacyId, NewId)
select distinct u.LegacyCompanyId, u.NewCompanyId from #user u;

insert into dbo._MigrateUser (LegacyId, NewId, NewCompanyId)
select u.LegacyId, u.NewId, u.NewCompanyId from #user u;

-------- итог --------
select 'active companies (d_Event)' as [table], count(*) as [rows] from #active
union all select 'companies inserted', count(*) from #company
union all select 'users', count(*) from #user
union all select 'user roles', count(*) from dbo.IdentityUserRoles ur join #user u on u.NewId = ur.UserId
union all select 'admins', count(distinct u.NewId) from #user u join #role r on r.LegacyUserId = u.LegacyId where r.RoleId = 7
union all select 'owners', count(distinct u.NewId) from #user u join #role r on r.LegacyUserId = u.LegacyId where r.RoleId = 6;

-- пользователи активных компаний с ролью AutoDispatch, не попавшие в перенос (нет входа за полгода или email уже есть) — для контроля
select u.Email, u.CompanyId, u.IsDeleted,
       (select max(s.LastActivity) from autocall_reference.dbo.Session s where s.CreatedById = u.Id) as LastActivity
from autocall.dbo.Users u
where u.CompanyId in (select LegacyId from #active)
  and exists (select 1 from #role r where r.LegacyUserId = u.Id)
  and not exists (select 1 from #user x where x.LegacyId = u.Id)
order by u.CompanyId, u.Email;

commit tran;
-- rollback tran;  -- вместо commit для прогона «на сухую»
