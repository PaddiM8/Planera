using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planera.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Tickets_TicketId_TicketProjectId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_TicketId_TicketProjectId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "TicketProjectId",
                table: "Notes");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Notes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TicketId_ProjectId",
                table: "Notes",
                columns: new[] { "TicketId", "ProjectId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Tickets_TicketId_ProjectId",
                table: "Notes",
                columns: new[] { "TicketId", "ProjectId" },
                principalTable: "Tickets",
                principalColumns: new[] { "Id", "ProjectId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Tickets_TicketId_ProjectId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_TicketId_ProjectId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Notes");

            migrationBuilder.AddColumn<int>(
                name: "TicketProjectId",
                table: "Notes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TicketId_TicketProjectId",
                table: "Notes",
                columns: new[] { "TicketId", "TicketProjectId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Tickets_TicketId_TicketProjectId",
                table: "Notes",
                columns: new[] { "TicketId", "TicketProjectId" },
                principalTable: "Tickets",
                principalColumns: new[] { "Id", "ProjectId" });
        }
    }
}
