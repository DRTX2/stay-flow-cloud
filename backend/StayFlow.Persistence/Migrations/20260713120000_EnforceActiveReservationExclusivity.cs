using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayFlow.Persistence.Migrations;

[DbContext(typeof(StayFlowDbContext))]
[Migration("20260713120000_EnforceActiveReservationExclusivity")]
public sealed class EnforceActiveReservationExclusivity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");
        migrationBuilder.Sql(
            """
            ALTER TABLE "Reservations"
            ADD CONSTRAINT "EX_Reservations_ActiveRoomPeriod"
            EXCLUDE USING gist
            (
                "RoomId" WITH =,
                daterange("CheckIn", "CheckOut", '[)') WITH &&
            )
            WHERE
            (
                "IsDeleted" = FALSE
                AND "Status" IN ('Pending', 'Confirmed', 'CheckedIn')
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "Reservations"
            DROP CONSTRAINT "EX_Reservations_ActiveRoomPeriod";
            """);
    }
}
