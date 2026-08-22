using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedProjectCodeAndDeptCodeOnEmployeeShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XCompany_Branch_CompanyId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_ZKDevices_SZKDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_SZKDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "SZKDevicesId",
                table: "EmployeeShift");

            migrationBuilder.RenameColumn(
                name: "Company_Branch_Id",
                table: "EmployeeShift",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "EmployeeShift",
                newName: "DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeShift_CompanyId",
                table: "EmployeeShift",
                newName: "IX_EmployeeShift_DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_ProjectId",
                table: "EmployeeShift",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_Department_DepartmentId",
                table: "EmployeeShift",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_Device_DeviceId",
                table: "EmployeeShift",
                column: "DeviceId",
                principalTable: "Device",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_Employee_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_Project_ProjectId",
                table: "EmployeeShift",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_Department_DepartmentId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_Device_DeviceId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_Employee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_Project_ProjectId",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_ProjectId",
                table: "EmployeeShift");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "EmployeeShift",
                newName: "Company_Branch_Id");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "EmployeeShift",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeShift_DeviceId",
                table: "EmployeeShift",
                newName: "IX_EmployeeShift_CompanyId");

            migrationBuilder.AddColumn<Guid>(
                name: "SZKDevicesId",
                table: "EmployeeShift",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_SZKDevicesId",
                table: "EmployeeShift",
                column: "SZKDevicesId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XCompany_Branch_CompanyId",
                table: "EmployeeShift",
                column: "CompanyId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_ZKDevices_SZKDevicesId",
                table: "EmployeeShift",
                column: "SZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id");
        }
    }
}
