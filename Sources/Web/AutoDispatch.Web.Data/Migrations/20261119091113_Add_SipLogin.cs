using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoDispatch.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_SipLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SipLogin",
                table: "IdentityUsers",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SipPassword",
                table: "IdentityUsers",
                type: "varchar(50)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SipLogin",
                table: "IdentityUsers");

            migrationBuilder.DropColumn(
                name: "SipPassword",
                table: "IdentityUsers");
        }
    }
}
