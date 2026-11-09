using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planera.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrphanedTicketNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deleting a ticket used to leave its deadline notifications behind,
            // which made creating a new ticket with the same id fail.
            migrationBuilder.Sql("""
                DELETE FROM "NotificationQueue" AS queue
                WHERE queue."TargetKind" = 1
                  AND NOT EXISTS (
                    SELECT 1 FROM "Tickets" AS ticket
                    WHERE ticket."ProjectId" = queue."TargetId"
                      AND ticket."Id"::text = queue."ObjectId"
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
