using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoDispatch.Web.Data.Migrations.Log
{
    /// <inheritdoc />
    public partial class Alter_Log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Metric",
                columns: table => new
                {
                    DateTime = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    MetricId = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metric", x => new { x.DateTime, x.MetricId });
                });

            migrationBuilder.CreateTable(
                name: "UserActivity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeenAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    Source = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLogin",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", nullable: true),
                    Source = table.Column<byte>(type: "tinyint", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Ip = table.Column<string>(type: "varchar(45)", nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(256)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogin", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivity_SeenAt",
                table: "UserActivity",
                column: "SeenAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivity_UserId_SeenAt",
                table: "UserActivity",
                columns: new[] { "UserId", "SeenAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserLogin_DateTime",
                table: "UserLogin",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogin_UserId_DateTime",
                table: "UserLogin",
                columns: new[] { "UserId", "DateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Metric");

            migrationBuilder.DropTable(
                name: "UserActivity");

            migrationBuilder.DropTable(
                name: "UserLogin");
        }
    }
}
