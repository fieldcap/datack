using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datack.Data.Migrations
{
    public partial class Step_Server : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Servers_ServerId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_ServerId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "Jobs");

            migrationBuilder.AddColumn<Guid>(
                name: "ServerId",
                table: "Steps",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Steps_ServerId",
                table: "Steps",
                column: "ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Servers_ServerId",
                table: "Steps",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "ServerId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Servers_ServerId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Steps_ServerId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "ServerId",
                table: "Steps");

            migrationBuilder.AddColumn<Guid>(
                name: "ServerId",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ServerId",
                table: "Jobs",
                column: "ServerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Servers_ServerId",
                table: "Jobs",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "ServerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
