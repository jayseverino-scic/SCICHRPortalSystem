using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLeaveTypeFromOfficialBusiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfficialBusiness_LeaveType_LeaveTypeId",
                table: "OfficialBusiness");

            migrationBuilder.DropIndex(
                name: "IX_OfficialBusiness_LeaveTypeId",
                table: "OfficialBusiness");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                table: "OfficialBusiness");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Removed assignments cannot be reconstructed for existing OB records.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "OfficialBusiness") THEN
                        RAISE EXCEPTION 'Cannot restore OB leave-type assignments. Restore a database backup to roll back populated records.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AddColumn<int>(
                name: "LeaveTypeId",
                table: "OfficialBusiness",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OfficialBusiness_LeaveTypeId",
                table: "OfficialBusiness",
                column: "LeaveTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OfficialBusiness_LeaveType_LeaveTypeId",
                table: "OfficialBusiness",
                column: "LeaveTypeId",
                principalTable: "LeaveType",
                principalColumn: "LeaveTypeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
