using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStationInfoExtendedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppStationInfo_ExpressCode",
                table: "AppStationInfo");

            migrationBuilder.CreateTable(
                name: "AppStationInfoExtended",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExpressCode = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NodeId = table.Column<string>(type: "text", nullable: false),
                    NodeType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStationInfoExtended", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppStationInfoExtended_ExpressCode",
                table: "AppStationInfoExtended",
                column: "ExpressCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStationInfoExtended");

            migrationBuilder.CreateIndex(
                name: "IX_AppStationInfo_ExpressCode",
                table: "AppStationInfo",
                column: "ExpressCode",
                unique: true);
        }
    }
}
