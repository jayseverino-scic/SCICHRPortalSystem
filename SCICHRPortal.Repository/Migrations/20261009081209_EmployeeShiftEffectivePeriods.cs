using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeShiftEffectivePeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveEndDate",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveStartDate",
                table: "EmployeeShift",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTemporary",
                table: "EmployeeShift",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_EmployeeId_EffectiveStartDate",
                table: "EmployeeShift",
                columns: new[] { "EmployeeId", "EffectiveStartDate" },
                unique: true,
                filter: "\"Deleted\" = false AND \"EffectiveStartDate\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EmployeeShift_EffectivePeriod",
                table: "EmployeeShift",
                sql: "\"EffectiveEndDate\" IS NULL OR \"EffectiveStartDate\" IS NULL OR \"EffectiveEndDate\" > \"EffectiveStartDate\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_EmployeeId_EffectiveStartDate",
                table: "EmployeeShift");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EmployeeShift_EffectivePeriod",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "EffectiveEndDate",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "EffectiveStartDate",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "IsTemporary",
                table: "EmployeeShift");
        }
    }
}
