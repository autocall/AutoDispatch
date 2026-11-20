using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoDispatch.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_GeocodeAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeocodeAddress",
                columns: table => new
                {
                    Lat = table.Column<int>(type: "int", nullable: false),
                    Lng = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "varchar(400)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeocodeAddress", x => new { x.Lat, x.Lng });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeocodeAddress");
        }
    }
}
