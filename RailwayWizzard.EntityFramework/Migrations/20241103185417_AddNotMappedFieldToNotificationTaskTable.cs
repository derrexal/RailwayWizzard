using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddNotMappedFieldToNotificationTaskTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "TrainNumber",
                table: "AppNotificationTasks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalStationCode",
                table: "AppNotificationTasks");

            migrationBuilder.DropColumn(
                name: "DepartureStationCode",
                table: "AppNotificationTasks");

            migrationBuilder.DropColumn(
                name: "TrainNumber",
                table: "AppNotificationTasks");
        }
    }
}
