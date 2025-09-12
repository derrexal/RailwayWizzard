using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsStoppedFieldToNotificationTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStopped",
                table: "AppNotificationTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStopped",
                table: "AppNotificationTasks");
        }
    }
}
