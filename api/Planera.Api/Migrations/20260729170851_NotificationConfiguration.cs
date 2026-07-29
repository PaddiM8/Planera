using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Planera.Api.Data.Notifications;

#nullable disable

namespace Planera.Api.Migrations
{
    /// <inheritdoc />
    public partial class NotificationConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnabledNotificationKinds",
                table: "ProjectParticipants",
                type: "integer",
                nullable: false,
                defaultValue: NotificationKinds.Core | NotificationKinds.DeadlineMyTicket);

            migrationBuilder.AddColumn<List<string>>(
                name: "AssignedUserIds",
                table: "NotificationQueue",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationKind",
                table: "NotificationQueue",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EnabledNotificationKinds",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: NotificationKinds.Core | NotificationKinds.DeadlineMyTicket | NotificationKinds.DeadlineOtherTicket);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnabledNotificationKinds",
                table: "ProjectParticipants");

            migrationBuilder.DropColumn(
                name: "AssignedUserIds",
                table: "NotificationQueue");

            migrationBuilder.DropColumn(
                name: "NotificationKind",
                table: "NotificationQueue");

            migrationBuilder.DropColumn(
                name: "EnabledNotificationKinds",
                table: "AspNetUsers");
        }
    }
}
