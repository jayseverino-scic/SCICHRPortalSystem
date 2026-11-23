using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UserEmailUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "User",
                type: "text",
                nullable: true,
                computedColumnSql: "lower(btrim(\"Email\"))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_NormalizedEmail",
                table: "User",
                column: "NormalizedEmail",
                unique: true,
                filter: "\"Deleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_NormalizedEmail",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "User");
        }
    }
}
