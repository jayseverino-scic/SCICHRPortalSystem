using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ReovedForeignTablesFromEmployeeShiftAndTimeLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BiometricsLog_XCompany_Branch_XCompany_BranchId",
                table: "BiometricsLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_XCompany_Branch_Company_BranchId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XCompany_Branch_Company_Branch_Id",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShiftDevice_XCompany_Branch_BranchId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XCompany_Branch_XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_id",
                table: "EmployeeTimeLog");

            migrationBuilder.DropForeignKey(
                name: "FK_XEmployee_XCompany_Branch_Company_BranchId",
                table: "XEmployee");

            migrationBuilder.DropTable(
                name: "XCompany_Branch");

            migrationBuilder.DropIndex(
                name: "IX_XEmployee_Company_BranchId",
                table: "XEmployee");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTimeLog_XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShiftDevice_BranchId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_Company_Branch_Id",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAttendance_Company_BranchId",
                table: "EmployeeAttendance");

            migrationBuilder.DropIndex(
                name: "IX_BiometricsLog_XCompany_BranchId",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "Company_BranchId",
                table: "XEmployee");

            migrationBuilder.DropColumn(
                name: "XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "Company_BranchId",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "XCompany_BranchId",
                table: "BiometricsLog");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_id",
                table: "EmployeeTimeLog",
                column: "id",
                principalTable: "XEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_id",
                table: "EmployeeTimeLog");

            migrationBuilder.AddColumn<int>(
                name: "Company_BranchId",
                table: "XEmployee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "XCompany_BranchId",
                table: "EmployeeTimeLog",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "EmployeeShiftDevice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Company_BranchId",
                table: "EmployeeAttendance",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "XCompany_BranchId",
                table: "BiometricsLog",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "XCompany_Branch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Addr_Area_Id = table.Column<int>(type: "integer", nullable: true),
                    Addr_City_Id = table.Column<int>(type: "integer", nullable: true),
                    Addr_Country_Id = table.Column<int>(type: "integer", nullable: true),
                    Addr_Line1 = table.Column<string>(type: "text", nullable: true),
                    Addr_Line2 = table.Column<string>(type: "text", nullable: true),
                    Addr_Zip = table.Column<string>(type: "text", nullable: true),
                    Authorized_Tax_Representative_Employee_Id = table.Column<int>(type: "integer", nullable: true),
                    Authorized_Tax_Representative_Identification_Expiration_Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Authorized_Tax_Representative_Identification_Issuance_Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Authorized_Tax_Representative_Identification_Number = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Company_Branch_Code = table.Column<string>(type: "text", nullable: true),
                    Company_Id = table.Column<int>(type: "integer", nullable: false),
                    Cost_Center = table.Column<string>(type: "text", nullable: true),
                    Creation_Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Disabled = table.Column<bool>(type: "boolean", nullable: true),
                    Division = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Group = table.Column<string>(type: "text", nullable: true),
                    LandLine = table.Column<string>(type: "text", nullable: true),
                    Mobile = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Num_Employees = table.Column<int>(type: "integer", nullable: false),
                    Pagibig_Number = table.Column<string>(type: "text", nullable: true),
                    Philhealth_Number = table.Column<string>(type: "text", nullable: true),
                    Rdo_Code = table.Column<string>(type: "text", nullable: true),
                    Registered_Name = table.Column<string>(type: "text", nullable: true),
                    SSS_Number = table.Column<string>(type: "text", nullable: true),
                    Segment = table.Column<string>(type: "text", nullable: true),
                    Tin = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Vat = table.Column<bool>(type: "boolean", nullable: true),
                    Vat_Ratio = table.Column<double>(type: "double precision", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    _Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XCompany_Branch", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XEmployee_Company_BranchId",
                table: "XEmployee",
                column: "Company_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_XCompany_BranchId",
                table: "EmployeeTimeLog",
                column: "XCompany_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_BranchId",
                table: "EmployeeShiftDevice",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_Company_Branch_Id",
                table: "EmployeeShift",
                column: "Company_Branch_Id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_Company_BranchId",
                table: "EmployeeAttendance",
                column: "Company_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricsLog_XCompany_BranchId",
                table: "BiometricsLog",
                column: "XCompany_BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_BiometricsLog_XCompany_Branch_XCompany_BranchId",
                table: "BiometricsLog",
                column: "XCompany_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_XCompany_Branch_Company_BranchId",
                table: "EmployeeAttendance",
                column: "Company_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XCompany_Branch_Company_Branch_Id",
                table: "EmployeeShift",
                column: "Company_Branch_Id",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShiftDevice_XCompany_Branch_BranchId",
                table: "EmployeeShiftDevice",
                column: "BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XCompany_Branch_XCompany_BranchId",
                table: "EmployeeTimeLog",
                column: "XCompany_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_id",
                table: "EmployeeTimeLog",
                column: "id",
                principalTable: "XEmployee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_XEmployee_XCompany_Branch_Company_BranchId",
                table: "XEmployee",
                column: "Company_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");
        }
    }
}
