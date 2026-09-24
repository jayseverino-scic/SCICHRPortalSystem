using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class CorrectProjectCodeToCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjectCode",
                table: "Project",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_Project_ProjectCode",
                table: "Project",
                newName: "IX_Project_Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Project",
                newName: "ProjectCode");

            migrationBuilder.RenameIndex(
                name: "IX_Project_Code",
                table: "Project",
                newName: "IX_Project_ProjectCode");
        }
    }
}
