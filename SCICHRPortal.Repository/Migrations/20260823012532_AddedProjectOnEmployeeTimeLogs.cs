using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedProjectOnEmployeeTimeLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_XCompany_Branch_Company_BranchId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_XEmployee_EmployeeId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_ZKDevices_ZKDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XCompany_Branch_XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_id",
                table: "EmployeeTimeLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_ZKDevices_SZKDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTimeLog_SZKDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAttendance_ZKDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "SZKDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "ZKDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.RenameColumn(
                name: "XCompany_BranchId",
                table: "EmployeeTimeLog",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTimeLog_XCompany_BranchId",
                table: "EmployeeTimeLog",
                newName: "IX_EmployeeTimeLog_ProjectId");

            migrationBuilder.RenameColumn(
                name: "Company_BranchId",
                table: "EmployeeAttendance",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAttendance_Company_BranchId",
                table: "EmployeeAttendance",
                newName: "IX_EmployeeAttendance_ProjectId");

            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "EmployeeTimeLog",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "EmployeeAttendance",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_DeviceId",
                table: "EmployeeTimeLog",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_DeviceId",
                table: "EmployeeAttendance",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_Device_DeviceId",
                table: "EmployeeAttendance",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_Employee_EmployeeId",
                table: "EmployeeAttendance",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_Project_ProjectId",
                table: "EmployeeAttendance",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_Device_DeviceId",
                table: "EmployeeTimeLog",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_Employee_id",
                table: "EmployeeTimeLog",
                column: "id",
                principalTable: "Employee",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_Project_ProjectId",
                table: "EmployeeTimeLog",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_Device_DeviceId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_Employee_EmployeeId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_Project_ProjectId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_Device_DeviceId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_Employee_id",
                table: "EmployeeTimeLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_Project_ProjectId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTimeLog_DeviceId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAttendance_DeviceId",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "EmployeeAttendance");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "EmployeeTimeLog",
                newName: "XCompany_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTimeLog_ProjectId",
                table: "EmployeeTimeLog",
                newName: "IX_EmployeeTimeLog_XCompany_BranchId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "EmployeeAttendance",
                newName: "Company_BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeAttendance_ProjectId",
                table: "EmployeeAttendance",
                newName: "IX_EmployeeAttendance_Company_BranchId");

            migrationBuilder.AddColumn<Guid>(
                name: "SZKDevicesId",
                table: "EmployeeTimeLog",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ZKDevicesId",
                table: "EmployeeAttendance",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_SZKDevicesId",
                table: "EmployeeTimeLog",
                column: "SZKDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_ZKDevicesId",
                table: "EmployeeAttendance",
                column: "ZKDevicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_XCompany_Branch_Company_BranchId",
                table: "EmployeeAttendance",
                column: "Company_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_XEmployee_EmployeeId",
                table: "EmployeeAttendance",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_ZKDevices_ZKDevicesId",
                table: "EmployeeAttendance",
                column: "ZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XCompany_Branch_XCompany_BranchId",
                table: "EmployeeTimeLog",
                column: "XCompany_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_id",
                table: "EmployeeTimeLog",
                column: "id",
                principalTable: "XEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_ZKDevices_SZKDevicesId",
                table: "EmployeeTimeLog",
                column: "SZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id");
        }
    }
}
