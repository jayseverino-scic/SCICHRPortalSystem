using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeviceAndProjectOnTimeLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "TimekeepingDevices");

            migrationBuilder.AddColumn<string>(
                name: "DeviceTimeIn",
                table: "EmployeeTimeLog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceTimeOut",
                table: "EmployeeTimeLog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjecTimeIn",
                table: "EmployeeTimeLog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectTimeOut",
                table: "EmployeeTimeLog",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceTimeIn",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "DeviceTimeOut",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "ProjecTimeIn",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "ProjectTimeOut",
                table: "EmployeeTimeLog");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "TimekeepingDevices",
                type: "text",
                nullable: true);
        }
    }
}
