using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoDispatch.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Dispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CacheLoadIndex");

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "IdentityUsers",
                type: "varchar(64)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Signature",
                table: "IdentityUsers");

            migrationBuilder.CreateTable(
                name: "CacheLoadIndex",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    City = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Company = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
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
                    table.ForeignKey(
                        name: "FK_CacheLoadIndex_Load_LoadId",
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
        }
    }
}
