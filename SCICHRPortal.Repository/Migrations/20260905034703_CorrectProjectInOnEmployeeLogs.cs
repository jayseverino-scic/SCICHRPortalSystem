using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class CorrectProjectInOnEmployeeLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjecTimeIn",
                table: "EmployeeTimeLog",
                newName: "ProjectTimeIn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjectTimeIn",
                table: "EmployeeTimeLog",
                newName: "ProjecTimeIn");
        }
    }
}
