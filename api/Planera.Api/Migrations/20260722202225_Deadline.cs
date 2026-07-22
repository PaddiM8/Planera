using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planera.Api.Migrations
{
    /// <inheritdoc />
    public partial class Deadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableTicketDeadlines",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "EnableTicketDeadlines",
                table: "Projects");
        }
    }
}
