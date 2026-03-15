using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainBotTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "train_bot_tasks",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    telegram_id = table.Column<long>(type: "bigint", nullable: true),
                    from_station = table.Column<string>(type: "text", nullable: false),
                    to_station = table.Column<string>(type: "text", nullable: false),
                    departure_date = table.Column<DateOnly>(type: "date", nullable: false),
                    seats = table.Column<short>(type: "smallint", nullable: false),
                    mode = table.Column<string>(type: "text", nullable: false),
                    seat_mode = table.Column<string>(type: "text", nullable: false),
                    accessible_only = table.Column<bool>(type: "boolean", nullable: false),
                    mode_params = table.Column<string>(type: "jsonb", nullable: false),
                    seat_params = table.Column<string>(type: "jsonb", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_train_bot_tasks", x => x.task_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "train_bot_tasks");
        }
    }
}
