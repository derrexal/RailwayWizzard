using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.App.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTotalCountNotificationField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCountNotification",
                table: "AppNotificationTasks");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateFrom",
                table: "AppNotificationTasks",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationTime",
                table: "AppNotificationTasks",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateFrom",
                table: "AppNotificationTasks",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationTime",
                table: "AppNotificationTasks",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<int>(
                name: "TotalCountNotification",
                table: "AppNotificationTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
