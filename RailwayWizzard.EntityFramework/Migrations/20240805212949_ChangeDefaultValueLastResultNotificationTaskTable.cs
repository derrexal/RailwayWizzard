using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDefaultValueLastResultNotificationTaskTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastResult",
                table: "AppNotificationTasks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastResult",
                table: "AppNotificationTasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
