using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedProjectOnHoliday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoLeaveAbsentCountLimit",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "NoTimeLogCountLimit",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "ShiftLateTotalMinuteLimit",
                table: "Shift");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Project",
                newName: "ProjectCode");

            migrationBuilder.AddColumn<string>(
                name: "ProjectCode",
                table: "Holiday",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_ProjectCode",
                table: "Project",
                column: "ProjectCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Project_ProjectCode",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "ProjectCode",
                table: "Holiday");

            migrationBuilder.RenameColumn(
                name: "ProjectCode",
                table: "Project",
                newName: "Code");

            migrationBuilder.AddColumn<int>(
                name: "NoLeaveAbsentCountLimit",
                table: "Shift",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NoTimeLogCountLimit",
                table: "Shift",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShiftLateTotalMinuteLimit",
                table: "Shift",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
