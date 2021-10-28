using Microsoft.EntityFrameworkCore.Migrations;

namespace Datack.Web.Data.Migrations
{
    public partial class Job_Task_Add_IsActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "JobTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Jobs");
        }
    }
}
