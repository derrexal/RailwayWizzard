using Microsoft.EntityFrameworkCore.Migrations;

namespace RailwayWizzard.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCarTypesFieldNotificationTaskTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""AppNotificationTasks""
                SET ""CarTypes"" = ARRAY(
                    SELECT unnest(
                        CASE 
                            WHEN elem = 1 THEN ARRAY[1, 2]
                            WHEN elem = 2 THEN ARRAY[3, 4, 5, 6]
                            WHEN elem = 3 THEN ARRAY[7, 8]
                            WHEN elem = 4 THEN ARRAY[9]
                            ELSE ARRAY[elem]
                        END
                    )
                    FROM unnest(""CarTypes"") AS elem
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""AppNotificationTasks""
                SET ""CarTypes"" = ARRAY(
                    SELECT DISTINCT unnest(
                        CASE 
                            WHEN elem = 1 THEN ARRAY[1]
                            WHEN elem = 2 THEN ARRAY[1]
                            WHEN elem = 3 THEN ARRAY[2]
                            WHEN elem = 4 THEN ARRAY[2]
                            WHEN elem = 5 THEN ARRAY[2]
                            WHEN elem = 6 THEN ARRAY[2]
                            WHEN elem = 7 THEN ARRAY[3]
                            WHEN elem = 8 THEN ARRAY[3]
                            WHEN elem = 9 THEN ARRAY[4]
                            ELSE ARRAY[elem]
                        END
                    )
                    FROM unnest(""CarTypes"") AS elem
                );
            ");
        }
    }
}
