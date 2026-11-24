using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoDispatch.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Alter_SetupPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "W9ForeignPartnersHas",
                table: "SetupPacket",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "W9ForeignPartnersHas",
                table: "SetupPacket");
        }
    }
}
