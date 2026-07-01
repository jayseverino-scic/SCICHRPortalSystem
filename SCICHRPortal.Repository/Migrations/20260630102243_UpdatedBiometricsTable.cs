using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBiometricsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_Employee_EmployeeId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_Department_DepartmentId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_Employee_EmployeeId",
                table: "EmployeeShift");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShiftDevice_Employee_EmployeeId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_Employee_EmployeeId",
                table: "EmployeeTimeLog");

            migrationBuilder.AddColumn<int>(
                name: "TimekeepingDevicesId",
                table: "EmployeeTimeLog",
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
                name: "TimekeepingDevicesId",
                table: "EmployeeShiftDevice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "EmployeeShift",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimekeepingDevicesId",
                table: "EmployeeShift",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Company_BranchId",
                table: "EmployeeAttendance",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimekeepingDevicesId",
                table: "EmployeeAttendance",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "BiometricsLog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimekeepingDevicesId",
                table: "BiometricsLog",
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

            migrationBuilder.CreateTable(
                name: "XEmployee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Last_Name = table.Column<string>(type: "text", nullable: true),
                    First_Name = table.Column<string>(type: "text", nullable: true),
                    Middle_Name = table.Column<string>(type: "text", nullable: true),
                    Suffix = table.Column<string>(type: "text", nullable: true),
                    Display_Name = table.Column<string>(type: "text", nullable: true),
                    Birth_Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Blood_Type = table.Column<string>(type: "text", nullable: true),
                    Company_Id = table.Column<int>(type: "integer", nullable: false),
                    Company_Branch_Id = table.Column<int>(type: "integer", nullable: false),
                    Department_Id = table.Column<int>(type: "integer", nullable: true),
                    Position = table.Column<string>(type: "text", nullable: true),
                    Employment_Status = table.Column<string>(type: "text", nullable: true),
                    _Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    LandLine = table.Column<string>(type: "text", nullable: true),
                    Mobile = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Employee_code = table.Column<string>(type: "text", nullable: true),
                    Num_201_Files = table.Column<int>(type: "integer", nullable: false),
                    NickName = table.Column<string>(type: "text", nullable: true),
                    Weight_Kg = table.Column<double>(type: "double precision", nullable: true),
                    Height_M = table.Column<double>(type: "double precision", nullable: true),
                    Birth_Place_City_Id = table.Column<int>(type: "integer", nullable: true),
                    Marital_Status = table.Column<string>(type: "text", nullable: true),
                    Religion_Id = table.Column<int>(type: "integer", nullable: true),
                    Citizenship_Id = table.Column<int>(type: "integer", nullable: true),
                    Company_Position_Id = table.Column<int>(type: "integer", nullable: true),
                    Num_Employments = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SubDepartment_Id = table.Column<int>(type: "integer", nullable: true),
                    Business_Unit_Id = table.Column<int>(type: "integer", nullable: true),
                    Timekeeping_Device_Identifier = table.Column<string>(type: "text", nullable: true),
                    X_Point_Of_Contact = table.Column<string>(type: "text", nullable: true),
                    X_Seat_Class = table.Column<string>(type: "text", nullable: true),
                    Company_Job_Grade_Id = table.Column<int>(type: "integer", nullable: true),
                    Company_Job_Rank_Id = table.Column<int>(type: "integer", nullable: true),
                    Company_Job_Class_Id = table.Column<int>(type: "integer", nullable: true),
                    Company_Location_Id = table.Column<int>(type: "integer", nullable: true),
                    Nationality_Id = table.Column<int>(type: "integer", nullable: true),
                    Expat = table.Column<bool>(type: "boolean", nullable: true),
                    Default_Hr_Payroll_Record_Id = table.Column<int>(type: "integer", nullable: true),
                    Employee_Group_Id = table.Column<int>(type: "integer", nullable: true),
                    Employee_Classification_Id = table.Column<int>(type: "integer", nullable: true),
                    Location_Address_Line1 = table.Column<string>(type: "text", nullable: true),
                    Location_Address_Line2 = table.Column<string>(type: "text", nullable: true),
                    Location_Address_Location_Building_Id = table.Column<int>(type: "integer", nullable: true),
                    Location_Address_Location_Zone_Id = table.Column<int>(type: "integer", nullable: true),
                    Location_Address_Location_City_Id = table.Column<int>(type: "integer", nullable: true),
                    Location_Address_Location_Area_Id = table.Column<int>(type: "integer", nullable: true),
                    Location_Address_Location_Country_Id = table.Column<int>(type: "integer", nullable: true),
                    Location_Address_Zip = table.Column<string>(type: "text", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    Company_BranchId = table.Column<int>(type: "integer", nullable: true),
                    Company_PositionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XEmployee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XEmployee_XCompany_Branch_Company_BranchId",
                        column: x => x.Company_BranchId,
                        principalTable: "XCompany_Branch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_XEmployee_XCompany_Position_Company_PositionId",
                        column: x => x.Company_PositionId,
                        principalTable: "XCompany_Position",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_XEmployee_XDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "XDepartment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_TimekeepingDevicesId",
                table: "EmployeeTimeLog",
                column: "TimekeepingDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_XCompany_BranchId",
                table: "EmployeeTimeLog",
                column: "XCompany_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_BranchId",
                table: "EmployeeShiftDevice",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_TimekeepingDevicesId",
                table: "EmployeeShiftDevice",
                column: "TimekeepingDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_CompanyId",
                table: "EmployeeShift",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_TimekeepingDevicesId",
                table: "EmployeeShift",
                column: "TimekeepingDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_Company_BranchId",
                table: "EmployeeAttendance",
                column: "Company_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_TimekeepingDevicesId",
                table: "EmployeeAttendance",
                column: "TimekeepingDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricsLog_TimekeepingDevicesId",
                table: "BiometricsLog",
                column: "TimekeepingDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricsLog_XCompany_BranchId",
                table: "BiometricsLog",
                column: "XCompany_BranchId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_BiometricsLog_TimekeepingDevices_TimekeepingDevicesId",
                table: "BiometricsLog",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BiometricsLog_XCompany_Branch_XCompany_BranchId",
                table: "BiometricsLog",
                column: "XCompany_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeAttendance",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_XCompany_Branch_Company_BranchId",
                table: "EmployeeAttendance",
                column: "Company_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_XEmployee_EmployeeId",
                table: "EmployeeAttendance",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeShift",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
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
                name: "FK_EmployeeShiftDevice_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeShiftDevice",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShiftDevice_XCompany_Branch_BranchId",
                table: "EmployeeShiftDevice",
                column: "BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShiftDevice_XEmployee_EmployeeId",
                table: "EmployeeShiftDevice",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeTimeLog",
                column: "TimekeepingDevicesId",
                principalTable: "TimekeepingDevices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XCompany_Branch_XCompany_BranchId",
                table: "EmployeeTimeLog",
                column: "XCompany_BranchId",
                principalTable: "XCompany_Branch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BiometricsLog_TimekeepingDevices_TimekeepingDevicesId",
                table: "BiometricsLog");

            migrationBuilder.DropForeignKey(
                name: "FK_BiometricsLog_XCompany_Branch_XCompany_BranchId",
                table: "BiometricsLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_XCompany_Branch_Company_BranchId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendance_XEmployee_EmployeeId",
                table: "EmployeeAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShift_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeShift");

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
                name: "FK_EmployeeShiftDevice_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShiftDevice_XCompany_Branch_BranchId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShiftDevice_XEmployee_EmployeeId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_TimekeepingDevices_TimekeepingDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XCompany_Branch_XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropTable(
                name: "XEmployee");

            migrationBuilder.DropTable(
                name: "XCompany_Branch");

            migrationBuilder.DropTable(
                name: "XCompany_Position");

            migrationBuilder.DropTable(
                name: "XDepartment");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTimeLog_TimekeepingDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTimeLog_XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShiftDevice_BranchId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShiftDevice_TimekeepingDevicesId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_CompanyId",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShift_TimekeepingDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAttendance_Company_BranchId",
                table: "EmployeeAttendance");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAttendance_TimekeepingDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropIndex(
                name: "IX_BiometricsLog_TimekeepingDevicesId",
                table: "BiometricsLog");

            migrationBuilder.DropIndex(
                name: "IX_BiometricsLog_XCompany_BranchId",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "XCompany_BranchId",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "EmployeeShiftDevice");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "EmployeeShift");

            migrationBuilder.DropColumn(
                name: "Company_BranchId",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "TimekeepingDevicesId",
                table: "BiometricsLog");

            migrationBuilder.DropColumn(
                name: "XCompany_BranchId",
                table: "BiometricsLog");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendance_Employee_EmployeeId",
                table: "EmployeeAttendance",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_Department_DepartmentId",
                table: "EmployeeShift",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShift_Employee_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShiftDevice_Employee_EmployeeId",
                table: "EmployeeShiftDevice",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_Employee_EmployeeId",
                table: "EmployeeTimeLog",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId");
        }
    }
}
