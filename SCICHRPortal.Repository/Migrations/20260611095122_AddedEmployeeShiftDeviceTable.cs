using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmployeeShiftDeviceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeShiftDevice",
                columns: table => new
                {
                    AssignedShiftId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    ShiftDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BreakStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    BreakEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsFlexibleShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsFlexibleBreak = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoBreak = table.Column<bool>(type: "boolean", nullable: false),
                    Devicename = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeShiftDevice", x => x.AssignedShiftId);
                    table.ForeignKey(
                        name: "FK_EmployeeShiftDevice_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_EmployeeShiftDevice_Shift_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shift",
                        principalColumn: "ShiftId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_EmployeeId",
                table: "EmployeeShiftDevice",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_ShiftId",
                table: "EmployeeShiftDevice",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeShiftDevice");
        }
    }
}
