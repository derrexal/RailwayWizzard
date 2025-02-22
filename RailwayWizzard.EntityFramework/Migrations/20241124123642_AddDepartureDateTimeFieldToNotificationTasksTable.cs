using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartureDateTimeFieldToNotificationTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TimeFrom",
                table: "AppNotificationTasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DateFrom",
                table: "AppNotificationTasks",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureDateTime",
                table: "AppNotificationTasks",
                type: "timestamp without time zone",
                nullable: true);
   
            migrationBuilder.Sql(@"
                UPDATE ""AppNotificationTasks""
                SET ""DepartureDateTime"" = ""DateFrom"" + CAST(""TimeFrom"" AS INTERVAL)
            ");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DepartureDateTime",
                table: "AppNotificationTasks",
                type: "timestamp without time zone",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TimeFrom",
                table: "AppNotificationTasks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateFrom",
                table: "AppNotificationTasks",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.DropColumn(
                name: "DepartureDateTime",
                table: "AppNotificationTasks");
        }
    }
}
