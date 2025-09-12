using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MovedToNewVersionApplicationPart3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalStation",
                table: "AppNotificationTasks");

            migrationBuilder.DropColumn(
                name: "DepartureStation",
                table: "AppNotificationTasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArrivalStation",
                table: "AppNotificationTasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DepartureStation",
                table: "AppNotificationTasks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
