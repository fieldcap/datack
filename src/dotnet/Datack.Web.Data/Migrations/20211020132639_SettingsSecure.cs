using Microsoft.EntityFrameworkCore.Migrations;

namespace Datack.Web.Data.Migrations
{
    public partial class SettingsSecure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Settings");

            migrationBuilder.AddColumn<bool>(
                name: "Secure",
                table: "Settings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Secure",
                table: "Settings");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Settings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
