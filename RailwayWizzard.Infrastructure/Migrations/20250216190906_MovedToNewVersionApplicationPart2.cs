using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayWizzard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MovedToNewVersionApplicationPart2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""AppNotificationTasks""
                SET ""DepartureStationId"" = (
                    SELECT ""Id"" FROM ""AppStationInfo"" 
                    WHERE ""AppStationInfo"".""Name"" = ""AppNotificationTasks"".""DepartureStation"");
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""AppNotificationTasks""
                SET ""ArrivalStationId"" = (
                    SELECT ""Id"" FROM ""AppStationInfo"" 
                    WHERE ""AppStationInfo"".""Name"" = ""AppNotificationTasks"".""ArrivalStation"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
