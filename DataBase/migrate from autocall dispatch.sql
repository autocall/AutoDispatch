-- Перенос данных AutoDispatch из autocall (V1) -> autodispatch. Выполняется под sa на db.myautocall.net, блоками (AI/wiki/data-migration.md).
-- По образцу AutoCallV3/Repo/DataBase/migrate from autocall.sql. Таблицы autodispatch должны существовать (миграция EF применена сервером).
-- Сейчас здесь только блок 0 — компании и пользователи; блоки 1–5 (справочники, d_Load, d_HistoryObject, d_Event, файлы) добавляются позже.
--
-- Блок 0. Кого переносим:
--   * компании с событиями d_Event за последние 6 месяцев (на 2026-11-11 — 7 компаний);
--   * их не удалённые пользователи со статусом allowed (VerifyId = 2; waiting и blocked не переносятся — решено 2026-11-13),
--     у которых есть хотя бы одна роль с модулем AutoDispatch (Permissions.GetModulesByRole V1):
--     Agent, Warehouse, Dispatcher, Sales, Accounting, Owner, Admin — остальные роли V1 (User, Driver, Gps Support) не переносятся;
--   * и которые входили в систему за последние 6 месяцев (autocall_reference.dbo.Session, как в V3) — бывшие сотрудники без входа не нужны.
-- Роли переносятся все, какие есть у пользователя (в V1 RoleName — флаги, клиент считает сумму), Admin V1 остаётся Admin.
-- Пароли (PasswordHash Identity v2) и SecurityStamp копируются как есть — Identity Core проверяет старый формат и перехэширует при входе.
-- Ключи Guid сохраняются (решено 2026-11-13): Users.Id -> IdentityUsers.Id, Company.EntityId -> Company.Id, маппинга нет,
-- блоки 1–5 копируют CompanyId и CreatedById/LastModifiedById в d_* как есть.
-- UserName = Users.Name V1 (отображаемое имя в истории; пусто -> email); NormalizedUserName уникален, при дубле добавляется email.
-- Повторный запуск перезаливает блок целиком (шаг 0.0 удаляет перенесённых по Id из autocall); пользователи, заведённые в autodispatch
-- руками, остаются и по Email пропускаются.

use autodispatch;
set nocount on;
set xact_abort on;

declare @since datetime2(0) = dateadd(month, -6, sysutcdatetime());
declare @systemUserId uniqueidentifier = '00000000-0000-0000-0000-000000001000';

begin tran;

-------- 0.0 очистка прошлого переноса (служебные Company и пользователи System/Admin не задеты: их Id нет в autocall) --------
delete from dbo.UserSetting       where UserId in (select Id from autocall.dbo.Users);
delete from dbo.IdentityUserRoles where UserId in (select Id from autocall.dbo.Users);
delete from dbo.IdentityUsers     where Id     in (select Id from autocall.dbo.Users);
delete from dbo.Company           where Id     in (select EntityId from autocall.dbo.Company);

-------- 0.1 активные компании --------
select distinct e.CompanyId as Id
into #active
from autocall.dbo.d_Event e
where e.CreatedDate >= @since;

-------- 0.2 пользователи с ролями AutoDispatch --------
-- имена ролей V1 в таблице Roles совпадают с RoleName без пробелов ("Gps Support" -> GpsSupport);
-- Id роли в autodispatch = RoleEnum.ToRoleId(): 00000000-0000-0000-0000-00000000000N
select ur.UserId as LegacyUserId,
       cast(case replace(r.Name, ' ', '')
           when 'Agent'      then '00000000-0000-0000-0000-000000000001'
           when 'Warehouse'  then '00000000-0000-0000-0000-000000000002'
           when 'Dispatcher' then '00000000-0000-0000-0000-000000000003'
           when 'Sales'      then '00000000-0000-0000-0000-000000000004'
           when 'Accounting' then '00000000-0000-0000-0000-000000000005'
           when 'Owner'      then '00000000-0000-0000-0000-000000000006'
           when 'Admin'      then '00000000-0000-0000-0000-000000000007'
       end as uniqueidentifier) as RoleId
into #role
from autocall.dbo.UserRoles ur
join autocall.dbo.Roles r on r.Id = ur.RoleId
where replace(r.Name, ' ', '') in ('Agent', 'Warehouse', 'Dispatcher', 'Sales', 'Accounting', 'Owner', 'Admin');

select
    u.Id, u.CompanyId,
    coalesce(nullif(ltrim(rtrim(u.Name)), ''), u.Email) as UserName,
    u.Email, u.PasswordHash, u.SecurityStamp, u.PhoneNumber, u.LockoutEnabled, u.LockoutEndDateUtc,
    u.CreatedDate, u.LastModifiedDate
into #user
from autocall.dbo.Users u
where u.IsDeleted = 0
  and u.VerifyId = 2
  and u.CompanyId in (select Id from #active)
  and exists (select 1 from #role r where r.LegacyUserId = u.Id)
  and exists (select 1 from autocall_reference.dbo.Session s where s.CreatedById = u.Id and s.LastActivity >= @since)
  and not exists (select 1 from dbo.IdentityUsers x where x.NormalizedEmail = upper(u.Email) collate database_default);

-- одинаковые отображаемые имена: NormalizedUserName уникален — к повторам добавляется email
update u set UserName = u.UserName + ' (' + u.Email + ')'
from #user u
where exists (select 1 from #user x where upper(x.UserName) = upper(u.UserName) collate database_default and x.Id <> u.Id)
   or exists (select 1 from dbo.IdentityUsers x where x.NormalizedUserName = upper(u.UserName) collate database_default);

-------- 0.3 компании отобранных пользователей (удалённые восстанавливаются активными) --------
select c.EntityId as Id, c.CompanyName, c.CreatedDate, c.LastModifiedDate
into #company
from autocall.dbo.Company c
where c.EntityId in (select distinct CompanyId from #user);

insert into dbo.Company (Id, Name, CreatedDateTime, CreatedById, ModifiedDateTime, ModifiedById, IsDeleted)
select Id, left(CompanyName, 100), CreatedDate, @systemUserId, LastModifiedDate, @systemUserId, 0
from #company;

-------- 0.4 IdentityUsers + роли --------
insert into dbo.IdentityUsers (Id, CompanyId, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp,
    ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, TimeZoneId,
    CreatedDateTime, ModifiedDateTime, CreatedId, ModifiedId, IsDeleted)
select u.Id, u.CompanyId, u.UserName, upper(u.UserName), u.Email, upper(u.Email), 1, u.PasswordHash, u.SecurityStamp,
    lower(convert(varchar(36), newid())), u.PhoneNumber, 0, 0, u.LockoutEndDateUtc, u.LockoutEnabled, 0, 'UTC',
    u.CreatedDate, u.LastModifiedDate, @systemUserId, @systemUserId, 0
from #user u;

insert into dbo.IdentityUserRoles (UserId, RoleId)
select distinct u.Id, r.RoleId
from #user u join #role r on r.LegacyUserId = u.Id;

-------- итог --------
select 'active companies (d_Event)' as [table], count(*) as [rows] from #active
union all select 'companies inserted', count(*) from #company
union all select 'users', count(*) from #user
union all select 'user roles', count(*) from dbo.IdentityUserRoles ur join #user u on u.Id = ur.UserId
union all select 'admins', count(distinct u.Id) from #user u join #role r on r.LegacyUserId = u.Id where r.RoleId = '00000000-0000-0000-0000-000000000007'
union all select 'owners', count(distinct u.Id) from #user u join #role r on r.LegacyUserId = u.Id where r.RoleId = '00000000-0000-0000-0000-000000000006';

-- пользователи активных компаний с ролью AutoDispatch, не попавшие в перенос (нет входа за полгода, не allowed или email уже есть) — для контроля
select u.Email, u.CompanyId, u.IsDeleted, u.VerifyId,
       (select max(s.LastActivity) from autocall_reference.dbo.Session s where s.CreatedById = u.Id) as LastActivity
from autocall.dbo.Users u
where u.CompanyId in (select Id from #active)
  and exists (select 1 from #role r where r.LegacyUserId = u.Id)
  and not exists (select 1 from #user x where x.Id = u.Id)
order by u.CompanyId, u.Email;

commit tran;
-- rollback tran;  -- вместо commit для прогона «на сухую»
