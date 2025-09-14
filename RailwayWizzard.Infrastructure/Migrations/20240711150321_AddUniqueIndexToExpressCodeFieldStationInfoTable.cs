using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToExpressCodeFieldStationInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppStationInfo_ExpressCode",
                table: "AppStationInfo",
                column: "ExpressCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppStationInfo_ExpressCode",
                table: "AppStationInfo");
        }
    }
}
