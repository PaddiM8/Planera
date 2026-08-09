using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planera.Api.Migrations
{
    /// <inheritdoc />
    public partial class PinnedProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "PinnedProjects",
                table: "AspNetUsers",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>());
            migrationBuilder.Sql("""
                WITH "projectList" AS (
                  SELECT 
                  "user"."Id" AS "userId",
                  COALESCE(
                    array_remove(array_agg(DISTINCT participant."ProjectId"), NULL),
                    ARRAY[]::text[]
                ) AS "projectIds"
                  FROM "AspNetUsers" "user"
                  LEFT JOIN "ProjectParticipants" participant
                    ON "user"."Id" = participant."UserId"
                  GROUP BY "user"."Id"  
                )
                UPDATE "AspNetUsers" target
                SET "PinnedProjects" = "projectList"."projectIds"
                FROM "projectList"
                WHERE target."Id" = "projectList"."userId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PinnedProjects",
                table: "AspNetUsers");
        }
    }
}
