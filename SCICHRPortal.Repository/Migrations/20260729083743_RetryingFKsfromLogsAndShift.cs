using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RetryingFKsfromLogsAndShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_Shift_ShiftId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XCompany_Branch_CompanyId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_CompanyId",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "EmployeeShift");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_Company_Branch_Id",
                table: "EmployeeShift",
                column: "Company_Branch_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_Shift_ShiftId",
                table: "EmployeeShift",
                column: "ShiftId",
                principalTable: "Shift",
                principalColumn: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XCompany_Branch_Company_Branch_Id",
                table: "EmployeeShift",
                column: "Company_Branch_Id",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_Shift_ShiftId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XCompany_Branch_Company_Branch_Id",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_Company_Branch_Id",
                table: "EmployeeShift");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "EmployeeShift",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_CompanyId",
                table: "EmployeeShift",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_Shift_ShiftId",
                table: "EmployeeShift",
                column: "ShiftId",
                principalTable: "Shift",
                principalColumn: "ShiftId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XCompany_Branch_CompanyId",
                table: "EmployeeShift",
                column: "CompanyId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
