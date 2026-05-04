using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SertecDashboard.Api.Data.Migrations;

/// <summary>
/// Initial schema for MS SQL Server (Somee.com — MS SQL 2022 Express).
/// All table names are lowercase for consistency.
/// </summary>
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                Username     = table.Column<string>(maxLength: 50),
                PasswordHash = table.Column<string>(maxLength: 256),
                Role         = table.Column<string>(maxLength: 30),
                DisplayName  = table.Column<string>(maxLength: 100),
                Email        = table.Column<string>(maxLength: 200, nullable: true),
                CreatedAt    = table.Column<DateTime>(),
            },
            constraints: table => table.PrimaryKey("PK_users", x => x.Username));

        migrationBuilder.CreateTable(
            name: "questions",
            columns: table => new
            {
                Id                 = table.Column<string>(maxLength: 32),
                Text               = table.Column<string>(maxLength: 500),
                Freq               = table.Column<string>(maxLength: 30, defaultValue: "Every 1 hour"),
                Type               = table.Column<string>(maxLength: 30, defaultValue: "production"),
                AlertAnswer        = table.Column<string>(maxLength: 5, defaultValue: "no"),
                YesLabel           = table.Column<string>(maxLength: 50, defaultValue: "Igen"),
                NoLabel            = table.Column<string>(maxLength: 50, defaultValue: "Nem"),
                RequireExplanation = table.Column<string>(maxLength: 10, defaultValue: "no"),
                AnswerWindowMs     = table.Column<long>(defaultValue: 600000L),
                CreatedAt          = table.Column<string>(maxLength: 50),
                LastSent           = table.Column<long>(defaultValue: 0L),
                LastShiftSentMs    = table.Column<long>(defaultValue: 0L),
            },
            constraints: table => table.PrimaryKey("PK_questions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "pendingitems",
            columns: table => new
            {
                Id                 = table.Column<string>(maxLength: 32),
                QuestionId         = table.Column<string>(maxLength: 32),
                Text               = table.Column<string>(maxLength: 500),
                SentAt             = table.Column<string>(maxLength: 50),
                SentAtMs           = table.Column<long>(),
                Deadline           = table.Column<long>(),
                AnswerWindowMs     = table.Column<long>(defaultValue: 600000L),
                AlertAnswer        = table.Column<string>(maxLength: 5, defaultValue: "no"),
                TargetOperator     = table.Column<string>(maxLength: 50, nullable: true),
                TargetOperatorName = table.Column<string>(maxLength: 100, nullable: true),
                YesLabel           = table.Column<string>(maxLength: 50, defaultValue: "Igen"),
                NoLabel            = table.Column<string>(maxLength: 50, defaultValue: "Nem"),
                RequireExplanation = table.Column<string>(maxLength: 10, defaultValue: "no"),
                SentBy             = table.Column<string>(maxLength: 10, defaultValue: "auto"),
                ShiftId            = table.Column<string>(maxLength: 32, nullable: true),
                AlertSent          = table.Column<bool>(defaultValue: false),
                Expired            = table.Column<bool>(defaultValue: false),
            },
            constraints: table => table.PrimaryKey("PK_pendingitems", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_pendingitems_Expired",
            table: "pendingitems",
            column: "Expired");

        migrationBuilder.CreateTable(
            name: "responses",
            columns: table => new
            {
                Id           = table.Column<string>(maxLength: 32),
                Question     = table.Column<string>(maxLength: 500),
                Answer       = table.Column<string>(maxLength: 5),
                Reason       = table.Column<string>(maxLength: 1000),
                Operator     = table.Column<string>(maxLength: 50, nullable: true),
                OperatorName = table.Column<string>(maxLength: 100, defaultValue: "Operator"),
                AlertAnswer  = table.Column<string>(maxLength: 5, defaultValue: "no"),
                Time         = table.Column<string>(maxLength: 50),
                TimeMs       = table.Column<long>(),
                PendingId    = table.Column<string>(maxLength: 32, nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_responses", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_responses_Operator",
            table: "responses",
            column: "Operator");

        migrationBuilder.CreateIndex(
            name: "IX_responses_TimeMs",
            table: "responses",
            column: "TimeMs");

        migrationBuilder.CreateTable(
            name: "shifts",
            columns: table => new
            {
                Id               = table.Column<string>(maxLength: 32),
                OperatorUsername = table.Column<string>(maxLength: 50),
                OperatorName     = table.Column<string>(maxLength: 100),
                Role             = table.Column<string>(maxLength: 30),
                StartTime        = table.Column<long>(),
                StartTimeStr     = table.Column<string>(maxLength: 50),
                Active           = table.Column<bool>(defaultValue: true),
                EndTime          = table.Column<long>(nullable: true),
                EndTimeStr       = table.Column<string>(maxLength: 50, nullable: true),
                EndedBy          = table.Column<string>(maxLength: 50, nullable: true),
                Report           = table.Column<string>(maxLength: 2000, nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_shifts", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_shifts_Active",
            table: "shifts",
            column: "Active");

        migrationBuilder.CreateTable(
            name: "alerts",
            columns: table => new
            {
                Id               = table.Column<string>(maxLength: 32),
                Type             = table.Column<string>(maxLength: 30, defaultValue: "missed_question"),
                OperatorUsername = table.Column<string>(maxLength: 50),
                OperatorName     = table.Column<string>(maxLength: 100),
                QuestionText     = table.Column<string>(maxLength: 500),
                PendingId        = table.Column<string>(maxLength: 32, nullable: true),
                Time             = table.Column<string>(maxLength: 50),
                TimeMs           = table.Column<long>(),
                Acknowledged     = table.Column<bool>(defaultValue: false),
                ShiftId          = table.Column<string>(maxLength: 32, nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_alerts", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_alerts_Acknowledged",
            table: "alerts",
            column: "Acknowledged");

        migrationBuilder.CreateTable(
            name: "machines",
            columns: table => new
            {
                Id   = table.Column<string>(maxLength: 32),
                Name = table.Column<string>(maxLength: 100),
            },
            constraints: table => table.PrimaryKey("PK_machines", x => x.Id));

        migrationBuilder.CreateTable(
            name: "machineparts",
            columns: table => new
            {
                Id        = table.Column<int>().Annotation("SqlServer:Identity", "1, 1"),
                MachineId = table.Column<string>(maxLength: 32),
                Name      = table.Column<string>(maxLength: 200),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_machineparts", x => x.Id);
                table.ForeignKey(
                    name: "FK_machineparts_machines",
                    column: x => x.MachineId,
                    principalTable: "machines",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_machineparts_MachineId",
            table: "machineparts",
            column: "MachineId");

        // Documents — Data stores /docs/{id}.pdf path or a cloud embed URL.
        // No base64 blobs in the DB; files live in wwwroot/docs/ on the server.
        migrationBuilder.CreateTable(
            name: "documents",
            columns: table => new
            {
                Id         = table.Column<string>(maxLength: 32),
                Name       = table.Column<string>(maxLength: 200),
                Size       = table.Column<string>(maxLength: 50),
                Data       = table.Column<string>(),  // nvarchar(max) — file path or embed URL
                UploadedAt = table.Column<string>(maxLength: 50),
            },
            constraints: table => table.PrimaryKey("PK_documents", x => x.Id));

        migrationBuilder.CreateTable(
            name: "passwordresets",
            columns: table => new
            {
                Id            = table.Column<string>(maxLength: 32),
                Username      = table.Column<string>(maxLength: 50),
                DisplayName   = table.Column<string>(maxLength: 100),
                Role          = table.Column<string>(maxLength: 30),
                RequestedAt   = table.Column<string>(maxLength: 50),
                RequestedAtMs = table.Column<long>(),
                Handled       = table.Column<bool>(defaultValue: false),
                HandledAt     = table.Column<string>(maxLength: 50, nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_passwordresets", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_passwordresets_Handled",
            table: "passwordresets",
            column: "Handled");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("machineparts");
        migrationBuilder.DropTable("machines");
        migrationBuilder.DropTable("documents");
        migrationBuilder.DropTable("passwordresets");
        migrationBuilder.DropTable("alerts");
        migrationBuilder.DropTable("shifts");
        migrationBuilder.DropTable("responses");
        migrationBuilder.DropTable("pendingitems");
        migrationBuilder.DropTable("questions");
        migrationBuilder.DropTable("users");
    }
}
