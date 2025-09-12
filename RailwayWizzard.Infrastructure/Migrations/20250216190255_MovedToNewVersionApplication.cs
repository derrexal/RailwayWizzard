using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MovedToNewVersionApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalStationCode",
                table: "AppNotificationTasks");

            migrationBuilder.DropColumn(
                name: "DepartureStationCode",
                table: "AppNotificationTasks");

            migrationBuilder.DropColumn(
                name: "LastResult",
                table: "AppNotificationTasks");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AppNotificationTasks",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "IdTg",
                table: "AppUsers",
                newName: "TelegramUserId");

            migrationBuilder.RenameColumn(
                name: "StationName",
                table: "AppStationInfo",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "IsWorked",
                table: "AppNotificationTasks",
                newName: "IsProcess");

            migrationBuilder.AddColumn<bool>(
                name: "HasBlockedBot",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ArrivalStationId",
                table: "AppNotificationTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartureStationId",
                table: "AppNotificationTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AppMessageOutbox",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotificationTaskId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsSending = table.Column<bool>(type: "boolean", nullable: false),
                    Send = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMessageOutbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppNotificationTaskResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotificationTaskId = table.Column<int>(type: "integer", nullable: false),
                    Started = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Finished = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ResultStatus = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    HashResult = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotificationTaskResult", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMessageOutbox");

            migrationBuilder.DropTable(
                name: "AppNotificationTaskResult");

            migrationBuilder.DropColumn(
                name: "HasBlockedBot",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ArrivalStationId",
                table: "AppNotificationTasks");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "AppNotificationTasks",
                newName: "UserId");

            migrationBuilder.DropColumn(
                name: "DepartureStationId",
                table: "AppNotificationTasks");

            migrationBuilder.RenameColumn(
                name: "TelegramUserId",
                table: "AppUsers",
                newName: "IdTg");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AppStationInfo",
                newName: "StationName");

            migrationBuilder.RenameColumn(
                name: "IsProcess",
                table: "AppNotificationTasks",
                newName: "IsWorked");

            migrationBuilder.AddColumn<long>(
                name: "ArrivalStationCode",
                table: "AppNotificationTasks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DepartureStationCode",
                table: "AppNotificationTasks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "LastResult",
                table: "AppNotificationTasks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
