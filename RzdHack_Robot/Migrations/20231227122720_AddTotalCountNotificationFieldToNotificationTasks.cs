using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RzdHackRobot.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalCountNotificationFieldToNotificationTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalCountNotification",
                table: "AppNotificationTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCountNotification",
                table: "AppNotificationTasks");
        }
    }
}
