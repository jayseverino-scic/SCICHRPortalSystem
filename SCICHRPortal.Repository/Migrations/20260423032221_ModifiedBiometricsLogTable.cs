using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedBiometricsLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AntiPass",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "DateTimeLog",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "GMNo",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "ProxyWork",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "TMNo",
                table: "BiometricsLog");

            migrationBuilder.RenameColumn(
                name: "Mode",
                table: "BiometricsLog",
                newName: "PersonnelId");

            migrationBuilder.RenameColumn(
                name: "InOut",
                table: "BiometricsLog",
                newName: "LogType");

            migrationBuilder.RenameColumn(
                name: "EmployeeNo",
                table: "BiometricsLog",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "EmployeeName",
                table: "BiometricsLog",
                newName: "FirstName");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "BiometricsLog",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "BiometricsLog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Time",
                table: "BiometricsLog",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "BiometricsLog");

            migrationBuilder.RenameColumn(
                name: "PersonnelId",
                table: "BiometricsLog",
                newName: "Mode");

            migrationBuilder.RenameColumn(
                name: "LogType",
                table: "BiometricsLog",
                newName: "InOut");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "BiometricsLog",
                newName: "EmployeeNo");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "BiometricsLog",
                newName: "EmployeeName");

            migrationBuilder.AddColumn<int>(
                name: "AntiPass",
                table: "BiometricsLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTimeLog",
                table: "BiometricsLog",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "GMNo",
                table: "BiometricsLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProxyWork",
                table: "BiometricsLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TMNo",
                table: "BiometricsLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
