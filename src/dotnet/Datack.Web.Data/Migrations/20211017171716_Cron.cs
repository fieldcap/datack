using Microsoft.EntityFrameworkCore.Migrations;

namespace Datack.Web.Data.Migrations
{
    public partial class Cron : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackupType",
                table: "JobRuns");

            migrationBuilder.AddColumn<string>(
                name: "Cron",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cron",
                table: "Jobs");

            migrationBuilder.AddColumn<int>(
                name: "BackupType",
                table: "JobRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
