using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProjectCodeToIdForHoliday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectCode",
                table: "Holiday");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Holiday",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holiday_ProjectId",
                table: "Holiday",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Holiday_Project_ProjectId",
                table: "Holiday",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Holiday_Project_ProjectId",
                table: "Holiday");

            migrationBuilder.DropIndex(
                name: "IX_Holiday_ProjectId",
                table: "Holiday");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Holiday");

            migrationBuilder.AddColumn<string>(
                name: "ProjectCode",
                table: "Holiday",
                type: "text",
                nullable: true);
        }
    }
}
