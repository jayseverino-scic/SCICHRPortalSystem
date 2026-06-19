using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangesOnBreaksandShiftsRelatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakLateMinuteGracePeriod",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "BreakLateTotalMinuteLimit",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "ShiftEnd",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "ShiftStart",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "BreakEnd",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "BreakIn",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "BreakOut",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "BreakStart",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "DateBreakIn",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "DateBreakOut",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "IsFlexibleBreak",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "IsFlexibleBreak",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "ShiftEnd",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "ShiftStart",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "IsFlexibleBreak",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "ShiftEnd",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "ShiftStart",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "BreakEnd",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "BreakIn",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "BreakLate",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "BreakOut",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "BreakStart",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "BreakUndertime",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "IsFlexibleBreak",
                table: "EmployeeAttendance");

            migrationBuilder.RenameColumn(
                name: "BreakStart",
                table: "Shift",
                newName: "WednesdayShiftStart");

            migrationBuilder.RenameColumn(
                name: "BreakEnd",
                table: "Shift",
                newName: "WednesdayShiftEnd");

            migrationBuilder.RenameColumn(
                name: "BreakStart",
                table: "EmployeeShiftDevice",
                newName: "WednesdayShiftStart");

            migrationBuilder.RenameColumn(
                name: "BreakEnd",
                table: "EmployeeShiftDevice",
                newName: "WednesdayShiftEnd");

            migrationBuilder.RenameColumn(
                name: "BreakStart",
                table: "EmployeeShift",
                newName: "WednesdayShiftStart");

            migrationBuilder.RenameColumn(
                name: "BreakEnd",
                table: "EmployeeShift",
                newName: "WednesdayShiftEnd");

            migrationBuilder.AddColumn<DateTime>(
                name: "FridayShiftEnd",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FridayShiftStart",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MondayShiftEnd",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MondayShiftStart",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaturdayShiftEnd",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaturdayShiftStart",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SundayShiftEnd",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SundayShiftStart",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThursdayShiftEnd",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThursdayShiftStart",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TuesdayShiftEnd",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TuesdayShiftStart",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FridayShiftEnd",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FridayShiftStart",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MondayShiftEnd",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MondayShiftStart",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaturdayShiftEnd",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaturdayShiftStart",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SundayShiftEnd",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SundayShiftStart",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThursdayShiftEnd",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThursdayShiftStart",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TuesdayShiftEnd",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TuesdayShiftStart",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FridayShiftEnd",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FridayShiftStart",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MondayShiftEnd",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MondayShiftStart",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaturdayShiftEnd",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaturdayShiftStart",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SundayShiftEnd",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SundayShiftStart",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThursdayShiftEnd",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ThursdayShiftStart",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TuesdayShiftEnd",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TuesdayShiftStart",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShiftStart",
                table: "EmployeeAttendance",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShiftEnd",
                table: "EmployeeAttendance",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FridayShiftEnd",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "FridayShiftStart",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "MondayShiftEnd",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "MondayShiftStart",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "SaturdayShiftEnd",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "SaturdayShiftStart",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "SundayShiftEnd",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "SundayShiftStart",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "ThursdayShiftEnd",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "ThursdayShiftStart",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "TuesdayShiftEnd",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "TuesdayShiftStart",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "FridayShiftEnd",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "FridayShiftStart",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "MondayShiftEnd",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "MondayShiftStart",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "SaturdayShiftEnd",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "SaturdayShiftStart",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "SundayShiftEnd",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "SundayShiftStart",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "ThursdayShiftEnd",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "ThursdayShiftStart",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "TuesdayShiftEnd",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "TuesdayShiftStart",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "FridayShiftEnd",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "FridayShiftStart",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "MondayShiftEnd",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "MondayShiftStart",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "SaturdayShiftEnd",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "SaturdayShiftStart",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "SundayShiftEnd",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "SundayShiftStart",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "ThursdayShiftEnd",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "ThursdayShiftStart",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "TuesdayShiftEnd",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "TuesdayShiftStart",
                table: "EmployeeShift");

            migrationBuilder.RenameColumn(
                name: "WednesdayShiftStart",
                table: "Shift",
                newName: "BreakStart");

            migrationBuilder.RenameColumn(
                name: "WednesdayShiftEnd",
                table: "Shift",
                newName: "BreakEnd");

            migrationBuilder.RenameColumn(
                name: "WednesdayShiftStart",
                table: "EmployeeShiftDevice",
                newName: "BreakStart");

            migrationBuilder.RenameColumn(
                name: "WednesdayShiftEnd",
                table: "EmployeeShiftDevice",
                newName: "BreakEnd");

            migrationBuilder.RenameColumn(
                name: "WednesdayShiftStart",
                table: "EmployeeShift",
                newName: "BreakStart");

            migrationBuilder.RenameColumn(
                name: "WednesdayShiftEnd",
                table: "EmployeeShift",
                newName: "BreakEnd");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftEnd",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftStart",
                table: "Shift",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BreakEnd",
                table: "EmployeeTimeLog",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BreakIn",
                table: "EmployeeTimeLog",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BreakOut",
                table: "EmployeeTimeLog",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BreakStart",
                table: "EmployeeTimeLog",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateBreakIn",
                table: "EmployeeTimeLog",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateBreakOut",
                table: "EmployeeTimeLog",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlexibleBreak",
                table: "EmployeeTimeLog",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlexibleBreak",
                table: "EmployeeShiftDevice",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftEnd",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftStart",
                table: "EmployeeShiftDevice",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsFlexibleBreak",
                table: "EmployeeShift",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftEnd",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftStart",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShiftStart",
                table: "EmployeeAttendance",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ShiftEnd",
                table: "EmployeeAttendance",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BreakEnd",
                table: "EmployeeAttendance",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BreakIn",
                table: "EmployeeAttendance",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BreakLate",
                table: "EmployeeAttendance",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BreakOut",
                table: "EmployeeAttendance",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BreakStart",
                table: "EmployeeAttendance",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BreakUndertime",
                table: "EmployeeAttendance",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlexibleBreak",
                table: "EmployeeAttendance",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
