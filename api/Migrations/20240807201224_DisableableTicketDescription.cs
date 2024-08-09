using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planera.Migrations
{
    /// <inheritdoc />
    public partial class DisableableTicketDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableTicketAssignees",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTicketDescriptions",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableTicketAssignees",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EnableTicketDescriptions",
                table: "Projects");
        }
    }
}
