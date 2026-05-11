using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimekeepingSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakLateMinuteGracePeriod",
                table: "TimekeepingAdminSetup");

            migrationBuilder.DropColumn(
                name: "BreakLateTotalMinuteLimit",
                table: "TimekeepingAdminSetup");

            migrationBuilder.DropColumn(
                name: "NoLeaveAbsentCountLimit",
                table: "TimekeepingAdminSetup");

            migrationBuilder.DropColumn(
                name: "NoTimeLogCountLimit",
                table: "TimekeepingAdminSetup");

            migrationBuilder.DropColumn(
                name: "RestDays",
                table: "TimekeepingAdminSetup");

            migrationBuilder.DropColumn(
                name: "ShiftLateMinuteGracePeriod",
                table: "TimekeepingAdminSetup");

            migrationBuilder.DropColumn(
                name: "ShiftLateTotalMinuteLimit",
                table: "TimekeepingAdminSetup");

            migrationBuilder.AddColumn<string>(
                name: "AdminPassword",
                table: "TimekeepingAdminSetup",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminPassword",
                table: "TimekeepingAdminSetup");

            migrationBuilder.AddColumn<int>(
                name: "BreakLateMinuteGracePeriod",
                table: "TimekeepingAdminSetup",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BreakLateTotalMinuteLimit",
                table: "TimekeepingAdminSetup",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NoLeaveAbsentCountLimit",
                table: "TimekeepingAdminSetup",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NoTimeLogCountLimit",
                table: "TimekeepingAdminSetup",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RestDays",
                table: "TimekeepingAdminSetup",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ShiftLateMinuteGracePeriod",
                table: "TimekeepingAdminSetup",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShiftLateTotalMinuteLimit",
                table: "TimekeepingAdminSetup",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
