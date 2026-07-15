using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class CorrectedTablesBiometricsLogsAndEmployeeTimeLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SZKDevicesId",
                table: "EmployeeTimeLog",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SZKDevicesId",
                table: "EmployeeShiftDevice",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SZKDevicesId",
                table: "EmployeeShift",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ZKDevicesId",
                table: "EmployeeAttendance",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SZKDevicesId",
                table: "BiometricsLog",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ZKDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IPAddress = table.Column<string>(type: "text", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    APIVersion = table.Column<string>(type: "text", nullable: true),
                    RegistryCode = table.Column<string>(type: "text", nullable: true),
                    FirmwareVersion = table.Column<string>(type: "text", nullable: true),
                    DeviceFunction = table.Column<string>(type: "text", nullable: true),
                    FingerprintSupported = table.Column<bool>(type: "boolean", nullable: false),
                    FaceSupported = table.Column<bool>(type: "boolean", nullable: false),
                    PalmSupported = table.Column<bool>(type: "boolean", nullable: false),
                    LockOpenDuration = table.Column<int>(type: "integer", nullable: false),
                    DeviceInformation = table.Column<string>(type: "text", nullable: true),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    AntiPassback = table.Column<int>(type: "integer", nullable: false),
                    AntiPassbackOn = table.Column<bool>(type: "boolean", nullable: false),
                    KeyMapping = table.Column<string>(type: "text", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZKDevices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_SZKDevicesId",
                table: "EmployeeTimeLog",
                column: "SZKDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_SZKDevicesId",
                table: "EmployeeShiftDevice",
                column: "SZKDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_SZKDevicesId",
                table: "EmployeeShift",
                column: "SZKDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_ZKDevicesId",
                table: "EmployeeAttendance",
                column: "ZKDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricsLog_SZKDevicesId",
                table: "BiometricsLog",
                column: "SZKDevicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_BiometricsLog_ZKDevices_SZKDevicesId",
                table: "BiometricsLog",
                column: "SZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_ZKDevices_ZKDevicesId",
                table: "EmployeeAttendance",
                column: "ZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_ZKDevices_SZKDevicesId",
                table: "EmployeeShift",
                column: "SZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShiftDevice_ZKDevices_SZKDevicesId",
                table: "EmployeeShiftDevice",
                column: "SZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_ZKDevices_SZKDevicesId",
                table: "EmployeeTimeLog",
                column: "SZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BiometricsLog_ZKDevices_SZKDevicesId",
                table: "BiometricsLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_ZKDevices_ZKDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_ZKDevices_SZKDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShiftDevice_ZKDevices_SZKDevicesId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_ZKDevices_SZKDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropTable(
                name: "ZKDevices");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTimeLog_SZKDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShiftDevice_SZKDevicesId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_SZKDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAttendance_ZKDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropIndex(
                name: "IX_BiometricsLog_SZKDevicesId",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "SZKDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "SZKDevicesId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "SZKDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "ZKDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "SZKDevicesId",
                table: "BiometricsLog");
        }
    }
}
