using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmployeeRelationshipOnEmployeeShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.AddColumn<int>(
                name: "Company_BranchId",
                table: "XEmployee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Company_PositionId",
                table: "XEmployee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
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
                name: "CompanyId",
                table: "EmployeeShift",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId1",
                table: "EmployeeShift",
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
                    Name = table.Column<string>(type: "text", nullable: true),
                    Registered_Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Addr_Line1 = table.Column<string>(type: "text", nullable: true),
                    Addr_Line2 = table.Column<string>(type: "text", nullable: true),
                    Addr_City_Id = table.Column<int>(type: "integer", nullable: true),
                    Addr_Area_Id = table.Column<int>(type: "integer", nullable: true),
                    Addr_Country_Id = table.Column<int>(type: "integer", nullable: true),
                    Addr_Zip = table.Column<string>(type: "text", nullable: true),
                    Company_Id = table.Column<int>(type: "integer", nullable: false),
                    _Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    LandLine = table.Column<string>(type: "text", nullable: true),
                    Mobile = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Num_Employees = table.Column<int>(type: "integer", nullable: false),
                    Tin = table.Column<string>(type: "text", nullable: true),
                    Vat = table.Column<bool>(type: "boolean", nullable: true),
                    Vat_Ratio = table.Column<double>(type: "double precision", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    SSS_Number = table.Column<string>(type: "text", nullable: true),
                    Philhealth_Number = table.Column<string>(type: "text", nullable: true),
                    Pagibig_Number = table.Column<string>(type: "text", nullable: true),
                    Rdo_Code = table.Column<string>(type: "text", nullable: true),
                    Creation_Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Company_Branch_Code = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Cost_Center = table.Column<string>(type: "text", nullable: true),
                    Segment = table.Column<string>(type: "text", nullable: true),
                    Group = table.Column<string>(type: "text", nullable: true),
                    Division = table.Column<string>(type: "text", nullable: true),
                    Disabled = table.Column<bool>(type: "boolean", nullable: true),
                    Authorized_Tax_Representative_Employee_Id = table.Column<int>(type: "integer", nullable: true),
                    Authorized_Tax_Representative_Identification_Number = table.Column<string>(type: "text", nullable: true),
                    Authorized_Tax_Representative_Identification_Issuance_Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Authorized_Tax_Representative_Identification_Expiration_Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XCompany_Branch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XCompany_Position",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Company_Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Rank = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XCompany_Position", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XDepartment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Company_Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    _Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XDepartment", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XEmployee_Company_BranchId",
                table: "XEmployee",
                column: "Company_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_XEmployee_Company_PositionId",
                table: "XEmployee",
                column: "Company_PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_XEmployee_DepartmentId",
                table: "XEmployee",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_XCompany_BranchId",
                table: "EmployeeTimeLog",
                column: "XCompany_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_BranchId",
                table: "EmployeeShiftDevice",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_CompanyId",
                table: "EmployeeShift",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_DepartmentId",
                table: "EmployeeShift",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_EmployeeId1",
                table: "EmployeeShift",
                column: "EmployeeId1");

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
                name: "FK_EmployeeShift_XCompany_Branch_CompanyId",
                table: "EmployeeShift",
                column: "CompanyId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_XDepartment_DepartmentId",
                table: "EmployeeShift",
                column: "DepartmentId",
                principalTable: "XDepartment",
                principalColumn: "Id");

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
                name: "FK_XEmployee_XCompany_Branch_Company_BranchId",
                table: "XEmployee",
                column: "Company_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_XEmployee_XCompany_Position_Company_PositionId",
                table: "XEmployee",
                column: "Company_PositionId",
                principalTable: "XCompany_Position",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_XEmployee_XDepartment_DepartmentId",
                table: "XEmployee",
                column: "DepartmentId",
                principalTable: "XDepartment",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BiometricsLog_XCompany_Branch_XCompany_BranchId",
                table: "BiometricsLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_XCompany_Branch_Company_BranchId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XCompany_Branch_CompanyId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XDepartment_DepartmentId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_XEmployee_EmployeeId1",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShiftDevice_XCompany_Branch_BranchId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XCompany_Branch_XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropForeignKey(
                name: "FK_XEmployee_XCompany_Branch_Company_BranchId",
                table: "XEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_XEmployee_XCompany_Position_Company_PositionId",
                table: "XEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_XEmployee_XDepartment_DepartmentId",
                table: "XEmployee");

            migrationBuilder.DropTable(
                name: "XCompany_Branch");

            migrationBuilder.DropTable(
                name: "XCompany_Position");

            migrationBuilder.DropTable(
                name: "XDepartment");

            migrationBuilder.DropIndex(
                name: "IX_XEmployee_Company_BranchId",
                table: "XEmployee");

            migrationBuilder.DropIndex(
                name: "IX_XEmployee_Company_PositionId",
                table: "XEmployee");

            migrationBuilder.DropIndex(
                name: "IX_XEmployee_DepartmentId",
                table: "XEmployee");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTimeLog_XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShiftDevice_BranchId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_CompanyId",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_DepartmentId",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_EmployeeId1",
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
                name: "Company_PositionId",
                table: "XEmployee");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "XEmployee");

            migrationBuilder.DropColumn(
                name: "XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "EmployeeShift");

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
        }
    }
}
