using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemovedFKEmployeeOnTimeLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");
        }
    }
}
