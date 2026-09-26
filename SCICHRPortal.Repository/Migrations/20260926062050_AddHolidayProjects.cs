using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidayProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HolidayProject",
                columns: table => new
                {
                    HolidayId = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayProject", x => new { x.HolidayId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_HolidayProject_Holiday_HolidayId",
                        column: x => x.HolidayId,
                        principalTable: "Holiday",
                        principalColumn: "HolidayId");
                    table.ForeignKey(
                        name: "FK_HolidayProject_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HolidayProject_ProjectId",
                table: "HolidayProject",
                column: "ProjectId");

            // Preserve every existing Holiday ID and its project assignment, including
            // deleted parents. Parent deletion and assignment deletion are independent.
            migrationBuilder.Sql("""
                INSERT INTO "HolidayProject"
                    ("HolidayId", "ProjectId", "Deleted", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
                SELECT "HolidayId", "ProjectId", FALSE, "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt"
                FROM "Holiday"
                WHERE "ProjectId" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT "HolidayId" FROM "HolidayProject"
                        WHERE NOT "Deleted"
                        GROUP BY "HolidayId" HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Cannot roll back Holidays with multiple project assignments. Reduce each Holiday to one project or All Projects first.';
                    END IF;
                END $$;
                """);
            migrationBuilder.DropTable(
                name: "HolidayProject");
        }
    }
}
