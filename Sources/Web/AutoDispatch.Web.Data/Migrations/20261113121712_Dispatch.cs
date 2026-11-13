using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoDispatch.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Dispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyCustomStatus",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyCustomStatus", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_CompanyCustomStatus_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerCompany",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Contacts = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCompany", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_CustomerCompany_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DefectRepair",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Comment = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefectRepair", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_DefectRepair_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DispatchCompany",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogoSmall = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    LogoBig = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    McNumber = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalName = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    PhysicalAddress = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    MailingAddress = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    ShippersPhone = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    CellPhone = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    WorkPhone = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    WorkFax = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    WorkEmail = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    PaymentInstructions = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    SMTP_Email = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    SMTP_Password = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    SMTP_Host = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    SMTP_Port = table.Column<int>(type: "int", nullable: false),
                    SMS_Phone = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    ConfirmationFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    BOLFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    InvoiceFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    BnsfLogin = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    BnsfPassword = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    BnsfLastMessage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    BnsfLastDone = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BnsfCookies = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    SubmitRates = table.Column<bool>(type: "bit", nullable: false),
                    SMTP_SchemeEmail = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    SMTP_SchemePassword = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    SMTP_SchemeHost = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    SMTP_SchemePort = table.Column<int>(type: "int", nullable: false),
                    BoardType = table.Column<byte>(type: "tinyint", nullable: false),
                    NextConfirmationNumber = table.Column<int>(type: "int", nullable: false),
                    NextInvoiceNumber = table.Column<int>(type: "int", nullable: false),
                    NextLoadNumber = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumberAsLoadNumber = table.Column<bool>(type: "bit", nullable: false),
                    NextLoadExtra = table.Column<byte[]>(type: "varbinary(128)", maxLength: 128, nullable: true),
                    CarrierViewToken = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    FactoringCompanyJson = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchCompany", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_DispatchCompany_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HistoryObject",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Undo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryObject", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SetupPacketContact",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Contact = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Phone = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Title = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Cell = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Fax = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupPacketContact", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "SetupPacketFactoringCompany",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    City = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    ZipCode = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    Email = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    StateId = table.Column<byte>(type: "tinyint", nullable: false),
                    CountryId = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupPacketFactoringCompany", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "SetupPacketInsuranceAgency",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Phone = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Fax = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Number = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupPacketInsuranceAgency", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "ShipperReceiverCompany",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Contacts = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipperReceiverCompany", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_ShipperReceiverCompany_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DispatchCompanyCustom",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispatchCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Params = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchCompanyCustom", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_DispatchCompanyCustom_DispatchCompany_DispatchCompanyId",
                        column: x => x.DispatchCompanyId,
                        principalTable: "DispatchCompany",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "DatDriver",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    TruckNumber = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    TrailerNumber = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    ContactPhone = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Comments = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CarrierViewId = table.Column<int>(type: "int", nullable: true),
                    CarrierViewLoadId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CarrierViewLink = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CarrierViewLastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CarrierViewStatus = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CarrierViewStatusesJson = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CarrierViewLate = table.Column<bool>(type: "bit", nullable: false),
                    CarrierViewPickupId = table.Column<int>(type: "int", nullable: true),
                    CarrierViewDeliveryId = table.Column<int>(type: "int", nullable: true),
                    CarrierViewPickupComment = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CarrierViewDeliveryComment = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CurrentLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatDriver", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_DatDriver_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DatDriver_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Event_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Event_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Warehouse",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    City = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouse", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Warehouse_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Warehouse_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Yard",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    Address = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    City = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Yard", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Yard_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Yard_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SetupPacket",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<byte>(type: "tinyint", nullable: false),
                    LegalName = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    DBAName = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Step = table.Column<byte>(type: "tinyint", nullable: false),
                    PhysicalAddress = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    PhysicalCity = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    PhysicalStateId = table.Column<byte>(type: "tinyint", nullable: true),
                    PhysicalZipCode = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    PhysicalCountryId = table.Column<byte>(type: "tinyint", nullable: true),
                    MailingCompanyLegalName = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    MailingAddress = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    MailingCity = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    MailingStateId = table.Column<byte>(type: "tinyint", nullable: true),
                    MailingZipCode = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    MailingCountryId = table.Column<byte>(type: "tinyint", nullable: true),
                    MailingEmail = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    DunsNumber = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    UseFactoringCompany = table.Column<bool>(type: "bit", nullable: false),
                    FactoringCompanyId = table.Column<short>(type: "smallint", nullable: true),
                    ConfidantContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorporateContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SalesContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DispatchContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AfterHourContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SalesSameAs = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    DispatchSameAs = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    AfterHourSameAs = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CVORNumber = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    NIRNumber = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    NSCNumber = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    CountryServiceType = table.Column<byte>(type: "tinyint", nullable: true),
                    AuthorizedToHaulElectronics = table.Column<bool>(type: "bit", nullable: true),
                    HasReferBreakdown = table.Column<bool>(type: "bit", nullable: true),
                    HasNonOwnedAuto = table.Column<bool>(type: "bit", nullable: true),
                    HasActiveShipmentAtTimeOfReg = table.Column<bool>(type: "bit", nullable: true),
                    AllowAcquirePolicyExclusions = table.Column<bool>(type: "bit", nullable: false),
                    AgreementAgree = table.Column<bool>(type: "bit", nullable: false),
                    AgreementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Agreement_02Signature = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Agreement_02SignatureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Agreement_02Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Agreement2_02Signature = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    AutoSameAs = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CargoSameAs = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    WorkersCompensationSameAs = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    GeneralLiabilityInsuranceAgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AutoInsuranceAgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CargoInsuranceAgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkersCompensationInsuranceAgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkersCompensationSignature = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    WorkersCompensationSignatureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    W9Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    W9BusinessName = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    W9CompanyType = table.Column<byte>(type: "tinyint", nullable: true),
                    W9TaxClassification = table.Column<byte>(type: "tinyint", nullable: true),
                    W9OtherTaxClassification = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    W9ExemptPayeeCode = table.Column<byte>(type: "tinyint", nullable: true),
                    W9FATCACode = table.Column<byte>(type: "tinyint", nullable: true),
                    W9Address = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    W9City = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    W9StateId = table.Column<byte>(type: "tinyint", nullable: true),
                    W9ZipCode = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    W9NamePerson = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    W9AccountNumber = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    W9SocialSecurityNumber = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    W9EmployerIdentificationNumber = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    W9Agree = table.Column<bool>(type: "bit", nullable: false),
                    W9AgreeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    W9Signature = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Step8Modes = table.Column<byte[]>(type: "varbinary(2)", maxLength: 2, nullable: true),
                    Step8Commodities = table.Column<byte[]>(type: "varbinary(4)", maxLength: 4, nullable: true),
                    TractorsCount = table.Column<short>(type: "smallint", nullable: false),
                    SCACCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    DriversCount = table.Column<short>(type: "smallint", nullable: true),
                    TeamsCount = table.Column<short>(type: "smallint", nullable: true),
                    OperatorsCount = table.Column<short>(type: "smallint", nullable: true),
                    OwnerName = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    OwnerPhone = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    OwnerEmail = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    HasMexico = table.Column<bool>(type: "bit", nullable: false),
                    HasCanadian = table.Column<bool>(type: "bit", nullable: false),
                    HasEmailCarrier = table.Column<bool>(type: "bit", nullable: false),
                    CertificationSmartWay = table.Column<bool>(type: "bit", nullable: true),
                    CertificationC_TPAT = table.Column<bool>(type: "bit", nullable: true),
                    CertificationFAST = table.Column<bool>(type: "bit", nullable: true),
                    CertificationTWIC = table.Column<bool>(type: "bit", nullable: true),
                    CertificationCARB = table.Column<bool>(type: "bit", nullable: true),
                    CertificationHazMatCertification = table.Column<bool>(type: "bit", nullable: true),
                    CertificationSafetyPermit = table.Column<bool>(type: "bit", nullable: true),
                    SafetyAgree = table.Column<bool>(type: "bit", nullable: false),
                    LoadUrgency = table.Column<byte>(type: "tinyint", nullable: false),
                    NumberOfUnits = table.Column<byte[]>(type: "varbinary(80)", maxLength: 80, nullable: true),
                    W8States = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Diversity1M_WBE = table.Column<bool>(type: "bit", nullable: true),
                    Diversity1 = table.Column<byte>(type: "tinyint", nullable: true),
                    Diversity2 = table.Column<byte>(type: "tinyint", nullable: true),
                    Diversity2Other = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Diversity3Certified = table.Column<bool>(type: "bit", nullable: true),
                    Diversity3 = table.Column<byte>(type: "tinyint", nullable: true),
                    Diversity3Other = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    FileMC = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    FileCOI = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    EmailSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupPacket", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_SetupPacket_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SetupPacket_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SetupPacket_SetupPacketContact_AfterHourContactId",
                        column: x => x.AfterHourContactId,
                        principalTable: "SetupPacketContact",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_SetupPacket_SetupPacketContact_ConfidantContactId",
                        column: x => x.ConfidantContactId,
                        principalTable: "SetupPacketContact",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_SetupPacket_SetupPacketContact_CorporateContactId",
                        column: x => x.CorporateContactId,
                        principalTable: "SetupPacketContact",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_SetupPacket_SetupPacketContact_DispatchContactId",
                        column: x => x.DispatchContactId,
                        principalTable: "SetupPacketContact",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_SetupPacket_SetupPacketContact_SalesContactId",
                        column: x => x.SalesContactId,
                        principalTable: "SetupPacketContact",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_SetupPacket_SetupPacketInsuranceAgency_AutoInsuranceAgencyId",
                        column: x => x.AutoInsuranceAgencyId,
                        principalTable: "SetupPacketInsuranceAgency",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_SetupPacket_SetupPacketInsuranceAgency_CargoInsuranceAgencyId",
                        column: x => x.CargoInsuranceAgencyId,
                        principalTable: "SetupPacketInsuranceAgency",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_SetupPacket_SetupPacketInsuranceAgency_GeneralLiabilityInsuranceAgencyId",
                        column: x => x.GeneralLiabilityInsuranceAgencyId,
                        principalTable: "SetupPacketInsuranceAgency",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_SetupPacket_SetupPacketInsuranceAgency_WorkersCompensationInsuranceAgencyId",
                        column: x => x.WorkersCompensationInsuranceAgencyId,
                        principalTable: "SetupPacketInsuranceAgency",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "HistoryLocationDatDriver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatDriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryLocationDatDriver", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryLocationDatDriver_DatDriver_DatDriverId",
                        column: x => x.DatDriverId,
                        principalTable: "DatDriver",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_HistoryLocationDatDriver_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Trailer",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    AnnualInspection_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    AnnualInspection_Expiration = table.Column<DateTime>(type: "date", nullable: true),
                    Registration_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Maintenance_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Maintenance_Expiration = table.Column<DateTime>(type: "date", nullable: true),
                    SchemeFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    OriginSchemeFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    SchemeDate = table.Column<DateTime>(type: "date", nullable: true),
                    BnsfOwner = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfShipper = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfUnit = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfLoadEmptyStatus = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfCurrentLocation = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfEventCode = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfEventDate = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfETA_Deramp = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfOrigin = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfDestination = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfBeyondGoalHours = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfTimeDeramp = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfTimeNotify = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfFinalTrainArrival = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfGoalDate = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfNextevent = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfPickUpNumber = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfStorageDay = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfTrain = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfTrucker = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfWaybillDate = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfRateAuthority = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfAmountDue = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfDueDate = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfWaybillNumber = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfBillNumber = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    BnsfAdjustments = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    History = table.Column<bool>(type: "bit", nullable: false),
                    YardId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CurrentLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trailer", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Trailer_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trailer_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trailer_Yard_YardId",
                        column: x => x.YardId,
                        principalTable: "Yard",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "DatCompany",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispatchCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    McNumber = table.Column<int>(type: "int", nullable: true),
                    DotNumber = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    EntityType = table.Column<byte>(type: "tinyint", nullable: false),
                    Contacts = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Phy_UpsStore = table.Column<bool>(type: "bit", nullable: false),
                    Mai_UpsStore = table.Column<bool>(type: "bit", nullable: false),
                    Phy_MailBox = table.Column<bool>(type: "bit", nullable: false),
                    Mai_MailBox = table.Column<bool>(type: "bit", nullable: false),
                    InsuranceExpiration = table.Column<DateTime>(type: "date", nullable: true),
                    InsuranceFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Comments = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Rating = table.Column<byte>(type: "tinyint", nullable: false),
                    HyperlinkFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DoNotUse = table.Column<bool>(type: "bit", nullable: false),
                    SetupPacketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SetupPacketEmailSent = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    SetupPacketSent = table.Column<bool>(type: "bit", nullable: false),
                    SetupPacketCompleted = table.Column<bool>(type: "bit", nullable: false),
                    SetupPacketAccept = table.Column<bool>(type: "bit", nullable: false),
                    SetupPacketFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    W9File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatCompany", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_DatCompany_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DatCompany_DispatchCompany_DispatchCompanyId",
                        column: x => x.DispatchCompanyId,
                        principalTable: "DispatchCompany",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_DatCompany_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DatCompany_SetupPacket_SetupPacketId",
                        column: x => x.SetupPacketId,
                        principalTable: "SetupPacket",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "SetupPacketReference",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Contact = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Company = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Phone = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    SetupPacketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupPacketReference", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_SetupPacketReference_SetupPacket_SetupPacketId",
                        column: x => x.SetupPacketId,
                        principalTable: "SetupPacket",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "HistoryLocationTrailer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryLocationTrailer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryLocationTrailer_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoryLocationTrailer_Trailer_TrailerId",
                        column: x => x.TrailerId,
                        principalTable: "Trailer",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "Truck",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    TrailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnnualInspection_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    AnnualInspection_Expiration = table.Column<DateTime>(type: "date", nullable: true),
                    Registration_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Registration_Expiration = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CurrentLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Truck", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Truck_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Truck_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Truck_Trailer_TrailerId",
                        column: x => x.TrailerId,
                        principalTable: "Trailer",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "Defect",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TruckId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RepairId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefectId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ServiceNeeded = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Defect", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Defect_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Defect_DefectRepair_RepairId",
                        column: x => x.RepairId,
                        principalTable: "DefectRepair",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Defect_Trailer_TrailerId",
                        column: x => x.TrailerId,
                        principalTable: "Trailer",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Defect_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "HistoryLocationTruck",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TruckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HistoryLocationTrailerId = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryLocationTruck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryLocationTruck_HistoryLocationTrailer_HistoryLocationTrailerId",
                        column: x => x.HistoryLocationTrailerId,
                        principalTable: "HistoryLocationTrailer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoryLocationTruck_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoryLocationTruck_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "CacheLoadIndex",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    City = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Company = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CacheLoadIndex", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CacheLoadIndex_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Driver",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TruckId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentLoadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DriverStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    CarrierViewId = table.Column<int>(type: "int", nullable: true),
                    CarrierViewLastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    THE_APPLICATION_OF_EMPLOYMENT_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    MVR_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    MEDICAL_CARD_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CDL_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DOT_INSPECTIONS_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    BACKGROUND_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DRUG_TEST_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    RANDOM_DRUG_TEST_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    ACH_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    W9_File = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    MVR_Expiration = table.Column<DateTime>(type: "date", nullable: true),
                    MEDICAL_CARD_Expiration = table.Column<DateTime>(type: "date", nullable: true),
                    CDL_Expiration = table.Column<DateTime>(type: "date", nullable: true),
                    ACH_Expiration = table.Column<DateTime>(type: "date", nullable: true),
                    W9_Expiration = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CurrentLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Driver", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Driver_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Driver_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Driver_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "HistoryLocationDriver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HistoryLocationTruckId = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryLocationDriver", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryLocationDriver_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_HistoryLocationDriver_HistoryLocationTruck_HistoryLocationTruckId",
                        column: x => x.HistoryLocationTruckId,
                        principalTable: "HistoryLocationTruck",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoryLocationDriver_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Load",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: true),
                    IndexExtra = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    IndexPrefix = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    DispatchCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrackerId = table.Column<int>(type: "int", nullable: true),
                    CarrierViewPhone = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CarrierViewLink = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CarrierViewLastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CarrierViewStatus = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CarrierViewStatusesJson = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CarrierViewLate = table.Column<bool>(type: "bit", nullable: false),
                    CarrierViewPickupId = table.Column<int>(type: "int", nullable: true),
                    CarrierViewDeliveryId = table.Column<int>(type: "int", nullable: true),
                    CarrierViewPickupComment = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CarrierViewDeliveryComment = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    RecordId = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    BOLNumber = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    DestinationWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DatCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DatDriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HistoryDriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HistoryTruckId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HistoryTrailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HistoryAssignedTrailerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HistoryWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HistoryDatCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HistoryDatDriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OriginCity = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    DestinationCity = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    CurrentOriginUtcOffset = table.Column<int>(type: "int", nullable: false),
                    CurrentDestinationUtcOffset = table.Column<int>(type: "int", nullable: false),
                    OriginAddress = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    OriginLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    OriginLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    DestinationLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    DestinationLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Distance = table.Column<int>(type: "int", nullable: true),
                    Comments = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Comments2 = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Consolidation = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Equipment = table.Column<byte>(type: "tinyint", nullable: false),
                    PalletType = table.Column<byte>(type: "tinyint", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: true),
                    Length = table.Column<int>(type: "int", nullable: true),
                    Pallets = table.Column<short>(type: "smallint", nullable: true),
                    Spots = table.Column<short>(type: "smallint", nullable: true),
                    WeightType = table.Column<byte>(type: "tinyint", nullable: false),
                    Commodity = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    PickupFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PickupTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShipperId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PickupNumber = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    DeliveryNumber = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    BookedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BoardChanged = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarrierViewTracking = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LineHaul = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Expenses = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Gains = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Currency = table.Column<byte>(type: "tinyint", nullable: false),
                    QuikPay = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    LoadType = table.Column<byte>(type: "tinyint", nullable: false),
                    ShowPickupAddress = table.Column<bool>(type: "bit", nullable: false),
                    Background = table.Column<byte[]>(type: "binary(4)", nullable: false),
                    Foreground = table.Column<byte[]>(type: "binary(4)", nullable: false),
                    DivideType = table.Column<byte>(type: "tinyint", nullable: false),
                    ContainerType = table.Column<byte>(type: "tinyint", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HistoryShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrailerType = table.Column<byte>(type: "tinyint", nullable: false),
                    Temperature = table.Column<short>(type: "smallint", nullable: true),
                    Layout = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    BindDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedTrailerDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CustomStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastStatusChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastRateChanged = table.Column<byte>(type: "tinyint", nullable: true),
                    WasAtPickup = table.Column<bool>(type: "bit", nullable: false),
                    WasAtDelivery = table.Column<bool>(type: "bit", nullable: false),
                    PickupOrder = table.Column<byte>(type: "tinyint", nullable: true),
                    DeliveryOrder = table.Column<byte>(type: "tinyint", nullable: true),
                    Board = table.Column<byte>(type: "tinyint", nullable: false),
                    DatBrokerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BOLFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    BOLDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    PODFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    PODDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    LumperFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    LumperDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaperworkFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    PaperworkDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    InvoiceFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    EmailInvoiceSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailInvoiceSent = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CheckNumber = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    CheckFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CheckDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    OldOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmationDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ConfirmationFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    EmailConfirmationSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ConfirmedFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    TrailerSchemeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrailerSchemeFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    TrailerSchemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmailConfirmationSent = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    PhotoFile = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Payout = table.Column<float>(type: "real", nullable: true),
                    NewScheduledAppointment = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Submited = table.Column<bool>(type: "bit", nullable: false),
                    SendSms = table.Column<bool>(type: "bit", nullable: false),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CurrentLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Load", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Load_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Load_CustomerCompany_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerCompany",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_DatCompany_DatBrokerId",
                        column: x => x.DatBrokerId,
                        principalTable: "DatCompany",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_DatCompany_DatCompanyId",
                        column: x => x.DatCompanyId,
                        principalTable: "DatCompany",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_DatCompany_HistoryDatCompanyId",
                        column: x => x.HistoryDatCompanyId,
                        principalTable: "DatCompany",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_DatDriver_DatDriverId",
                        column: x => x.DatDriverId,
                        principalTable: "DatDriver",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_DatDriver_HistoryDatDriverId",
                        column: x => x.HistoryDatDriverId,
                        principalTable: "DatDriver",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_DispatchCompany_DispatchCompanyId",
                        column: x => x.DispatchCompanyId,
                        principalTable: "DispatchCompany",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_Driver_HistoryDriverId",
                        column: x => x.HistoryDriverId,
                        principalTable: "Driver",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Load_Load_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Load",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_Load_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Load",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_ShipperReceiverCompany_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "ShipperReceiverCompany",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_ShipperReceiverCompany_ShipperId",
                        column: x => x.ShipperId,
                        principalTable: "ShipperReceiverCompany",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_Trailer_HistoryAssignedTrailerId",
                        column: x => x.HistoryAssignedTrailerId,
                        principalTable: "Trailer",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_Trailer_HistoryTrailerId",
                        column: x => x.HistoryTrailerId,
                        principalTable: "Trailer",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_Trailer_TrailerId",
                        column: x => x.TrailerId,
                        principalTable: "Trailer",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_Truck_HistoryTruckId",
                        column: x => x.HistoryTruckId,
                        principalTable: "Truck",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_Warehouse_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_Warehouse_HistoryWarehouseId",
                        column: x => x.HistoryWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "EntityId");
                    table.ForeignKey(
                        name: "FK_Load_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateTable(
                name: "HistoryLocationLoad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HistoryLocationTrailerId = table.Column<int>(type: "int", nullable: true),
                    HistoryLocationDatDriverId = table.Column<int>(type: "int", nullable: true),
                    HistoryDatCompanyId = table.Column<int>(type: "int", nullable: true),
                    HistoryShipmentId = table.Column<int>(type: "int", nullable: true),
                    HistoryWarehouseId = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    HistoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryLocationLoad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryLocationLoad_HistoryLocationDatDriver_HistoryLocationDatDriverId",
                        column: x => x.HistoryLocationDatDriverId,
                        principalTable: "HistoryLocationDatDriver",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoryLocationLoad_HistoryLocationTrailer_HistoryLocationTrailerId",
                        column: x => x.HistoryLocationTrailerId,
                        principalTable: "HistoryLocationTrailer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoryLocationLoad_HistoryObject_HistoryDatCompanyId",
                        column: x => x.HistoryDatCompanyId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoryLocationLoad_HistoryObject_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoryLocationLoad_HistoryObject_HistoryWarehouseId",
                        column: x => x.HistoryWarehouseId,
                        principalTable: "HistoryObject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoryLocationLoad_Load_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Load",
                        principalColumn: "EntityId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CacheLoadIndex_CompanyId",
                table: "CacheLoadIndex",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CacheLoadIndex_LoadId",
                table: "CacheLoadIndex",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_CacheLoadLink_City",
                table: "CacheLoadIndex",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_CacheLoadLink_Company",
                table: "CacheLoadIndex",
                column: "Company");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCustomStatus_CompanyId",
                table: "CompanyCustomStatus",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCompany_CompanyId",
                table: "CustomerCompany",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DatCompany_CompanyId",
                table: "DatCompany",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DatCompany_DispatchCompanyId",
                table: "DatCompany",
                column: "DispatchCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DatCompany_HistoryId",
                table: "DatCompany",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DatCompany_McNumber",
                table: "DatCompany",
                column: "McNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DatCompany_SetupPacketId",
                table: "DatCompany",
                column: "SetupPacketId");

            migrationBuilder.CreateIndex(
                name: "IX_DatDriver_CompanyId",
                table: "DatDriver",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DatDriver_HistoryId",
                table: "DatDriver",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Defect_CompanyId",
                table: "Defect",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Defect_RepairId",
                table: "Defect",
                column: "RepairId");

            migrationBuilder.CreateIndex(
                name: "IX_Defect_TrailerId",
                table: "Defect",
                column: "TrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_Defect_TruckId",
                table: "Defect",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_DefectRepair_CompanyId",
                table: "DefectRepair",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchCompany_CompanyId",
                table: "DispatchCompany",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchCompanyCustom_DispatchCompanyId",
                table: "DispatchCompanyCustom",
                column: "DispatchCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_CompanyId",
                table: "Driver",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_CurrentLoadId",
                table: "Driver",
                column: "CurrentLoadId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_HistoryId",
                table: "Driver",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_TruckId",
                table: "Driver",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_CompanyId",
                table: "Event",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_HistoryId",
                table: "Event",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationDatDriver_DatDriverId",
                table: "HistoryLocationDatDriver",
                column: "DatDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationDatDriver_EventId",
                table: "HistoryLocationDatDriver",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationDatDriver_HistoryId",
                table: "HistoryLocationDatDriver",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationDriver_DriverId",
                table: "HistoryLocationDriver",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationDriver_EventId",
                table: "HistoryLocationDriver",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationDriver_HistoryId",
                table: "HistoryLocationDriver",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationDriver_HistoryLocationTruckId",
                table: "HistoryLocationDriver",
                column: "HistoryLocationTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationLoad_EventId",
                table: "HistoryLocationLoad",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationLoad_HistoryDatCompanyId",
                table: "HistoryLocationLoad",
                column: "HistoryDatCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationLoad_HistoryId",
                table: "HistoryLocationLoad",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationLoad_HistoryLocationDatDriverId",
                table: "HistoryLocationLoad",
                column: "HistoryLocationDatDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationLoad_HistoryLocationTrailerId",
                table: "HistoryLocationLoad",
                column: "HistoryLocationTrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationLoad_HistoryWarehouseId",
                table: "HistoryLocationLoad",
                column: "HistoryWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationLoad_LoadId",
                table: "HistoryLocationLoad",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationTrailer_EventId",
                table: "HistoryLocationTrailer",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationTrailer_HistoryId",
                table: "HistoryLocationTrailer",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationTrailer_TrailerId",
                table: "HistoryLocationTrailer",
                column: "TrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationTruck_EventId",
                table: "HistoryLocationTruck",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationTruck_HistoryId",
                table: "HistoryLocationTruck",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationTruck_HistoryLocationTrailerId",
                table: "HistoryLocationTruck",
                column: "HistoryLocationTrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLocationTruck_TruckId",
                table: "HistoryLocationTruck",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryObject_GroupId",
                table: "HistoryObject",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryObject_ReferenceId",
                table: "HistoryObject",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_CompanyId",
                table: "Load",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_CustomerId",
                table: "Load",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_DatBrokerId",
                table: "Load",
                column: "DatBrokerId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_DatCompanyId",
                table: "Load",
                column: "DatCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_DatDriverId",
                table: "Load",
                column: "DatDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_DestinationWarehouseId",
                table: "Load",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_DispatchCompanyId",
                table: "Load",
                column: "DispatchCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_HistoryAssignedTrailerId",
                table: "Load",
                column: "HistoryAssignedTrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_HistoryDatCompanyId",
                table: "Load",
                column: "HistoryDatCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_HistoryDatDriverId",
                table: "Load",
                column: "HistoryDatDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_HistoryDriverId",
                table: "Load",
                column: "HistoryDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_HistoryId",
                table: "Load",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_HistoryTrailerId",
                table: "Load",
                column: "HistoryTrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_HistoryTruckId",
                table: "Load",
                column: "HistoryTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_HistoryWarehouseId",
                table: "Load",
                column: "HistoryWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_ParentId",
                table: "Load",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_ReceiverId",
                table: "Load",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_SendSms",
                table: "Load",
                column: "SendSms");

            migrationBuilder.CreateIndex(
                name: "IX_Load_ShipmentId",
                table: "Load",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_ShipperId",
                table: "Load",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_TrailerId",
                table: "Load",
                column: "TrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_WarehouseId",
                table: "Load",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_AfterHourContactId",
                table: "SetupPacket",
                column: "AfterHourContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_AutoInsuranceAgencyId",
                table: "SetupPacket",
                column: "AutoInsuranceAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_CargoInsuranceAgencyId",
                table: "SetupPacket",
                column: "CargoInsuranceAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_CompanyId",
                table: "SetupPacket",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_ConfidantContactId",
                table: "SetupPacket",
                column: "ConfidantContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_CorporateContactId",
                table: "SetupPacket",
                column: "CorporateContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_DispatchContactId",
                table: "SetupPacket",
                column: "DispatchContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_GeneralLiabilityInsuranceAgencyId",
                table: "SetupPacket",
                column: "GeneralLiabilityInsuranceAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_HistoryId",
                table: "SetupPacket",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_SalesContactId",
                table: "SetupPacket",
                column: "SalesContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacket_WorkersCompensationInsuranceAgencyId",
                table: "SetupPacket",
                column: "WorkersCompensationInsuranceAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupPacketReference_SetupPacketId",
                table: "SetupPacketReference",
                column: "SetupPacketId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipperReceiverCompany_CompanyId",
                table: "ShipperReceiverCompany",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Trailer_CompanyId",
                table: "Trailer",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Trailer_HistoryId",
                table: "Trailer",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Trailer_Number",
                table: "Trailer",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_Trailer_YardId",
                table: "Trailer",
                column: "YardId");

            migrationBuilder.CreateIndex(
                name: "IX_Truck_CompanyId",
                table: "Truck",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Truck_HistoryId",
                table: "Truck",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Truck_TrailerId",
                table: "Truck",
                column: "TrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouse_CompanyId",
                table: "Warehouse",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouse_HistoryId",
                table: "Warehouse",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Yard_CompanyId",
                table: "Yard",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Yard_HistoryId",
                table: "Yard",
                column: "HistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CacheLoadIndex_Load_LoadId",
                table: "CacheLoadIndex",
                column: "LoadId",
                principalTable: "Load",
                principalColumn: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Driver_Load_CurrentLoadId",
                table: "Driver",
                column: "CurrentLoadId",
                principalTable: "Load",
                principalColumn: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Driver_Load_CurrentLoadId",
                table: "Driver");

            migrationBuilder.DropTable(
                name: "CacheLoadIndex");

            migrationBuilder.DropTable(
                name: "CompanyCustomStatus");

            migrationBuilder.DropTable(
                name: "Defect");

            migrationBuilder.DropTable(
                name: "DispatchCompanyCustom");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "HistoryLocationDriver");

            migrationBuilder.DropTable(
                name: "HistoryLocationLoad");

            migrationBuilder.DropTable(
                name: "SetupPacketFactoringCompany");

            migrationBuilder.DropTable(
                name: "SetupPacketReference");

            migrationBuilder.DropTable(
                name: "DefectRepair");

            migrationBuilder.DropTable(
                name: "HistoryLocationTruck");

            migrationBuilder.DropTable(
                name: "HistoryLocationDatDriver");

            migrationBuilder.DropTable(
                name: "HistoryLocationTrailer");

            migrationBuilder.DropTable(
                name: "Load");

            migrationBuilder.DropTable(
                name: "CustomerCompany");

            migrationBuilder.DropTable(
                name: "DatCompany");

            migrationBuilder.DropTable(
                name: "DatDriver");

            migrationBuilder.DropTable(
                name: "Driver");

            migrationBuilder.DropTable(
                name: "ShipperReceiverCompany");

            migrationBuilder.DropTable(
                name: "Warehouse");

            migrationBuilder.DropTable(
                name: "DispatchCompany");

            migrationBuilder.DropTable(
                name: "SetupPacket");

            migrationBuilder.DropTable(
                name: "Truck");

            migrationBuilder.DropTable(
                name: "SetupPacketContact");

            migrationBuilder.DropTable(
                name: "SetupPacketInsuranceAgency");

            migrationBuilder.DropTable(
                name: "Trailer");

            migrationBuilder.DropTable(
                name: "Yard");

            migrationBuilder.DropTable(
                name: "HistoryObject");
        }
    }
}
