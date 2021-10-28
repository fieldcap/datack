using Microsoft.EntityFrameworkCore.Migrations;

namespace Datack.Web.Data.Migrations
{
    public partial class JobTask_Add_MaxItemsToKeep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxItemsToKeep",
                table: "JobTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxItemsToKeep",
                table: "JobTasks");
        }
    }
}
