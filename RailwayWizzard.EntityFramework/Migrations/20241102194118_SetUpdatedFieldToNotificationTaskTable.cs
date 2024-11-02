using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class SetUpdatedFieldToNotificationTaskTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"UPDATE AppNotificationTasks SET Updated = {DateTime.MinValue} WHERE Updated IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"UPDATE AppNotificationTasks SET Updated = NULL WHERE Id > 0");
        }
    }
}
