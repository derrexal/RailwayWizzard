using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedFieldsToNotificationTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFrom",
                table: "AppNotificationTasks");

            migrationBuilder.DropColumn(
                name: "TimeFrom",
                table: "AppNotificationTasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateFrom",
                table: "AppNotificationTasks",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TimeFrom",
                table: "AppNotificationTasks",
                type: "text",
                nullable: true);
        }
    }
}
