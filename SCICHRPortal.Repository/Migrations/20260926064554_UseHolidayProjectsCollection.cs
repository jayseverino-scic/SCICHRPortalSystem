using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UseHolidayProjectsCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deploy together with the new API: the old API still writes ProjectId.
            migrationBuilder.Sql("""
                LOCK TABLE "Holiday", "HolidayProject" IN ACCESS EXCLUSIVE MODE;

                -- Preserve parents created by legacy clients after the initial backfill.
                -- Existing assignment history is authoritative and must not be reactivated.
                INSERT INTO "HolidayProject"
                    ("HolidayId", "ProjectId", "Deleted", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt")
                SELECT h."HolidayId", h."ProjectId", FALSE, h."CreatedBy", h."CreatedAt", h."UpdatedBy", h."UpdatedAt"
                FROM "Holiday" h
                WHERE h."ProjectId" IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM "HolidayProject" hp WHERE hp."HolidayId" = h."HolidayId");

                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM "Holiday" h
                        WHERE (h."ProjectId" IS NULL AND EXISTS (
                            SELECT 1 FROM "HolidayProject" hp
                            WHERE hp."HolidayId" = h."HolidayId" AND NOT hp."Deleted"))
                        OR (h."ProjectId" IS NOT NULL AND NOT EXISTS (
                            SELECT 1 FROM "HolidayProject" hp
                            WHERE hp."HolidayId" = h."HolidayId" AND hp."ProjectId" = h."ProjectId" AND NOT hp."Deleted"))
                    ) THEN
                        RAISE EXCEPTION 'Holiday has conflicting legacy and collection project assignments. Reconcile them before applying UseHolidayProjectsCollection.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Holiday_Project_ProjectId",
                table: "Holiday");

            migrationBuilder.DropIndex(
                name: "IX_Holiday_ProjectId",
                table: "Holiday");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Holiday");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Holiday",
                type: "integer",
                nullable: true);

            // Restoring the compatibility field does not discard any memberships.
            // The preceding migration guards against dropping a multi-project join table.
            migrationBuilder.Sql("""
                UPDATE "Holiday" h
                SET "ProjectId" = (
                    SELECT MIN(hp."ProjectId") FROM "HolidayProject" hp
                    WHERE hp."HolidayId" = h."HolidayId" AND NOT hp."Deleted"
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Holiday_ProjectId",
                table: "Holiday",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Holiday_Project_ProjectId",
                table: "Holiday",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");
        }
    }
}
