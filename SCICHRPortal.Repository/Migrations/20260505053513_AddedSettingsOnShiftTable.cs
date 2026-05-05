using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedSettingsOnShiftTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreakLateMinuteGracePeriod",
                table: "Shift",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BreakLateTotalMinuteLimit",
                table: "Shift",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NoLeaveAbsentCountLimit",
                table: "Shift",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NoTimeLogCountLimit",
                table: "Shift",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RestDays",
                table: "Shift",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShiftLateMinuteGracePeriod",
                table: "Shift",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShiftLateTotalMinuteLimit",
                table: "Shift",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakLateMinuteGracePeriod",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "BreakLateTotalMinuteLimit",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "NoLeaveAbsentCountLimit",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "NoTimeLogCountLimit",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "RestDays",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "ShiftLateMinuteGracePeriod",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "ShiftLateTotalMinuteLimit",
                table: "Shift");
        }
    }
}
