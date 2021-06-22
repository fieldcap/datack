using Microsoft.EntityFrameworkCore.Migrations;

namespace Datack.Data.Migrations
{
    public partial class Step_Add_Type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Steps",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Steps");
        }
    }
}
