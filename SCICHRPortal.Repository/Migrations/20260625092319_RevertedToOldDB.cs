using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RevertedToOldDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Company_Branch_Company_BranchId",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "Company_Branch");

            migrationBuilder.DropIndex(
                name: "IX_Employee_Company_BranchId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Birth_Place_City_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Blood_Type",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Business_Unit_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Citizenship_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Company_BranchId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Company_Branch_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Company_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Company_Job_Class_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Company_Job_Grade_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Company_Job_Rank_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Company_Location_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Company_Position_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Default_Hr_Payroll_Record_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Department_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Display_Name",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Employee_Classification_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Employee_Group_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Employee_code",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Employment_Status",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Expat",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "First_Name",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Height_M",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "LandLine",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Last_Name",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Location_Address_Line1",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Location_Address_Line2",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Location_Address_Location_Area_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Location_Address_Location_Building_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Location_Address_Location_City_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Location_Address_Location_Country_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Location_Address_Location_Zone_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Location_Address_Zip",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Marital_Status",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Middle_Name",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Nationality_Id",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "NickName",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Num_201_Files",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Num_Employments",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Timekeeping_Device_Identifier",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Weight_Kg",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "X_Point_Of_Contact",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "X_Seat_Class",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "_Deleted",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Company_Id",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "_Deleted",
                table: "Department");

            migrationBuilder.RenameColumn(
                name: "SubDepartment_Id",
                table: "Employee",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Religtion_Id",
                table: "Employee",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "Birth_Date",
                table: "Employee",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Employee",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Department",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Department",
                newName: "DepartmentId");

            migrationBuilder.AlterColumn<string>(
                name: "Suffix",
                table: "Employee",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Employee",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Employee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Employee",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Employee",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Employee",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNo",
                table: "Employee",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Employee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Employee",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Employee",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Employee",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Department",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Department",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Department",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Department",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeptCode",
                table: "Department",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Department",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_PositionId",
                table: "Employee",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_UserId",
                table: "Employee",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Position_PositionId",
                table: "Employee",
                column: "PositionId",
                principalTable: "Position",
                principalColumn: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_User_UserId",
                table: "Employee",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Position_PositionId",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_User_UserId",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_PositionId",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_UserId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "EmployeeNo",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "DeptCode",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Department");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Employee",
                newName: "SubDepartment_Id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Employee",
                newName: "Birth_Date");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "Employee",
                newName: "Religtion_Id");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Employee",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Department",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Department",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Suffix",
                table: "Employee",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employee",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "Birth_Place_City_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Blood_Type",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Business_Unit_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Citizenship_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Company_BranchId",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Company_Branch_Id",
                table: "Employee",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Company_Id",
                table: "Employee",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Company_Job_Class_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Company_Job_Grade_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Company_Job_Rank_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Company_Location_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Company_Position_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Default_Hr_Payroll_Record_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Department_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Display_Name",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Employee_Classification_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Employee_Group_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Employee_code",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Employment_Status",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Expat",
                table: "Employee",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "First_Name",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Height_M",
                table: "Employee",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LandLine",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Last_Name",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location_Address_Line1",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location_Address_Line2",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Location_Address_Location_Area_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Location_Address_Location_Building_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Location_Address_Location_City_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Location_Address_Location_Country_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Location_Address_Location_Zone_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location_Address_Zip",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Marital_Status",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Middle_Name",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Nationality_Id",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Num_201_Files",
                table: "Employee",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Num_Employments",
                table: "Employee",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timekeeping_Device_Identifier",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Weight_Kg",
                table: "Employee",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "X_Point_Of_Contact",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "X_Seat_Class",
                table: "Employee",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "_Deleted",
                table: "Employee",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Company_Id",
                table: "Department",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "_Deleted",
                table: "Department",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Company_Branch",
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
                    table.PrimaryKey("PK_Company_Branch", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_Company_BranchId",
                table: "Employee",
                column: "Company_BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Company_Branch_Company_BranchId",
                table: "Employee",
                column: "Company_BranchId",
                principalTable: "Company_Branch",
                principalColumn: "Id");
        }
    }
}
