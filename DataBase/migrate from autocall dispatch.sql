-- Перенос данных AutoDispatch из autocall (V1) -> autodispatch. Выполняется под sa на db.myautocall.net, блоками (AI/wiki/data-migration.md).
-- По образцу AutoCallV3/Repo/DataBase/migrate from autocall.sql. Таблицы autodispatch должны существовать (миграция EF применена сервером).
-- Блок 0 — компании и пользователи; блоки 1–5 — справочники и объекты, грузы, HistoryObject, Event, список файлов; блок 6 — проверка FK;
-- блок 7 — адреса обратного геокодинга из Cache (таблица GeocodeAddress, миграция пользователя после 2026-11-20).
--
-- Блок 0. Кого переносим:
--   * компании с событиями d_Event за 6 месяцев до первого переноса — с 2026-05-18 (7 компаний; дата зафиксирована для догона, см. @since);
--   * их не удалённые пользователи со статусом allowed (VerifyId = 2; waiting и blocked не переносятся — решено 2026-11-13),
--     у которых есть хотя бы одна роль с модулем AutoDispatch (Permissions.GetModulesByRole V1):
--     Agent, Warehouse, Dispatcher, Sales, Accounting, Owner, Admin — остальные роли V1 (User, Driver, Gps Support) не переносятся;
--   * и которые входили в систему за последний год (решено 2026-11-13): Users.LastModifiedDate (V1 обновляет его при работе из App: проверено по данным 2026-11-13,
--     у billing TORPEDO без веб-сессий — 2026-11-06) или autocall_reference.dbo.Session.LastActivity (вход на сайт);
--     одной Session мало — она пишется только сайтом (проверено 2026-11-13). Бывшие сотрудники без входа не нужны.
-- Роли переносятся все, какие есть у пользователя (в V1 RoleName — флаги, клиент считает сумму), Admin V1 остаётся Admin.
-- Пароли (PasswordHash Identity v2) и SecurityStamp копируются как есть — Identity Core проверяет старый формат и перехэширует при входе.
-- Ключи Guid сохраняются (решено 2026-11-13): Users.Id -> IdentityUsers.Id, Company.EntityId -> Company.Id, маппинга нет,
-- блоки 1–5 копируют CompanyId и CreatedById/LastModifiedById в d_* как есть.
-- UserName = Users.Name V1 (отображаемое имя в истории; пусто -> email); NormalizedUserName уникален, при дубле добавляется email.
-- Повторный запуск перезаливает блок целиком (шаг 0.0 удаляет перенесённых по Id из autocall); пользователи, заведённые в autodispatch
-- руками, остаются и по Email пропускаются.
--
-- Догон (перед сверкой этапа 7 и в окно переключения этапа 8 — AI/wiki/data-migration.md «Догон»): весь скрипт ещё раз в одной сессии под sa.
-- Блоки 0–2 перезаливаются целиком (Company — только новые: на неё ссылаются FK данных), блок 3 дополняет _MigrateHistoryKeep
-- и докачивает снимки (новые и пропущенные ниже max(Id)), блок 4 — события после max(Id), блок 5 — список каталогов для досинка файлов,
-- блок 6 — FK. Если в autodispatch уже писал новый сервер (проверка App этапа 7), его Event и HistoryObject занимают Id из того же диапазона,
-- что новые записи V1, — перед блоком 1 удалить их (шаг «записи нового сервера» ниже) или переносить заново в пустую БД.

use autodispatch;
set nocount on;
set xact_abort on;

-- окно активности компаний зафиксировано датой первого переноса 2026-11-18 минус 6 месяцев: при догоне набор компаний не меняется
-- (UA LOGISTICS — последнее событие 2026-06-08 — со скользящим окном выпала бы 2026-12-08, а её снимки и события остались бы без Company)
declare @since datetime2(0) = '2026-05-18';
declare @sinceLogin datetime2(0) = dateadd(month, -12, sysutcdatetime());   -- окно входа пользователей шире окна активности компаний

-- активные компании — одна временная таблица на весь скрипт (второй select into #active в том же batch не компилируется)
if object_id('tempdb..#active') is not null drop table #active;
create table #active (Id uniqueidentifier not null primary key);
insert into #active (Id)
select distinct e.CompanyId from autocall.dbo.d_Event e where e.CreatedDate >= @since;
declare @systemUserId uniqueidentifier = '00000000-0000-0000-0000-000000001000';

begin tran;

-------- 0.0 очистка прошлого переноса (служебные Company и пользователи System/Admin не задеты: их Id нет в autocall) --------
-- Company не удаляется: после блока 6 на неё ссылаются FK данных (догон); новые компании добавляет шаг 0.3
delete from dbo.UserSetting        where UserId in (select Id from autocall.dbo.Users);
delete from dbo.IdentityUserRoles  where UserId in (select Id from autocall.dbo.Users);
delete from dbo.IdentityUserClaims where UserId in (select Id from autocall.dbo.Users);
delete from dbo.IdentityUserLogins where UserId in (select Id from autocall.dbo.Users);
delete from dbo.IdentityUserToken  where UserId in (select Id from autocall.dbo.Users);
delete from dbo.IdentityUsers     where Id     in (select Id from autocall.dbo.Users);

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
    u.CreatedDate, u.LastModifiedDate,
    left(nullif(ltrim(rtrim(u.MicroSIPLogin)), ''), 10) as SipLogin, left(nullif(u.MicroSIPPassword, ''), 50) as SipPassword, -- софтфон App (решено 2026-11-19, AI/wiki/plan.md «Софтфоны в App»)
    left(nullif(ltrim(rtrim(u.Signature)), ''), 64) as Signature -- подпись агента в confirmed-письме (AdUser.Signature varchar(64); в V1 nvarchar(max), заполнено у 10, max 13 символов)
into #user
from autocall.dbo.Users u
where u.IsDeleted = 0
  and u.VerifyId = 2
  and u.CompanyId in (select Id from #active)
  and exists (select 1 from #role r where r.LegacyUserId = u.Id)
  and (u.LastModifiedDate >= @sinceLogin or exists (select 1 from autocall_reference.dbo.Session s where s.CreatedById = u.Id and s.LastActivity >= @sinceLogin))
  and not exists (select 1 from dbo.IdentityUsers x where x.NormalizedEmail = upper(u.Email) collate database_default);

-- одинаковые отображаемые имена: NormalizedUserName уникален — к повторам добавляется email
update u set UserName = u.UserName + ' (' + u.Email + ')'
from #user u
where exists (select 1 from #user x where upper(x.UserName) = upper(u.UserName) collate database_default and x.Id <> u.Id)
   or exists (select 1 from dbo.IdentityUsers x where x.NormalizedUserName = upper(u.UserName) collate database_default);

-------- 0.3 все активные компании, в том числе без переносимых пользователей (удалённые восстанавливаются активными) --------
-- решено 2026-11-18: данные блоков 1–4 идут по #active, без строки Company не включатся FK CompanyId (UA LOGISTICS — 0 пользователей allowed)
select c.EntityId as Id, c.CompanyName, c.CreatedDate, c.LastModifiedDate
into #company
from autocall.dbo.Company c
where c.EntityId in (select Id from #active);

insert into dbo.Company (Id, Name, CreatedDateTime, CreatedById, ModifiedDateTime, ModifiedById, IsDeleted)
select Id, left(CompanyName, 100), CreatedDate, @systemUserId, LastModifiedDate, @systemUserId, 0
from #company c
where not exists (select 1 from dbo.Company x where x.Id = c.Id);

-------- 0.4 IdentityUsers + роли --------
insert into dbo.IdentityUsers (Id, CompanyId, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp,
    ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, TimeZoneId,
    CreatedDateTime, ModifiedDateTime, CreatedId, ModifiedId, IsDeleted, Signature, SipLogin, SipPassword)
select u.Id, u.CompanyId, u.UserName, upper(u.UserName), u.Email, upper(u.Email), 1, u.PasswordHash, u.SecurityStamp,
    lower(convert(varchar(36), newid())), u.PhoneNumber, 0, 0, u.LockoutEndDateUtc, u.LockoutEnabled, 0, 'UTC',
    u.CreatedDate, u.LastModifiedDate, @systemUserId, @systemUserId, 0, u.Signature, u.SipLogin, u.SipPassword
from #user u;

insert into dbo.IdentityUserRoles (UserId, RoleId)
select distinct u.Id, r.RoleId
from #user u join #role r on r.LegacyUserId = u.Id;

-------- 0.5 настройки App: секции UserSetting, только dispatch (решено 2026-11-17, AI/wiki/user-settings.md) --------
-- Settings.Binary V1 = gzip JSON (Json.NET, $type) словаря «свойство UserSettings -> значение»; Binary как есть не переносится —
-- раскладывается по секциям, как в AutoCall V3: строка на свойство, Section = имя свойства. Свойства AutoCall (DataSourceSettings,
-- Sound/HotKey/Connection/Zoom, BoardsSettings, Default*PatternSettings, PatternTabs, PatternsDialogGridView, ViewType, Volume*)
-- не переносятся; в BoardsSettings2 остаются только борды Dispatch*. byte[] (раскладки гридов Telerik, DockingLayout, Selected*Company)
-- — строка base64 без обёртки {"$type":"System.Byte[], mscorlib","$value":…}; остальные объекты — JSON Json.NET как есть (с $type).
-- CallServiceSettings (софтфон) переносится с 2026-11-19 (у 13 из 17 пользователей, AI/wiki/plan.md «Софтфоны в App»).
-- null-свойства не пишутся (секции нет -> App берёт дефолт). Проверено select'ом 2026-11-17: 17 пользователей, 201 строка, весь JSON валиден.
insert into dbo.UserSetting (UserId, Section, Json, ModifiedDateTime)
select u.Id, j.[key],
    case
        when j.[key] = 'BoardsSettings2' then
            (select '{' + string_agg(cast('"' + b.[key] + '":' + b.value as nvarchar(max)), ',') + '}' from openjson(j.value) b where b.[key] like 'Dispatch%')
        when j.value like '{"$type":"System.Byte`[`], mscorlib","$value":"%' escape '`' then '"' + substring(j.value, 46, len(j.value) - 46)
        when j.type = 1 then '"' + string_escape(j.value, 'json') + '"'
        else j.value
    end,
    sysutcdatetime()
from #user u
join autocall.dbo.Settings s on s.UserId = u.Id
cross apply openjson(cast(decompress(s.Binary) as nvarchar(max))) j
where j.type <> 0
  and j.[key] in ('BoardsSettings2', 'DockingLayout', 'FiltersColumns', 'DefaultLoadSettings', 'DatCompaniesDialogGridView',
      'EquipmentsDialogGridViewDrivers', 'EquipmentsDialogGridViewTrucks', 'EquipmentsDialogGridViewTrailers',
      'EquipmentsDialogGridViewWarehouses', 'EquipmentsDialogGridViewYards',
      'SelectedCarrierCompany', 'SelectedBrokerCompany', 'ScanEmail', 'ScanBW', 'ScanADF', 'SearchTrailerWarehouseMode', 'CallServiceSettings');

-------- итог --------
select 'active companies (d_Event)' as [table], count(*) as [rows] from #active
union all select 'companies inserted', count(*) from #company
union all select 'users', count(*) from #user
union all select 'user roles', count(*) from dbo.IdentityUserRoles ur join #user u on u.Id = ur.UserId
union all select 'user settings', count(*) from dbo.UserSetting us join #user u on u.Id = us.UserId
union all select 'admins', count(distinct u.Id) from #user u join #role r on r.LegacyUserId = u.Id where r.RoleId = '00000000-0000-0000-0000-000000000007'
union all select 'owners', count(distinct u.Id) from #user u join #role r on r.LegacyUserId = u.Id where r.RoleId = '00000000-0000-0000-0000-000000000006';

-- пользователи активных компаний с ролью AutoDispatch, не попавшие в перенос (нет входа за год, не allowed или email уже есть) — для контроля
select u.Email, u.CompanyId, u.IsDeleted, u.VerifyId, u.LastModifiedDate as AppLogin,
       (select max(s.LastActivity) from autocall_reference.dbo.Session s where s.CreatedById = u.Id) as WebLogin
from autocall.dbo.Users u
where u.CompanyId in (select Id from #active)
  and exists (select 1 from #role r where r.LegacyUserId = u.Id)
  and not exists (select 1 from #user x where x.Id = u.Id)
order by u.CompanyId, u.Email;

commit tran;
-- rollback tran;  -- вместо commit для прогона «на сухую»

-- ======================================================================================================================
-- Блоки 1–5 (написаны 2026-11-13, прогнаны 2026-11-18 — пробный перенос в autodispatch на db.myautocall.net). Схема приёмника — AI/wiki/data-model.md:
--   таблицы без префикса d_, d_ActualCompany -> DispatchCompany, ActualCompanyId -> DispatchCompanyId,
--   geography -> пары decimal (OriginLocation.Lat/.Long -> OriginLatitude/OriginLongitude и т.д.).
-- Ключи Guid и int identity сохраняются. FK на время заливки отключаются (блоки идут не в порядке зависимостей:
-- HistoryId объектов ссылается на HistoryObject из блока 3) и включаются с проверкой в блоке 6.
-- Блоки 1–2 — одной транзакцией каждый, повторный запуск перезаливает блок; блоки 3–4 — пакетами, возобновляемы с max(Id).
-- Отбор активных компаний (#active) создаётся в шапке скрипта; блоки 1–6 без неё не запускаются (проверка ниже).
-- ======================================================================================================================

if object_id('tempdb..#active') is null begin
    raiserror ('#active отсутствует: выполните шапку скрипта (declare + create table #active) в этой же сессии', 16, 1);
    return;
end;

exec sp_MSforeachtable 'alter table ? nocheck constraint all';

-------- записи нового сервера — только для догона после проверки App этапа 7 (AI/wiki/data-migration.md «Догон») --------
-- Блоки 1–2 и так перезаливаются целиком; Event и HistoryObject — нет, а новый сервер нумерует их после max(Id) прошлого прогона.
-- Подставить maxId из итогов блоков 3 и 4 прошлого прогона (записаны в data-migration.md) и раскомментировать:
-- delete from dbo.Event         where Id > /* Event maxId */ 0;
-- delete from dbo.HistoryObject where Id > /* HistoryObject maxId */ 0;

-------- 1. справочники и объекты компаний --------
begin tran;

delete from dbo.Driver;                     delete from dbo.Truck;                      delete from dbo.Trailer;
delete from dbo.DatDriver;                  delete from dbo.DatCompany;
delete from dbo.SetupPacketReference;       delete from dbo.SetupPacketInsuranceAgency; delete from dbo.SetupPacketContact;
delete from dbo.SetupPacketFactoringCompany; delete from dbo.SetupPacket;
delete from dbo.Warehouse;                  delete from dbo.Yard;
delete from dbo.Defect;                     delete from dbo.DefectRepair;
delete from dbo.ShipperReceiverCompany;     delete from dbo.CustomerCompany;            delete from dbo.CompanyCustomStatus;
delete from dbo.DispatchCompanyCustom;      delete from dbo.DispatchCompany;

-- setup packets активных компаний: по ним отбираются контакты, страховые агентства и рекомендации
select EntityId, ConfidantContactId, CorporateContactId, SalesContactId, DispatchContactId, AfterHourContactId,
       GeneralLiabilityInsuranceAgencyId, AutoInsuranceAgencyId, CargoInsuranceAgencyId, WorkersCompensationInsuranceAgencyId
into #sp
from autocall.dbo.d_SetupPacket
where CompanyId in (select Id from #active);

-- d_ActualCompany -> DispatchCompany
insert into dbo.DispatchCompany ([EntityId], [LogoSmall], [LogoBig], [McNumber], [CompanyId], [LegalName], [PhysicalAddress], [MailingAddress], [ShippersPhone], [CellPhone], [WorkPhone], [WorkFax], [WorkEmail], [PaymentInstructions], [SMTP_Email], [SMTP_Password], [SMTP_Host], [SMTP_Port], [SMS_Phone], [ConfirmationFile], [BOLFile], [InvoiceFile], [BnsfLogin], [BnsfPassword], [BnsfLastMessage], [BnsfLastDone], [BnsfCookies], [SubmitRates], [SMTP_SchemeEmail], [SMTP_SchemePassword], [SMTP_SchemeHost], [SMTP_SchemePort], [BoardType], [NextConfirmationNumber], [NextInvoiceNumber], [NextLoadNumber], [InvoiceNumberAsLoadNumber], [NextLoadExtra], [CarrierViewToken], [FactoringCompanyJson], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted])
select s.[EntityId], s.[LogoSmall], s.[LogoBig], s.[McNumber], s.[CompanyId], s.[LegalName], s.[PhysicalAddress], s.[MailingAddress], s.[ShippersPhone], s.[CellPhone], s.[WorkPhone], s.[WorkFax], s.[WorkEmail], s.[PaymentInstructions], s.[SMTP_Email], s.[SMTP_Password], s.[SMTP_Host], s.[SMTP_Port], s.[SMS_Phone], s.[ConfirmationFile], s.[BOLFile], s.[InvoiceFile], s.[BnsfLogin], s.[BnsfPassword], s.[BnsfLastMessage], s.[BnsfLastDone], s.[BnsfCookies], s.[SubmitRates], s.[SMTP_SchemeEmail], s.[SMTP_SchemePassword], s.[SMTP_SchemeHost], s.[SMTP_SchemePort], s.[BoardType], s.[NextConfirmationNumber], s.[NextInvoiceNumber], s.[NextLoadNumber], s.[InvoiceNumberAsLoadNumber], s.[NextLoadExtra], s.[CarrierViewToken], s.[FactoringCompanyJson], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted]
from autocall.dbo.d_ActualCompany s
where s.CompanyId in (select Id from #active);

-- d_ActualCompanyCustom -> DispatchCompanyCustom
insert into dbo.DispatchCompanyCustom ([EntityId], [DispatchCompanyId], [Name], [Type], [Params], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted])
select s.[EntityId], s.[ActualCompanyId], s.[Name], s.[Type], s.[Params], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted]
from autocall.dbo.d_ActualCompanyCustom s
where s.ActualCompanyId in (select EntityId from autocall.dbo.d_ActualCompany where CompanyId in (select Id from #active));

-- d_CompanyCustomStatus -> CompanyCustomStatus
insert into dbo.CompanyCustomStatus ([EntityId], [Name], [CompanyId], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted])
select s.[EntityId], s.[Name], s.[CompanyId], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted]
from autocall.dbo.d_CompanyCustomStatus s
where s.CompanyId in (select Id from #active);

-- d_CustomerCompany -> CustomerCompany
insert into dbo.CustomerCompany ([EntityId], [Name], [Contacts], [CompanyId], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted])
select s.[EntityId], s.[Name], s.[Contacts], s.[CompanyId], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted]
from autocall.dbo.d_CustomerCompany s
where s.CompanyId in (select Id from #active);

-- d_ShipperReceiverCompany -> ShipperReceiverCompany
insert into dbo.ShipperReceiverCompany ([EntityId], [Name], [Contacts], [CompanyId], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted])
select s.[EntityId], s.[Name], s.[Contacts], s.[CompanyId], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted]
from autocall.dbo.d_ShipperReceiverCompany s
where s.CompanyId in (select Id from #active);

-- d_DefectRepair -> DefectRepair
insert into dbo.DefectRepair ([EntityId], [CompanyId], [Date], [Cost], [Comment], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted])
select s.[EntityId], s.[CompanyId], s.[Date], s.[Cost], s.[Comment], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted]
from autocall.dbo.d_DefectRepair s
where s.CompanyId in (select Id from #active);

-- d_Defect -> Defect
insert into dbo.Defect ([EntityId], [CompanyId], [TrailerId], [TruckId], [RepairId], [DefectId], [Description], [ServiceNeeded], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted])
select s.[EntityId], s.[CompanyId], s.[TrailerId], s.[TruckId], s.[RepairId], s.[DefectId], s.[Description], s.[ServiceNeeded], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted]
from autocall.dbo.d_Defect s
where s.CompanyId in (select Id from #active);

-- d_Yard -> Yard
insert into dbo.Yard ([EntityId], [Name], [Address], [City], [Latitude], [Longitude], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted], [HistoryId], [CompanyId])
select s.[EntityId], s.[Name], s.[Address], s.[City], s.Location.Lat, s.Location.Long, s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted], s.[HistoryId], s.[CompanyId]
from autocall.dbo.d_Yard s
where s.CompanyId in (select Id from #active);

-- d_Warehouse -> Warehouse
insert into dbo.Warehouse ([EntityId], [Name], [Address], [City], [Latitude], [Longitude], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted], [HistoryId], [CompanyId])
select s.[EntityId], s.[Name], s.[Address], s.[City], s.Location.Lat, s.Location.Long, s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted], s.[HistoryId], s.[CompanyId]
from autocall.dbo.d_Warehouse s
where s.CompanyId in (select Id from #active);

-- d_SetupPacket -> SetupPacket
insert into dbo.SetupPacket ([EntityId], [Version], [LegalName], [DBAName], [Step], [PhysicalAddress], [PhysicalCity], [PhysicalStateId], [PhysicalZipCode], [PhysicalCountryId], [MailingCompanyLegalName], [MailingAddress], [MailingCity], [MailingStateId], [MailingZipCode], [MailingCountryId], [MailingEmail], [DunsNumber], [UseFactoringCompany], [FactoringCompanyId], [ConfidantContactId], [CorporateContactId], [SalesContactId], [DispatchContactId], [AfterHourContactId], [SalesSameAs], [DispatchSameAs], [AfterHourSameAs], [CVORNumber], [NIRNumber], [NSCNumber], [CountryServiceType], [AuthorizedToHaulElectronics], [HasReferBreakdown], [HasNonOwnedAuto], [HasActiveShipmentAtTimeOfReg], [AllowAcquirePolicyExclusions], [AgreementAgree], [AgreementDate], [Agreement_02Signature], [Agreement_02SignatureDate], [Agreement_02Date], [Agreement2_02Signature], [AutoSameAs], [CargoSameAs], [WorkersCompensationSameAs], [GeneralLiabilityInsuranceAgencyId], [AutoInsuranceAgencyId], [CargoInsuranceAgencyId], [WorkersCompensationInsuranceAgencyId], [WorkersCompensationSignature], [WorkersCompensationSignatureDate], [W9Name], [W9BusinessName], [W9CompanyType], [W9TaxClassification], [W9OtherTaxClassification], [W9ExemptPayeeCode], [W9FATCACode], [W9Address], [W9City], [W9StateId], [W9ZipCode], [W9NamePerson], [W9AccountNumber], [W9SocialSecurityNumber], [W9EmployerIdentificationNumber], [W9Agree], [W9AgreeDate], [W9Signature], [Step8Modes], [Step8Commodities], [TractorsCount], [SCACCode], [DriversCount], [TeamsCount], [OperatorsCount], [OwnerName], [OwnerPhone], [OwnerEmail], [HasMexico], [HasCanadian], [HasEmailCarrier], [CertificationSmartWay], [CertificationC_TPAT], [CertificationFAST], [CertificationTWIC], [CertificationCARB], [CertificationHazMatCertification], [CertificationSafetyPermit], [SafetyAgree], [LoadUrgency], [NumberOfUnits], [W8States], [Diversity1M_WBE], [Diversity1], [Diversity2], [Diversity2Other], [Diversity3Certified], [Diversity3], [Diversity3Other], [FileMC], [FileCOI], [EmailSent], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted], [HistoryId], [CompanyId])
select s.[EntityId], s.[Version], s.[LegalName], s.[DBAName], s.[Step], s.[PhysicalAddress], s.[PhysicalCity], s.[PhysicalStateId], s.[PhysicalZipCode], s.[PhysicalCountryId], s.[MailingCompanyLegalName], s.[MailingAddress], s.[MailingCity], s.[MailingStateId], s.[MailingZipCode], s.[MailingCountryId], s.[MailingEmail], s.[DunsNumber], s.[UseFactoringCompany], s.[FactoringCompanyId], s.[ConfidantContactId], s.[CorporateContactId], s.[SalesContactId], s.[DispatchContactId], s.[AfterHourContactId], s.[SalesSameAs], s.[DispatchSameAs], s.[AfterHourSameAs], s.[CVORNumber], s.[NIRNumber], s.[NSCNumber], s.[CountryServiceType], s.[AuthorizedToHaulElectronics], s.[HasReferBreakdown], s.[HasNonOwnedAuto], s.[HasActiveShipmentAtTimeOfReg], s.[AllowAcquirePolicyExclusions], s.[AgreementAgree], s.[AgreementDate], s.[Agreement_02Signature], s.[Agreement_02SignatureDate], s.[Agreement_02Date], s.[Agreement2_02Signature], s.[AutoSameAs], s.[CargoSameAs], s.[WorkersCompensationSameAs], s.[GeneralLiabilityInsuranceAgencyId], s.[AutoInsuranceAgencyId], s.[CargoInsuranceAgencyId], s.[WorkersCompensationInsuranceAgencyId], s.[WorkersCompensationSignature], s.[WorkersCompensationSignatureDate], s.[W9Name], s.[W9BusinessName], s.[W9CompanyType], s.[W9TaxClassification], s.[W9OtherTaxClassification], s.[W9ExemptPayeeCode], s.[W9FATCACode], s.[W9Address], s.[W9City], s.[W9StateId], s.[W9ZipCode], s.[W9NamePerson], s.[W9AccountNumber], s.[W9SocialSecurityNumber], s.[W9EmployerIdentificationNumber], s.[W9Agree], s.[W9AgreeDate], s.[W9Signature], s.[Step8Modes], s.[Step8Commodities], s.[TractorsCount], s.[SCACCode], s.[DriversCount], s.[TeamsCount], s.[OperatorsCount], s.[OwnerName], s.[OwnerPhone], s.[OwnerEmail], s.[HasMexico], s.[HasCanadian], s.[HasEmailCarrier], s.[CertificationSmartWay], s.[CertificationC_TPAT], s.[CertificationFAST], s.[CertificationTWIC], s.[CertificationCARB], s.[CertificationHazMatCertification], s.[CertificationSafetyPermit], s.[SafetyAgree], s.[LoadUrgency], s.[NumberOfUnits], s.[W8States], s.[Diversity1M_WBE], s.[Diversity1], s.[Diversity2], s.[Diversity2Other], s.[Diversity3Certified], s.[Diversity3], s.[Diversity3Other], s.[FileMC], s.[FileCOI], s.[EmailSent], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted], s.[HistoryId], s.[CompanyId]
from autocall.dbo.d_SetupPacket s
where s.CompanyId in (select Id from #active);

-- d_SetupPacketContact -> SetupPacketContact
insert into dbo.SetupPacketContact ([EntityId], [Contact], [Phone], [Email], [Title], [Cell], [Fax])
select s.[EntityId], s.[Contact], s.[Phone], s.[Email], s.[Title], s.[Cell], s.[Fax]
from autocall.dbo.d_SetupPacketContact s
where s.EntityId in (select ConfidantContactId from #sp union select CorporateContactId from #sp union select SalesContactId from #sp union select DispatchContactId from #sp union select AfterHourContactId from #sp);

-- d_SetupPacketInsuranceAgency -> SetupPacketInsuranceAgency
insert into dbo.SetupPacketInsuranceAgency ([EntityId], [CompanyName], [Phone], [Fax], [Email], [Number])
select s.[EntityId], s.[CompanyName], s.[Phone], s.[Fax], s.[Email], s.[Number]
from autocall.dbo.d_SetupPacketInsuranceAgency s
where s.EntityId in (select GeneralLiabilityInsuranceAgencyId from #sp union select AutoInsuranceAgencyId from #sp union select CargoInsuranceAgencyId from #sp union select WorkersCompensationInsuranceAgencyId from #sp);

-- d_SetupPacketReference -> SetupPacketReference
insert into dbo.SetupPacketReference ([EntityId], [CreatedDate], [Contact], [Company], [Phone], [Email], [SetupPacketId])
select s.[EntityId], s.[CreatedDate], s.[Contact], s.[Company], s.[Phone], s.[Email], s.[SetupPacketId]
from autocall.dbo.d_SetupPacketReference s
where s.SetupPacketId in (select EntityId from #sp);

-- d_SetupPacketFactoringCompany -> SetupPacketFactoringCompany
insert into dbo.SetupPacketFactoringCompany ([EntityId], [CreatedDate], [Address], [Name], [City], [ZipCode], [Email], [StateId], [CountryId])
select s.[EntityId], s.[CreatedDate], s.[Address], s.[Name], s.[City], s.[ZipCode], s.[Email], s.[StateId], s.[CountryId]
from autocall.dbo.d_SetupPacketFactoringCompany s
where 1 = 1;

-- d_DatCompany -> DatCompany
insert into dbo.DatCompany ([EntityId], [DispatchCompanyId], [McNumber], [DotNumber], [Name], [EntityType], [Contacts], [Phy_UpsStore], [Mai_UpsStore], [Phy_MailBox], [Mai_MailBox], [InsuranceExpiration], [InsuranceFile], [Comments], [Rating], [HyperlinkFile], [DoNotUse], [SetupPacketId], [SetupPacketEmailSent], [SetupPacketSent], [SetupPacketCompleted], [SetupPacketAccept], [SetupPacketFile], [W9File], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted], [HistoryId], [CompanyId])
select s.[EntityId], s.[ActualCompanyId], s.[McNumber], s.[DotNumber], s.[Name], s.[EntityType], s.[Contacts], s.[Phy_UpsStore], s.[Mai_UpsStore], s.[Phy_MailBox], s.[Mai_MailBox], s.[InsuranceExpiration], s.[InsuranceFile], s.[Comments], s.[Rating], s.[HyperlinkFile], s.[DoNotUse], s.[SetupPacketId], s.[SetupPacketEmailSent], s.[SetupPacketSent], s.[SetupPacketCompleted], s.[SetupPacketAccept], s.[SetupPacketFile], s.[W9File], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted], s.[HistoryId], s.[CompanyId]
from autocall.dbo.d_DatCompany s
where s.CompanyId in (select Id from #active);

-- d_DatDriver -> DatDriver
insert into dbo.DatDriver ([EntityId], [UserId], [Name], [TruckNumber], [TrailerNumber], [ContactPhone], [Comments], [CarrierViewId], [CarrierViewLoadId], [CarrierViewLink], [CarrierViewLastUpdatedDate], [CarrierViewStatus], [CarrierViewStatusesJson], [CarrierViewLate], [CarrierViewPickupId], [CarrierViewDeliveryId], [CarrierViewPickupComment], [CarrierViewDeliveryComment], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted], [HistoryId], [CompanyId], [CurrentLatitude], [CurrentLongitude])
select s.[EntityId], s.[UserId], s.[Name], s.[TruckNumber], s.[TrailerNumber], s.[ContactPhone], s.[Comments], s.[CarrierViewId], s.[CarrierViewLoadId], s.[CarrierViewLink], s.[CarrierViewLastUpdatedDate], s.[CarrierViewStatus], s.[CarrierViewStatusesJson], s.[CarrierViewLate], s.[CarrierViewPickupId], s.[CarrierViewDeliveryId], s.[CarrierViewPickupComment], s.[CarrierViewDeliveryComment], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted], s.[HistoryId], s.[CompanyId], s.CurrentLocation.Lat, s.CurrentLocation.Long
from autocall.dbo.d_DatDriver s
where s.CompanyId in (select Id from #active);

-- d_Trailer -> Trailer
insert into dbo.Trailer ([EntityId], [Number], [Type], [AnnualInspection_File], [AnnualInspection_Expiration], [Registration_File], [Maintenance_File], [Maintenance_Expiration], [SchemeFile], [OriginSchemeFile], [SchemeDate], [BnsfOwner], [BnsfShipper], [BnsfUnit], [BnsfLoadEmptyStatus], [BnsfCurrentLocation], [BnsfEventCode], [BnsfEventDate], [BnsfETA_Deramp], [BnsfOrigin], [BnsfDestination], [BnsfBeyondGoalHours], [BnsfTimeDeramp], [BnsfTimeNotify], [BnsfFinalTrainArrival], [BnsfGoalDate], [BnsfNextevent], [BnsfPickUpNumber], [BnsfStorageDay], [BnsfTrain], [BnsfTrucker], [BnsfWaybillDate], [BnsfRateAuthority], [BnsfAmountDue], [BnsfDueDate], [BnsfWaybillNumber], [BnsfBillNumber], [BnsfAdjustments], [History], [YardId], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted], [HistoryId], [CompanyId], [CurrentLatitude], [CurrentLongitude])
select s.[EntityId], s.[Number], s.[Type], s.[AnnualInspection_File], s.[AnnualInspection_Expiration], s.[Registration_File], s.[Maintenance_File], s.[Maintenance_Expiration], s.[SchemeFile], s.[OriginSchemeFile], s.[SchemeDate], s.[BnsfOwner], s.[BnsfShipper], s.[BnsfUnit], s.[BnsfLoadEmptyStatus], s.[BnsfCurrentLocation], s.[BnsfEventCode], s.[BnsfEventDate], s.[BnsfETA_Deramp], s.[BnsfOrigin], s.[BnsfDestination], s.[BnsfBeyondGoalHours], s.[BnsfTimeDeramp], s.[BnsfTimeNotify], s.[BnsfFinalTrainArrival], s.[BnsfGoalDate], s.[BnsfNextevent], s.[BnsfPickUpNumber], s.[BnsfStorageDay], s.[BnsfTrain], s.[BnsfTrucker], s.[BnsfWaybillDate], s.[BnsfRateAuthority], s.[BnsfAmountDue], s.[BnsfDueDate], s.[BnsfWaybillNumber], s.[BnsfBillNumber], s.[BnsfAdjustments], s.[History], s.[YardId], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted], s.[HistoryId], s.[CompanyId], s.CurrentLocation.Lat, s.CurrentLocation.Long
from autocall.dbo.d_Trailer s
where s.CompanyId in (select Id from #active);

-- d_Truck -> Truck
insert into dbo.Truck ([EntityId], [Number], [TrailerId], [AnnualInspection_File], [AnnualInspection_Expiration], [Registration_File], [Registration_Expiration], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted], [HistoryId], [CompanyId], [CurrentLatitude], [CurrentLongitude])
select s.[EntityId], s.[Number], s.[TrailerId], s.[AnnualInspection_File], s.[AnnualInspection_Expiration], s.[Registration_File], s.[Registration_Expiration], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted], s.[HistoryId], s.[CompanyId], s.CurrentLocation.Lat, s.CurrentLocation.Long
from autocall.dbo.d_Truck s
where s.CompanyId in (select Id from #active);

-- d_Driver -> Driver
insert into dbo.Driver ([EntityId], [TruckId], [CurrentLoadId], [DriverStatus], [CarrierViewId], [CarrierViewLastUpdatedDate], [Name], [Email], [Phone], [THE_APPLICATION_OF_EMPLOYMENT_File], [MVR_File], [MEDICAL_CARD_File], [CDL_File], [DOT_INSPECTIONS_File], [BACKGROUND_File], [DRUG_TEST_File], [RANDOM_DRUG_TEST_File], [ACH_File], [W9_File], [MVR_Expiration], [MEDICAL_CARD_Expiration], [CDL_Expiration], [ACH_Expiration], [W9_Expiration], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted], [HistoryId], [CompanyId], [CurrentLatitude], [CurrentLongitude])
select s.[EntityId], s.[TruckId], s.[CurrentLoadId], s.[DriverStatus], s.[CarrierViewId], s.[CarrierViewLastUpdatedDate], s.[Name], s.[Email], s.[Phone], s.[THE_APPLICATION_OF_EMPLOYMENT_File], s.[MVR_File], s.[MEDICAL_CARD_File], s.[CDL_File], s.[DOT_INSPECTIONS_File], s.[BACKGROUND_File], s.[DRUG_TEST_File], s.[RANDOM_DRUG_TEST_File], s.[ACH_File], s.[W9_File], s.[MVR_Expiration], s.[MEDICAL_CARD_Expiration], s.[CDL_Expiration], s.[ACH_Expiration], s.[W9_Expiration], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted], s.[HistoryId], s.[CompanyId], s.CurrentLocation.Lat, s.CurrentLocation.Long
from autocall.dbo.d_Driver s
where s.CompanyId in (select Id from #active);

drop table #sp;

select 'DispatchCompany' as [table], count(*) as [rows] from dbo.DispatchCompany
union all select 'DatCompany', count(*) from dbo.DatCompany
union all select 'DatDriver', count(*) from dbo.DatDriver
union all select 'Driver', count(*) from dbo.Driver
union all select 'Truck', count(*) from dbo.Truck
union all select 'Trailer', count(*) from dbo.Trailer
union all select 'Warehouse', count(*) from dbo.Warehouse
union all select 'SetupPacket', count(*) from dbo.SetupPacket
union all select 'SetupPacketFactoringCompany', count(*) from dbo.SetupPacketFactoringCompany;

commit tran;

-------- 2. грузы и история положений --------
begin tran;

delete from dbo.HistoryLocationLoad;        delete from dbo.HistoryLocationDriver;      delete from dbo.HistoryLocationTruck;
delete from dbo.HistoryLocationTrailer;     delete from dbo.HistoryLocationDatDriver;
delete from dbo.Load;

-- d_Load -> Load
insert into dbo.Load ([EntityId], [Index], [IndexExtra], [IndexPrefix], [DispatchCompanyId], [TrackerId], [CarrierViewPhone], [CarrierViewLink], [CarrierViewLastUpdatedDate], [CarrierViewStatus], [CarrierViewStatusesJson], [CarrierViewLate], [CarrierViewPickupId], [CarrierViewDeliveryId], [CarrierViewPickupComment], [CarrierViewDeliveryComment], [RecordId], [BOLNumber], [DestinationWarehouseId], [WarehouseId], [TrailerId], [DatCompanyId], [DatDriverId], [HistoryDriverId], [HistoryTruckId], [HistoryTrailerId], [HistoryAssignedTrailerId], [HistoryWarehouseId], [HistoryDatCompanyId], [HistoryDatDriverId], [OriginCity], [DestinationCity], [CurrentOriginUtcOffset], [CurrentDestinationUtcOffset], [OriginAddress], [DestinationAddress], [OriginLatitude], [OriginLongitude], [DestinationLatitude], [DestinationLongitude], [Distance], [Comments], [Comments2], [Consolidation], [Equipment], [PalletType], [Weight], [Length], [Pallets], [Spots], [WeightType], [Commodity], [PickupFrom], [PickupTo], [DeliveryFrom], [DeliveryTo], [ReceiverId], [ShipperId], [PickupNumber], [DeliveryNumber], [BookedDate], [BoardChanged], [CarrierViewTracking], [LineHaul], [DatRate], [Expenses], [Gains], [Currency], [QuikPay], [LoadType], [ShowPickupAddress], [Background], [Foreground], [DivideType], [ContainerType], [ParentId], [ShipmentId], [HistoryShipmentId], [TrailerType], [Temperature], [Layout], [BindDate], [AssignedTrailerDate], [Status], [CustomStatusId], [LastStatusChangedDate], [LastRateChanged], [WasAtPickup], [WasAtDelivery], [PickupOrder], [DeliveryOrder], [Board], [DatBrokerId], [CustomerId], [BOLFile], [BOLDate], [PODFile], [PODDate], [LumperFile], [LumperDate], [PaperworkFile], [PaperworkDate], [InvoiceNumber], [InvoiceFile], [InvoiceDate], [EmailInvoiceSentDate], [EmailInvoiceSent], [CheckNumber], [CheckFile], [CheckDate], [OldOrderId], [ConfirmationDate], [ConfirmationFile], [EmailConfirmationSentDate], [ConfirmedDate], [ConfirmedFile], [TrailerSchemeDate], [TrailerSchemeFile], [TrailerSchemeId], [EmailConfirmationSent], [PhotoFile], [Payout], [NewScheduledAppointment], [Submited], [SendSms], [DeliveredDate], [CreatedDate], [CreatedById], [LastModifiedDate], [LastModifiedById], [IsDeleted], [HistoryId], [CompanyId], [CurrentLatitude], [CurrentLongitude])
select s.[EntityId], s.[Index], s.[IndexExtra], s.[IndexPrefix], s.[ActualCompanyId], s.[TrackerId], s.[CarrierViewPhone], s.[CarrierViewLink], s.[CarrierViewLastUpdatedDate], s.[CarrierViewStatus], s.[CarrierViewStatusesJson], s.[CarrierViewLate], s.[CarrierViewPickupId], s.[CarrierViewDeliveryId], s.[CarrierViewPickupComment], s.[CarrierViewDeliveryComment], s.[RecordId], s.[BOLNumber], s.[DestinationWarehouseId], s.[WarehouseId], s.[TrailerId], s.[DatCompanyId], s.[DatDriverId], s.[HistoryDriverId], s.[HistoryTruckId], s.[HistoryTrailerId], s.[HistoryAssignedTrailerId], s.[HistoryWarehouseId], s.[HistoryDatCompanyId], s.[HistoryDatDriverId], s.[OriginCity], s.[DestinationCity], s.[CurrentOriginUtcOffset], s.[CurrentDestinationUtcOffset], s.[OriginAddress], s.[DestinationAddress], s.OriginLocation.Lat, s.OriginLocation.Long, s.DestinationLocation.Lat, s.DestinationLocation.Long, s.[Distance], s.[Comments], s.[Comments2], s.[Consolidation], s.[Equipment], s.[PalletType], s.[Weight], s.[Length], s.[Pallets], s.[Spots], s.[WeightType], s.[Commodity], s.[PickupFrom], s.[PickupTo], s.[DeliveryFrom], s.[DeliveryTo], s.[ReceiverId], s.[ShipperId], s.[PickupNumber], s.[DeliveryNumber], s.[BookedDate], s.[BoardChanged], s.[CarrierViewTracking], s.[LineHaul], s.[DatRate], s.[Expenses], s.[Gains], s.[Currency], s.[QuikPay], s.[LoadType], s.[ShowPickupAddress], s.[Background], s.[Foreground], s.[DivideType], s.[ContainerType], s.[ParentId], s.[ShipmentId], s.[HistoryShipmentId], s.[TrailerType], s.[Temperature], s.[Layout], s.[BindDate], s.[AssignedTrailerDate], s.[Status], s.[CustomStatusId], s.[LastStatusChangedDate], s.[LastRateChanged], s.[WasAtPickup], s.[WasAtDelivery], s.[PickupOrder], s.[DeliveryOrder], s.[Board], s.[DatBrokerId], s.[CustomerId], s.[BOLFile], s.[BOLDate], s.[PODFile], s.[PODDate], s.[LumperFile], s.[LumperDate], s.[PaperworkFile], s.[PaperworkDate], s.[InvoiceNumber], s.[InvoiceFile], s.[InvoiceDate], s.[EmailInvoiceSentDate], s.[EmailInvoiceSent], s.[CheckNumber], s.[CheckFile], s.[CheckDate], s.[OldOrderId], s.[ConfirmationDate], s.[ConfirmationFile], s.[EmailConfirmationSentDate], s.[ConfirmedDate], s.[ConfirmedFile], s.[TrailerSchemeDate], s.[TrailerSchemeFile], s.[TrailerSchemeId], s.[EmailConfirmationSent], s.[PhotoFile], s.[Payout], s.[NewScheduledAppointment], s.[Submited], s.[SendSms], s.[DeliveredDate], s.[CreatedDate], s.[CreatedById], s.[LastModifiedDate], s.[LastModifiedById], s.[IsDeleted], s.[HistoryId], s.[CompanyId], s.CurrentLocation.Lat, s.CurrentLocation.Long
from autocall.dbo.d_Load s
where s.CompanyId in (select Id from #active);

-- d_HistoryLocationTrailer -> HistoryLocationTrailer
set identity_insert dbo.HistoryLocationTrailer on;
insert into dbo.HistoryLocationTrailer ([Id], [TrailerId], [EventId], [HistoryId], [CreatedDate], [CreatedById], [Latitude], [Longitude])
select s.[Id], s.[TrailerId], s.[EventId], s.[HistoryId], s.[CreatedDate], s.[CreatedById], s.Location.Lat, s.Location.Long
from autocall.dbo.d_HistoryLocationTrailer s
where s.TrailerId in (select EntityId from autocall.dbo.d_Trailer where CompanyId in (select Id from #active));
set identity_insert dbo.HistoryLocationTrailer off;

-- d_HistoryLocationTruck -> HistoryLocationTruck
set identity_insert dbo.HistoryLocationTruck on;
insert into dbo.HistoryLocationTruck ([Id], [TruckId], [HistoryLocationTrailerId], [EventId], [HistoryId], [CreatedDate], [CreatedById], [Latitude], [Longitude])
select s.[Id], s.[TruckId], s.[HistoryLocationTrailerId], s.[EventId], s.[HistoryId], s.[CreatedDate], s.[CreatedById], s.Location.Lat, s.Location.Long
from autocall.dbo.d_HistoryLocationTruck s
where s.TruckId in (select EntityId from autocall.dbo.d_Truck where CompanyId in (select Id from #active));
set identity_insert dbo.HistoryLocationTruck off;

-- d_HistoryLocationDriver -> HistoryLocationDriver
set identity_insert dbo.HistoryLocationDriver on;
insert into dbo.HistoryLocationDriver ([Id], [DriverId], [HistoryLocationTruckId], [EventId], [HistoryId], [CreatedDate], [CreatedById], [Latitude], [Longitude])
select s.[Id], s.[DriverId], s.[HistoryLocationTruckId], s.[EventId], s.[HistoryId], s.[CreatedDate], s.[CreatedById], s.Location.Lat, s.Location.Long
from autocall.dbo.d_HistoryLocationDriver s
where s.DriverId in (select EntityId from autocall.dbo.d_Driver where CompanyId in (select Id from #active));
set identity_insert dbo.HistoryLocationDriver off;

-- d_HistoryLocationDatDriver -> HistoryLocationDatDriver
set identity_insert dbo.HistoryLocationDatDriver on;
insert into dbo.HistoryLocationDatDriver ([Id], [DatDriverId], [EventId], [HistoryId], [CreatedDate], [CreatedById], [Latitude], [Longitude])
select s.[Id], s.[DatDriverId], s.[EventId], s.[HistoryId], s.[CreatedDate], s.[CreatedById], s.Location.Lat, s.Location.Long
from autocall.dbo.d_HistoryLocationDatDriver s
where s.DatDriverId in (select EntityId from autocall.dbo.d_DatDriver where CompanyId in (select Id from #active));
set identity_insert dbo.HistoryLocationDatDriver off;

-- d_HistoryLocationLoad -> HistoryLocationLoad
set identity_insert dbo.HistoryLocationLoad on;
insert into dbo.HistoryLocationLoad ([Id], [LoadId], [HistoryLocationTrailerId], [HistoryLocationDatDriverId], [HistoryDatCompanyId], [HistoryShipmentId], [HistoryWarehouseId], [EventId], [HistoryId], [CreatedDate], [CreatedById], [Latitude], [Longitude])
select s.[Id], s.[LoadId], s.[HistoryLocationTrailerId], s.[HistoryLocationDatDriverId], s.[HistoryDatCompanyId], s.[HistoryShipmentId], s.[HistoryWarehouseId], s.[EventId], s.[HistoryId], s.[CreatedDate], s.[CreatedById], s.Location.Lat, s.Location.Long
from autocall.dbo.d_HistoryLocationLoad s
where s.LoadId in (select EntityId from autocall.dbo.d_Load where CompanyId in (select Id from #active));
set identity_insert dbo.HistoryLocationLoad off;

dbcc checkident ('dbo.HistoryLocationLoad', reseed);
dbcc checkident ('dbo.HistoryLocationTruck', reseed);
dbcc checkident ('dbo.HistoryLocationTrailer', reseed);
dbcc checkident ('dbo.HistoryLocationDriver', reseed);
dbcc checkident ('dbo.HistoryLocationDatDriver', reseed);

select 'Load' as [table], count(*) as [rows] from dbo.Load
union all select 'HistoryLocationLoad', count(*) from dbo.HistoryLocationLoad
union all select 'HistoryLocationTruck', count(*) from dbo.HistoryLocationTruck
union all select 'HistoryLocationTrailer', count(*) from dbo.HistoryLocationTrailer
union all select 'HistoryLocationDriver', count(*) from dbo.HistoryLocationDriver
union all select 'HistoryLocationDatDriver', count(*) from dbo.HistoryLocationDatDriver;

commit tran;

-------- 3. HistoryObject — снимки, на которые ссылаются перенесённые объекты, плюс все снимки грузов активных компаний не в статусе Paid --------
-- Правило QuartzService наоборот (AI/wiki/data-migration.md). Отбор — в постоянную таблицу dbo._MigrateHistoryKeep: при каждом запуске
-- дополняется ссылками, появившимися в V1 после прошлого прогона (догон); заливка пакетами по 100 000 Id без общей транзакции,
-- возобновляется с max(Id) приёмника, затем докачиваются снимки ниже max(Id), ставшие нужными позже (Undo, груз вышел из Paid).
-- Для полного повтора: delete from dbo.HistoryObject; drop table dbo._MigrateHistoryKeep;
if object_id('dbo._MigrateHistoryKeep') is null
    create table dbo._MigrateHistoryKeep (Id int not null primary key);

with refs as (
        select HistoryId as Id from autocall.dbo.d_Load l join #active c on c.Id = l.CompanyId where HistoryId is not null
        union select hl.HistoryId from autocall.dbo.d_HistoryLocationLoad hl join autocall.dbo.d_Load l on l.EntityId = hl.LoadId join #active c on c.Id = l.CompanyId
        union select hl.HistoryWarehouseId from autocall.dbo.d_HistoryLocationLoad hl join autocall.dbo.d_Load l on l.EntityId = hl.LoadId join #active c on c.Id = l.CompanyId where hl.HistoryWarehouseId is not null
        union select hl.HistoryDatCompanyId from autocall.dbo.d_HistoryLocationLoad hl join autocall.dbo.d_Load l on l.EntityId = hl.LoadId join #active c on c.Id = l.CompanyId where hl.HistoryDatCompanyId is not null
        union select HistoryId from autocall.dbo.d_Driver d join #active c on c.Id = d.CompanyId
        union select HistoryId from autocall.dbo.d_Truck d join #active c on c.Id = d.CompanyId
        union select HistoryId from autocall.dbo.d_Trailer d join #active c on c.Id = d.CompanyId
        union select HistoryId from autocall.dbo.d_Warehouse d join #active c on c.Id = d.CompanyId
        union select HistoryId from autocall.dbo.d_Yard d join #active c on c.Id = d.CompanyId
        union select HistoryId from autocall.dbo.d_DatCompany d join #active c on c.Id = d.CompanyId
        union select HistoryId from autocall.dbo.d_DatDriver d join #active c on c.Id = d.CompanyId
        union select HistoryId from autocall.dbo.d_SetupPacket d join #active c on c.Id = d.CompanyId
        union select hl.HistoryId from autocall.dbo.d_HistoryLocationTruck hl join autocall.dbo.d_Truck d on d.EntityId = hl.TruckId join #active c on c.Id = d.CompanyId
        union select hl.HistoryId from autocall.dbo.d_HistoryLocationTrailer hl join autocall.dbo.d_Trailer d on d.EntityId = hl.TrailerId join #active c on c.Id = d.CompanyId
        union select hl.HistoryId from autocall.dbo.d_HistoryLocationDriver hl join autocall.dbo.d_Driver d on d.EntityId = hl.DriverId join #active c on c.Id = d.CompanyId
        union select hl.HistoryId from autocall.dbo.d_HistoryLocationDatDriver hl join autocall.dbo.d_DatDriver d on d.EntityId = hl.DatDriverId join #active c on c.Id = d.CompanyId)
    insert into dbo._MigrateHistoryKeep (Id)
    select x.Id from (
        select Id from autocall.dbo.d_HistoryObject where Id in (select Id from refs)
        union
        select h.Id from autocall.dbo.d_HistoryObject h
          join autocall.dbo.d_Load l on l.EntityId = h.ReferenceId
          join #active c on c.Id = l.CompanyId
         where l.IsDeleted = 0 and l.Status <> 6) x
    where not exists (select 1 from dbo._MigrateHistoryKeep k where k.Id = x.Id);

declare @lastHistory int, @batch int;
set identity_insert dbo.HistoryObject on;
while 1 = 1 begin
    select @lastHistory = isnull(max(Id), 0) from dbo.HistoryObject;
    insert into dbo.HistoryObject ([Id], [Data], [CreatedDate], [ReferenceId], [GroupId], [Undo])
    select top (100000) h.[Id], h.[Data], h.[CreatedDate], h.[ReferenceId], h.[GroupId], h.[Undo]
    from autocall.dbo.d_HistoryObject h
    join dbo._MigrateHistoryKeep k on k.Id = h.Id
    where h.Id > @lastHistory
    order by h.Id;
    set @batch = @@rowcount;
    raiserror ('HistoryObject: +%d after %d', 0, 1, @batch, @lastHistory) with nowait;
    if @batch = 0 break;
end;
-- снимки ниже max(Id), которых не было в отборе прошлого прогона
insert into dbo.HistoryObject ([Id], [Data], [CreatedDate], [ReferenceId], [GroupId], [Undo])
select h.[Id], h.[Data], h.[CreatedDate], h.[ReferenceId], h.[GroupId], h.[Undo]
from autocall.dbo.d_HistoryObject h
join dbo._MigrateHistoryKeep k on k.Id = h.Id
where not exists (select 1 from dbo.HistoryObject x where x.Id = h.Id);
set @batch = @@rowcount;
raiserror ('HistoryObject: +%d below max(Id)', 0, 1, @batch) with nowait;
set identity_insert dbo.HistoryObject off;
dbcc checkident ('dbo.HistoryObject', reseed);

select 'HistoryObject' as [table], count(*) as [rows], max(Id) as [maxId], sum(datalength(Data)) / 1024 / 1024 as [mb] from dbo.HistoryObject;

-------- 4. Event активных компаний: HistoryId среди перенесённых снимков или пусто; пакетами по 500 000, возобновляемо --------
-- Для полного повтора: delete from dbo.Event;
-- Догон берёт события после max(Id); старые события, чей снимок попал в отбор только сейчас (груз вышел из Paid), не докачиваются —
-- полный проход по d_Event слишком дорог, теряется только их история.
declare @lastEvent int;
set identity_insert dbo.Event on;
while 1 = 1 begin
    select @lastEvent = isnull(max(Id), 0) from dbo.Event;
    insert into dbo.Event ([Id], [CompanyId], [CreatedDate], [Type], [HistoryId])
    select top (500000) e.[Id], e.[CompanyId], e.[CreatedDate], e.[Type], e.[HistoryId]
    from autocall.dbo.d_Event e
    join #active c on c.Id = e.CompanyId
    left join dbo._MigrateHistoryKeep k on k.Id = e.HistoryId
    where e.Id > @lastEvent and (e.HistoryId is null or k.Id is not null)
    order by e.Id;
    set @batch = @@rowcount;
    raiserror ('Event: +%d after %d', 0, 1, @batch, @lastEvent) with nowait;
    if @batch = 0 break;
end;
set identity_insert dbo.Event off;
dbcc checkident ('dbo.Event', reseed);

select 'Event' as [table], count(*) as [rows], max(Id) as [maxId] from dbo.Event;

-------- 5. файлы: каталоги E:\Files\{EntityId} для Scripts/sync-dispatch-files.ps1 (tar | ssh) — результат сохранить в dirs.txt --------
-- Каталог {EntityId} бывает у груза, карриера (DatCompany), setup packet, водителя, трака, трейлера, дефекта и ремонта, шаблонов
-- DispatchCompany (V1 FileGetFullPath; до 2026-11-18 список брал только первые три). Берётся из перенесённых строк; каталоги,
-- которых нет на диске, скрипт пропускает. Нижний регистр: сервер строит путь через Guid.ToString(), на Linux регистр важен.
select lower(convert(varchar(36), EntityId)) as Dir from dbo.Load where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.DatCompany      where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.SetupPacket     where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.Driver          where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.Truck           where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.Trailer         where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.Defect          where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.DefectRepair    where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.DispatchCompany where IsDeleted = 0;

-------- 6. включить FK с проверкой: нарушения — ошибка с именем ограничения --------
-- Проверено на боевой БД 2026-11-13: три удалённых груза активной компании ссылаются на DatCompany, ShipperReceiverCompany и
-- ActualCompany другой (неактивной) компании — такие ссылки обнуляются, остальные FK чистые (AI/wiki/data-migration.md)
update dbo.Load set DatCompanyId = null where DatCompanyId is not null and not exists (select 1 from dbo.DatCompany d where d.EntityId = dbo.Load.DatCompanyId);
update dbo.Load set ShipperId = null where ShipperId is not null and not exists (select 1 from dbo.ShipperReceiverCompany d where d.EntityId = dbo.Load.ShipperId);
update dbo.Load set ReceiverId = null where ReceiverId is not null and not exists (select 1 from dbo.ShipperReceiverCompany d where d.EntityId = dbo.Load.ReceiverId);
update dbo.Load set DispatchCompanyId = null where DispatchCompanyId is not null and not exists (select 1 from dbo.DispatchCompany d where d.EntityId = dbo.Load.DispatchCompanyId);
dbcc checkconstraints with all_constraints;   -- отчёт по нарушениям, если есть
exec sp_MSforeachtable 'alter table ? with check check constraint all';

-------- 7. адреса обратного геокодинга: autocall.dbo.Cache -> GeocodeAddress (решено 2026-11-20, AI/wiki/data-migration.md) --------
-- Cache V1 — общая таблица: ключи MD5 (16 байт) — часовые пояса, места, расстояния, IP, ставки DAT — не переносятся.
-- Адрес — ключ 32 байта (V1 GeocodeService): байты 1–8 нули, 9–12 — вариант (0 — formatted_address, его читает GeocodeService;
-- 1 — GetFormattedPlaceNames2 карты WebV1, не нужен), 17–20 и 25–28 — int32 little-endian round(lat / 0.0005) и round(lng / 0.0005).
-- Data — UTF-16 (BinaryHelper.GetBytes), читается как nvarchar без потерь; в varchar UTF-8 переводится явно, длина проверяется
-- в байтах UTF-8: длиннее varchar(400) и нечётная длина Data не обрезаются, а пропускаются (счётчики в итоге).
-- Только живые (Expires > сейчас, в V1 — 2 года). Догон добавляет новые ключи, существующие (в том числе записанные сервером) не трогает.
if object_id('tempdb..#geo') is not null drop table #geo;
select
    cast(substring(c.[Key], 20, 1) + substring(c.[Key], 19, 1) + substring(c.[Key], 18, 1) + substring(c.[Key], 17, 1) as int) as Lat,
    cast(substring(c.[Key], 28, 1) + substring(c.[Key], 27, 1) + substring(c.[Key], 26, 1) + substring(c.[Key], 25, 1) as int) as Lng,
    case when datalength(c.Data) % 2 = 0
         then cast(cast(c.Data as nvarchar(max)) collate Latin1_General_100_CI_AI_SC_UTF8 as varchar(max)) end as Address,
    c.CreatedDate
into #geo
from autocall.dbo.Cache c
where datalength(c.[Key]) = 32
  and substring(c.[Key], 1, 16) = 0x00000000000000000000000000000000
  and substring(c.[Key], 21, 4) = 0x00000000 and substring(c.[Key], 29, 4) = 0x00000000
  and c.Expires > sysutcdatetime();

select count(*) as [cache addresses],
       sum(case when Address is null then 1 else 0 end) as [skipped: odd length],
       sum(case when datalength(Address) > 400 then 1 else 0 end) as [skipped: > 400 bytes],
       max(datalength(Address)) as [max bytes]
from #geo;

insert into dbo.GeocodeAddress with (tablock) ([Lat], [Lng], [Address], [CreatedDate])
select g.Lat, g.Lng, g.Address, g.CreatedDate
from #geo g
where g.Address is not null and datalength(g.Address) <= 400
  and not exists (select 1 from dbo.GeocodeAddress a where a.Lat = g.Lat and a.Lng = g.Lng);
raiserror ('GeocodeAddress: +%d', 0, 1, @@rowcount) with nowait;

select 'GeocodeAddress' as [table], count(*) as [rows] from dbo.GeocodeAddress;
drop table #geo;
