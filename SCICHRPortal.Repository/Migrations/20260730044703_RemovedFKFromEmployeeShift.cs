using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemovedFKFromEmployeeShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "EmployeeShift",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeShift_EmployeeId",
                table: "EmployeeShift",
                newName: "IX_EmployeeShift_id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XEmployee_id",
                table: "EmployeeShift",
                column: "id",
                principalTable: "XEmployee",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_id",
                table: "EmployeeShift");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "EmployeeShift",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeShift_id",
                table: "EmployeeShift",
                newName: "IX_EmployeeShift_EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");
        }
    }
}
