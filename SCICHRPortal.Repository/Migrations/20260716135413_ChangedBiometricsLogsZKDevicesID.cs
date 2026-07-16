using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangedBiometricsLogsZKDevicesID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BiometricsLog_ZKDevices_SZKDevicesId",
                table: "BiometricsLog");

            migrationBuilder.AddForeignKey(
                name: "FK_BiometricsLog_ZKDevices_SZKDevicesId",
                table: "BiometricsLog",
                column: "SZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BiometricsLog_ZKDevices_SZKDevicesId",
                table: "BiometricsLog");

            migrationBuilder.AddForeignKey(
                name: "FK_BiometricsLog_ZKDevices_SZKDevicesId",
                table: "BiometricsLog",
                column: "SZKDevicesId",
                principalTable: "ZKDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
