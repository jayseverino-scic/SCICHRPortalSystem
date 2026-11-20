using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeDailyAttendanceComputation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeDailyAttendanceComputation",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeLogId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    TimeIn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeOut = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsFlexibleShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoBreak = table.Column<bool>(type: "boolean", nullable: false),
                    ShiftHours = table.Column<double>(type: "double precision", nullable: false),
                    RegularHour = table.Column<double>(type: "double precision", nullable: false),
                    TotalLoggedHours = table.Column<double>(type: "double precision", nullable: false),
                    OTHours = table.Column<double>(type: "double precision", nullable: false),
                    NDHours = table.Column<double>(type: "double precision", nullable: false),
                    ShiftLate = table.Column<double>(type: "double precision", nullable: false),
                    ShiftUndertime = table.Column<double>(type: "double precision", nullable: false),
                    ApprovedOT = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedHolidayOT = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedSPHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedSPHolidayOT = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedRestDay = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedRestDayOT = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDailyAttendanceComputation", x => new { x.ProjectId, x.AttendanceDate, x.TimeLogId });
                    table.ForeignKey(
                        name: "FK_EmployeeDailyAttendanceComputation_EmployeeTimeLog_TimeLogId",
                        column: x => x.TimeLogId,
                        principalTable: "EmployeeTimeLog",
                        principalColumn: "TimeLogId");
                    table.ForeignKey(
                        name: "FK_EmployeeDailyAttendanceComputation_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_EmployeeDailyAttendanceComputation_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDailyAttendanceImportCheckpoint",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DaytimeCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    OvernightCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDailyAttendanceImportCheckpoint", x => new { x.ProjectId, x.AttendanceDate });
                    table.ForeignKey(
                        name: "FK_EmployeeDailyAttendanceImportCheckpoint_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDailyAttendanceComputation_AttendanceDate_ProjectId~",
                table: "EmployeeDailyAttendanceComputation",
                columns: new[] { "AttendanceDate", "ProjectId", "TimeIn", "TimeLogId" },
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDailyAttendanceComputation_EmployeeId",
                table: "EmployeeDailyAttendanceComputation",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDailyAttendanceComputation_TimeLogId",
                table: "EmployeeDailyAttendanceComputation",
                column: "TimeLogId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDailyAttendanceComputation");

            migrationBuilder.DropTable(
                name: "EmployeeDailyAttendanceImportCheckpoint");
        }
    }
}
