using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedDepartmentRelationshipOnEmployeeShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XDepartment_DepartmentId",
                table: "EmployeeShift");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XDepartment_DepartmentId",
                table: "EmployeeShift",
                column: "DepartmentId",
                principalTable: "XDepartment",
                principalColumn: "Id");
        }
    }
}
