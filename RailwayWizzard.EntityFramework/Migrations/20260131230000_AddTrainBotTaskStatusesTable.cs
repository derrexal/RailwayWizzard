using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainBotTaskStatusesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "train_bot_task_statuses",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    last_error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_train_bot_task_statuses", x => x.task_id);
                    table.ForeignKey(
                        name: "FK_train_bot_task_statuses_train_bot_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "train_bot_tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_train_bot_task_status_status_allowed",
                table: "train_bot_task_statuses",
                sql: "status IN ('processing','error','end')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "train_bot_task_statuses");
        }
    }
}
