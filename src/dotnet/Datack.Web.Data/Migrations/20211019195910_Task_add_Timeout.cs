using Microsoft.EntityFrameworkCore.Migrations;

namespace Datack.Web.Data.Migrations
{
    public partial class Task_add_Timeout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Timeout",
                table: "JobTasks",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timeout",
                table: "JobTasks");
        }
    }
}
