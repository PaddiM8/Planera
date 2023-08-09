using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planera.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketAssignees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketUser");

            migrationBuilder.CreateTable(
                name: "TicketAssignees",
                columns: table => new
                {
                    TicketId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    TicketProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAssignees", x => new { x.UserId, x.TicketId, x.TicketProjectId });
                    table.ForeignKey(
                        name: "FK_TicketAssignees_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketAssignees_Tickets_TicketId_TicketProjectId",
                        columns: x => new { x.TicketId, x.TicketProjectId },
                        principalTable: "Tickets",
                        principalColumns: new[] { "Id", "ProjectId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignees_TicketId_TicketProjectId",
                table: "TicketAssignees",
                columns: new[] { "TicketId", "TicketProjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketAssignees");

            migrationBuilder.CreateTable(
                name: "TicketUser",
                columns: table => new
                {
                    AssigneesId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedTicketsId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedTicketsProjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketUser", x => new { x.AssigneesId, x.AssignedTicketsId, x.AssignedTicketsProjectId });
                    table.ForeignKey(
                        name: "FK_TicketUser_AspNetUsers_AssigneesId",
                        column: x => x.AssigneesId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketUser_Tickets_AssignedTicketsId_AssignedTicketsProjectId",
                        columns: x => new { x.AssignedTicketsId, x.AssignedTicketsProjectId },
                        principalTable: "Tickets",
                        principalColumns: new[] { "Id", "ProjectId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketUser_AssignedTicketsId_AssignedTicketsProjectId",
                table: "TicketUser",
                columns: new[] { "AssignedTicketsId", "AssignedTicketsProjectId" });
        }
    }
}
