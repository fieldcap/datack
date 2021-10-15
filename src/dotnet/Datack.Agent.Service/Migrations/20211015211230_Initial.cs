using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datack.Agent.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Settings = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobId);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    ServerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    DbSettings = table.Column<string>(type: "TEXT", nullable: true),
                    Settings = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.ServerId);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingId = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingId);
                });

            migrationBuilder.CreateTable(
                name: "JobRuns",
                columns: table => new
                {
                    JobRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BackupType = table.Column<int>(type: "INTEGER", nullable: false),
                    Started = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Completed = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    IsError = table.Column<bool>(type: "INTEGER", nullable: false),
                    Result = table.Column<string>(type: "TEXT", nullable: true),
                    Settings = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRuns", x => x.JobRunId);
                    table.ForeignKey(
                        name: "FK_JobRuns_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobTasks",
                columns: table => new
                {
                    JobTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Parallel = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    UsePreviousTaskArtifactsFromJobTaskId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Settings = table.Column<string>(type: "TEXT", nullable: true),
                    ServerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTasks", x => x.JobTaskId);
                    table.ForeignKey(
                        name: "FK_JobTasks_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobTasks_JobTasks_UsePreviousTaskArtifactsFromJobTaskId",
                        column: x => x.UsePreviousTaskArtifactsFromJobTaskId,
                        principalTable: "JobTasks",
                        principalColumn: "JobTaskId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobTasks_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobRunTasks",
                columns: table => new
                {
                    JobRunTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobRunId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Started = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Completed = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    ItemName = table.Column<string>(type: "TEXT", nullable: true),
                    TaskOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsError = table.Column<bool>(type: "INTEGER", nullable: false),
                    Result = table.Column<string>(type: "TEXT", nullable: true),
                    ResultArtifact = table.Column<string>(type: "TEXT", nullable: true),
                    Settings = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRunTasks", x => x.JobRunTaskId);
                    table.ForeignKey(
                        name: "FK_JobRunTasks_JobRuns_JobRunId",
                        column: x => x.JobRunId,
                        principalTable: "JobRuns",
                        principalColumn: "JobRunId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobRunTasks_JobTasks_JobTaskId",
                        column: x => x.JobTaskId,
                        principalTable: "JobTasks",
                        principalColumn: "JobTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobRunTaskLogs",
                columns: table => new
                {
                    JobRunTaskLogId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobRunTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    IsError = table.Column<bool>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRunTaskLogs", x => x.JobRunTaskLogId);
                    table.ForeignKey(
                        name: "FK_JobRunTaskLogs_JobRunTasks_JobRunTaskId",
                        column: x => x.JobRunTaskId,
                        principalTable: "JobRunTasks",
                        principalColumn: "JobRunTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobRuns_JobId",
                table: "JobRuns",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRunTaskLogs_JobRunTaskId",
                table: "JobRunTaskLogs",
                column: "JobRunTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRunTasks_JobRunId",
                table: "JobRunTasks",
                column: "JobRunId");

            migrationBuilder.CreateIndex(
                name: "IX_JobRunTasks_JobTaskId",
                table: "JobRunTasks",
                column: "JobTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTasks_JobId",
                table: "JobTasks",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTasks_ServerId",
                table: "JobTasks",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTasks_UsePreviousTaskArtifactsFromJobTaskId",
                table: "JobTasks",
                column: "UsePreviousTaskArtifactsFromJobTaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobRunTaskLogs");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "JobRunTasks");

            migrationBuilder.DropTable(
                name: "JobRuns");

            migrationBuilder.DropTable(
                name: "JobTasks");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
