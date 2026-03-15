using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToMessageOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppMessageOutbox_NotificationTaskId_Message",
                table: "AppMessageOutbox",
                columns: new[] { "NotificationTaskId", "Message" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppMessageOutbox_NotificationTaskId_Message",
                table: "AppMessageOutbox");
        }
    }
}
