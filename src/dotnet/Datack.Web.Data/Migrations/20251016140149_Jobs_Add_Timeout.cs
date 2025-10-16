using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datack.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Jobs_Add_Timeout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Timeout",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timeout",
                table: "Jobs");
        }
    }
}
