using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedXEmployeeRelationshipOnEmployeeShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId1",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_EmployeeId1",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "EmployeeShift");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId1",
                table: "EmployeeShift",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_EmployeeId1",
                table: "EmployeeShift",
                column: "EmployeeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId1",
                table: "EmployeeShift",
                column: "EmployeeId1",
                principalTable: "XEmployee",
                principalColumn: "Id");
        }
    }
}
