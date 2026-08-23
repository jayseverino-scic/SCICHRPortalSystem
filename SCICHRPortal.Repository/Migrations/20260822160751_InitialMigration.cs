using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Announcement",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnnouncementForm = table.Column<string>(type: "text", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcement", x => x.AnnouncementId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    RequestOrigin = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    TableName = table.Column<string>(type: "text", nullable: true),
                    SystemName = table.Column<string>(type: "text", nullable: true),
                    DateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    AffectedColumns = table.Column<string>(type: "text", nullable: true),
                    PrimaryKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CutOff",
                columns: table => new
                {
                    CutOffId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CutOff", x => x.CutOffId);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeptCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DepartmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SerialNumber = table.Column<string>(type: "text", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holiday",
                columns: table => new
                {
                    HolidayId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HolidayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HolidayDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    HolidayType = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    HolidayTypes = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holiday", x => x.HolidayId);
                });

            migrationBuilder.CreateTable(
                name: "LeaveType",
                columns: table => new
                {
                    LeaveTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeaveDescription = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AllowedDays = table.Column<int>(type: "integer", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveType", x => x.LeaveTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Module",
                columns: table => new
                {
                    ModuleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.ModuleId);
                });

            migrationBuilder.CreateTable(
                name: "Position",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PositionName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => x.PositionId);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Shift",
                columns: table => new
                {
                    ShiftId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MondayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MondayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TuesdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TuesdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WednesdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WednesdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ThursdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ThursdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FridayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FridayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SaturdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SaturdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SundayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SundayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ShiftLateMinuteGracePeriod = table.Column<int>(type: "integer", nullable: false),
                    ShiftLateTotalMinuteLimit = table.Column<int>(type: "integer", nullable: false),
                    NoTimeLogCountLimit = table.Column<int>(type: "integer", nullable: false),
                    NoLeaveAbsentCountLimit = table.Column<int>(type: "integer", nullable: false),
                    RestDays = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shift", x => x.ShiftId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    LastName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Username = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Salt = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    IsPasswordChanged = table.Column<bool>(type: "boolean", nullable: false),
                    LoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    Locked = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

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
                name: "ZKDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IPAddress = table.Column<string>(type: "text", nullable: true),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    APIVersion = table.Column<string>(type: "text", nullable: true),
                    RegistryCode = table.Column<string>(type: "text", nullable: true),
                    FirmwareVersion = table.Column<string>(type: "text", nullable: true),
                    DeviceFunction = table.Column<string>(type: "text", nullable: true),
                    FingerprintSupported = table.Column<bool>(type: "boolean", nullable: false),
                    FaceSupported = table.Column<bool>(type: "boolean", nullable: false),
                    PalmSupported = table.Column<bool>(type: "boolean", nullable: false),
                    LockOpenDuration = table.Column<int>(type: "integer", nullable: false),
                    DeviceInformation = table.Column<string>(type: "text", nullable: true),
                    TimeZone = table.Column<string>(type: "text", nullable: true),
                    AntiPassback = table.Column<int>(type: "integer", nullable: false),
                    AntiPassbackOn = table.Column<bool>(type: "boolean", nullable: false),
                    KeyMapping = table.Column<string>(type: "text", nullable: true),
                    SyncStatus = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZKDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Suffix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Address = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    ProjectId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employee_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_Employee_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employee_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserRoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
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
                    Company_Branch_Id = table.Column<int>(type: "integer", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "BiometricsLog",
                columns: table => new
                {
                    BiometricsLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonnelId = table.Column<string>(type: "text", nullable: true),
                    SZKDevicesId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LogType = table.Column<string>(type: "text", nullable: true),
                    DeviceName = table.Column<string>(type: "text", nullable: true),
                    ProjectName = table.Column<string>(type: "text", nullable: true),
                    XCompany_BranchId = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiometricsLog", x => x.BiometricsLogId);
                    table.ForeignKey(
                        name: "FK_BiometricsLog_XCompany_Branch_XCompany_BranchId",
                        column: x => x.XCompany_BranchId,
                        principalTable: "XCompany_Branch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BiometricsLog_ZKDevices_SZKDevicesId",
                        column: x => x.SZKDevicesId,
                        principalTable: "ZKDevices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeShift",
                columns: table => new
                {
                    AssignedShiftId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    ProjectId = table.Column<int>(type: "integer", nullable: true),
                    ShiftDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MondayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MondayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TuesdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TuesdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WednesdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WednesdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ThursdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ThursdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FridayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FridayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SaturdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SaturdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SundayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SundayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsFlexibleShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoBreak = table.Column<bool>(type: "boolean", nullable: false),
                    DeviceId = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeShift", x => x.AssignedShiftId);
                    table.ForeignKey(
                        name: "FK_EmployeeShift_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_EmployeeShift_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeShift_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_EmployeeShift_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeShift_Shift_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shift",
                        principalColumn: "ShiftId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequest",
                columns: table => new
                {
                    LeaveRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "integer", nullable: false),
                    DateRequest = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FromDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RequestDays = table.Column<double>(type: "double precision", nullable: false),
                    LeaveReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequest", x => x.LeaveRequestId);
                    table.ForeignKey(
                        name: "FK_LeaveRequest_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_LeaveRequest_LeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveType",
                        principalColumn: "LeaveTypeId");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeShiftDevice",
                columns: table => new
                {
                    AssignedShiftId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    ShiftDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MondayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MondayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TuesdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TuesdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WednesdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WednesdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ThursdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ThursdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FridayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FridayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SaturdayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SaturdayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SundayShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SundayShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsFlexibleShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoBreak = table.Column<bool>(type: "boolean", nullable: false),
                    Devicename = table.Column<string>(type: "text", nullable: true),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    SZKDevicesId = table.Column<Guid>(type: "uuid", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeShiftDevice", x => x.AssignedShiftId);
                    table.ForeignKey(
                        name: "FK_EmployeeShiftDevice_Shift_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shift",
                        principalColumn: "ShiftId");
                    table.ForeignKey(
                        name: "FK_EmployeeShiftDevice_XCompany_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "XCompany_Branch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeShiftDevice_XEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "XEmployee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeShiftDevice_ZKDevices_SZKDevicesId",
                        column: x => x.SZKDevicesId,
                        principalTable: "ZKDevices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTimeLog",
                columns: table => new
                {
                    TimeLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id = table.Column<int>(type: "integer", nullable: false),
                    DateIn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateOut = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeIn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TimeOut = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsFlexibleShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoBreak = table.Column<bool>(type: "boolean", nullable: false),
                    SystemRemarks = table.Column<string>(type: "text", nullable: true),
                    DeviceTimeIn = table.Column<string>(type: "text", nullable: true),
                    DeviceTimeOut = table.Column<string>(type: "text", nullable: true),
                    ProjecTimeIn = table.Column<string>(type: "text", nullable: true),
                    ProjectTimeOut = table.Column<string>(type: "text", nullable: true),
                    XCompany_BranchId = table.Column<int>(type: "integer", nullable: true),
                    SZKDevicesId = table.Column<Guid>(type: "uuid", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTimeLog", x => x.TimeLogId);
                    table.ForeignKey(
                        name: "FK_EmployeeTimeLog_XCompany_Branch_XCompany_BranchId",
                        column: x => x.XCompany_BranchId,
                        principalTable: "XCompany_Branch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeTimeLog_XEmployee_id",
                        column: x => x.id,
                        principalTable: "XEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeTimeLog_ZKDevices_SZKDevicesId",
                        column: x => x.SZKDevicesId,
                        principalTable: "ZKDevices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAttendance",
                columns: table => new
                {
                    EmployeeAttendanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeLogId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    TimeIn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeOut = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ShiftHours = table.Column<double>(type: "double precision", nullable: false),
                    RegularHour = table.Column<double>(type: "double precision", nullable: false),
                    TotalLoggedHours = table.Column<double>(type: "double precision", nullable: false),
                    ApprovedOT = table.Column<bool>(type: "boolean", nullable: false),
                    OTHours = table.Column<double>(type: "double precision", nullable: false),
                    NDHours = table.Column<double>(type: "double precision", nullable: false),
                    ShiftLate = table.Column<double>(type: "double precision", nullable: false),
                    ShiftUndertime = table.Column<double>(type: "double precision", nullable: false),
                    IsFlexibleShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoBreak = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoShift = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedHolidayOT = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedSPHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedSPHolidayOT = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedRestDay = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedRestDayOT = table.Column<bool>(type: "boolean", nullable: false),
                    Company_BranchId = table.Column<int>(type: "integer", nullable: true),
                    ZKDevicesId = table.Column<Guid>(type: "uuid", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAttendance", x => x.EmployeeAttendanceId);
                    table.ForeignKey(
                        name: "FK_EmployeeAttendance_EmployeeTimeLog_TimeLogId",
                        column: x => x.TimeLogId,
                        principalTable: "EmployeeTimeLog",
                        principalColumn: "TimeLogId");
                    table.ForeignKey(
                        name: "FK_EmployeeAttendance_XCompany_Branch_Company_BranchId",
                        column: x => x.Company_BranchId,
                        principalTable: "XCompany_Branch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeAttendance_XEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "XEmployee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeAttendance_ZKDevices_ZKDevicesId",
                        column: x => x.ZKDevicesId,
                        principalTable: "ZKDevices",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "RoleId", "CreatedAt", "CreatedBy", "Description", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 4, 18, 10, 30, 0, 0, DateTimeKind.Utc), "jun rivas", "Administrator", "Administrator", null, null },
                    { 2, new DateTime(2025, 4, 18, 10, 30, 0, 0, DateTimeKind.Utc), "jun rivas", "Other", "Other", null, null }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "ContactNumber", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsApproved", "IsPasswordChanged", "LastName", "Locked", "LoginAttempts", "MiddleName", "Password", "Salt", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[] { 1, null, new DateTime(2025, 4, 18, 10, 30, 0, 0, DateTimeKind.Utc), "jun rivas", "superadmin@mail.com", "Super", true, false, "Admin", false, 0, null, "4DRtkqzRrxUk9Px/+Zu7vzTIk5f0dHc4mPgicSMkQzI=", "ml4A7caIeJit28zFyeiXVA==", null, null, "superadmin" });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "UserRoleId", "CreatedAt", "CreatedBy", "RoleId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { 1, new DateTime(2025, 4, 18, 10, 30, 0, 0, DateTimeKind.Utc), "jun rivas", 1, null, null, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_BiometricsLog_SZKDevicesId",
                table: "BiometricsLog",
                column: "SZKDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_BiometricsLog_XCompany_BranchId",
                table: "BiometricsLog",
                column: "XCompany_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DepartmentId",
                table: "Employee",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_ProjectId",
                table: "Employee",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_UserId",
                table: "Employee",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_Company_BranchId",
                table: "EmployeeAttendance",
                column: "Company_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_EmployeeId",
                table: "EmployeeAttendance",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_TimeLogId",
                table: "EmployeeAttendance",
                column: "TimeLogId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_ZKDevicesId",
                table: "EmployeeAttendance",
                column: "ZKDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_DepartmentId",
                table: "EmployeeShift",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_DeviceId",
                table: "EmployeeShift",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_ProjectId",
                table: "EmployeeShift",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_ShiftId",
                table: "EmployeeShift",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_BranchId",
                table: "EmployeeShiftDevice",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_EmployeeId",
                table: "EmployeeShiftDevice",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_ShiftId",
                table: "EmployeeShiftDevice",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftDevice_SZKDevicesId",
                table: "EmployeeShiftDevice",
                column: "SZKDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_id",
                table: "EmployeeTimeLog",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_SZKDevicesId",
                table: "EmployeeTimeLog",
                column: "SZKDevicesId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_XCompany_BranchId",
                table: "EmployeeTimeLog",
                column: "XCompany_BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_EmployeeId",
                table: "LeaveRequest",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_LeaveTypeId",
                table: "LeaveRequest",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcement");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BiometricsLog");

            migrationBuilder.DropTable(
                name: "CutOff");

            migrationBuilder.DropTable(
                name: "EmployeeAttendance");

            migrationBuilder.DropTable(
                name: "EmployeeShift");

            migrationBuilder.DropTable(
                name: "EmployeeShiftDevice");

            migrationBuilder.DropTable(
                name: "Holiday");

            migrationBuilder.DropTable(
                name: "LeaveRequest");

            migrationBuilder.DropTable(
                name: "Module");

            migrationBuilder.DropTable(
                name: "Position");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "EmployeeTimeLog");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "Shift");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "LeaveType");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "XEmployee");

            migrationBuilder.DropTable(
                name: "ZKDevices");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "XCompany_Branch");

            migrationBuilder.DropTable(
                name: "XCompany_Position");

            migrationBuilder.DropTable(
                name: "XDepartment");
        }
    }
}
