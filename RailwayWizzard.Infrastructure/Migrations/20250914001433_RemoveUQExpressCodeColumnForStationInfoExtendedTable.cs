using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUQExpressCodeColumnForStationInfoExtendedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppStationInfoExtended_ExpressCode",
                table: "AppStationInfoExtended");

            migrationBuilder.CreateIndex(
                name: "IX_AppStationInfoExtended_ExpressCode",
                table: "AppStationInfoExtended",
                column: "ExpressCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppStationInfoExtended_ExpressCode",
                table: "AppStationInfoExtended");

            migrationBuilder.CreateIndex(
                name: "IX_AppStationInfoExtended_ExpressCode",
                table: "AppStationInfoExtended",
                column: "ExpressCode",
                unique: true);
        }
    }
}
