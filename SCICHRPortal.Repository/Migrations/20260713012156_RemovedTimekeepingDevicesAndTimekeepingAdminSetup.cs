using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTimekeepingDevicesAndTimekeepingAdminSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BiometricsLog_TimekeepingDevices_TimekeepingDevicesId",
                table: "BiometricsLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShiftDevice_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropTable(
                name: "TimekeepingAdminSetup");

            migrationBuilder.DropTable(
                name: "TimekeepingDevices");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTimeLog_TimekeepingDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShiftDevice_TimekeepingDevicesId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_TimekeepingDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAttendance_TimekeepingDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropIndex(
                name: "IX_BiometricsLog_TimekeepingDevicesId",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "BiometricsLog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimekeepingDevicesId",
                table: "EmployeeTimeLog",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimekeepingDevicesId",
                table: "EmployeeShiftDevice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimekeepingDevicesId",
                table: "EmployeeShift",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimekeepingDevicesId",
                table: "EmployeeAttendance",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimekeepingDevicesId",
                table: "BiometricsLog",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TimekeepingAdminSetup",
                columns: table => new
                {
                    SetupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdminPassword = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimekeepingAdminSetup", x => x.SetupId);
                });

            migrationBuilder.CreateTable(
                name: "TimekeepingDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SerialNumber = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimekeepingDevices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_TimekeepingDevicesId",
                table: "EmployeeTimeLog",
                column: "TimekeepingDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_TimekeepingDevicesId",
                table: "EmployeeShiftDevice",
                column: "TimekeepingDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_TimekeepingDevicesId",
                table: "EmployeeShift",
                column: "TimekeepingDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_TimekeepingDevicesId",
                table: "EmployeeAttendance",
                column: "TimekeepingDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricsLog_TimekeepingDevicesId",
                table: "BiometricsLog",
                column: "TimekeepingDevicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_BiometricsLog_TimekeepingDevices_TimekeepingDevicesId",
                table: "BiometricsLog",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeAttendance",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeShift",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShiftDevice_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeShiftDevice",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeTimeLog",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
                principalColumn: "Id");
        }
    }
}
