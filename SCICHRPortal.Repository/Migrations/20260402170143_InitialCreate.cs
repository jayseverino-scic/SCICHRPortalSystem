using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "BiometricsLog",
                columns: table => new
                {
                    BiometricsLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TMNo = table.Column<int>(type: "integer", nullable: false),
                    EmployeeNo = table.Column<string>(type: "text", nullable: true),
                    EmployeeName = table.Column<string>(type: "text", nullable: true),
                    GMNo = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<string>(type: "text", nullable: true),
                    InOut = table.Column<string>(type: "text", nullable: true),
                    AntiPass = table.Column<int>(type: "integer", nullable: false),
                    ProxyWork = table.Column<int>(type: "integer", nullable: false),
                    DateTimeLog = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiometricsLog", x => x.BiometricsLogId);
                });

            migrationBuilder.CreateTable(
                name: "CutOff",
                columns: table => new
                {
                    CutOffId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "Holiday",
                columns: table => new
                {
                    HolidayId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HolidayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HolidayDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HolidayType = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    HolidayTypes = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => x.PositionId);
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    ShiftStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BreakStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BreakEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shift", x => x.ShiftId);
                });

            migrationBuilder.CreateTable(
                name: "TimekeepingAdminSetup",
                columns: table => new
                {
                    SetupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftLateMinuteGracePeriod = table.Column<int>(type: "integer", nullable: false),
                    BreakLateMinuteGracePeriod = table.Column<int>(type: "integer", nullable: false),
                    ShiftLateTotalMinuteLimit = table.Column<int>(type: "integer", nullable: false),
                    BreakLateTotalMinuteLimit = table.Column<int>(type: "integer", nullable: false),
                    NoTimeLogCountLimit = table.Column<int>(type: "integer", nullable: false),
                    NoLeaveAbsentCountLimit = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimekeepingAdminSetup", x => x.SetupId);
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Suffix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Address = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    PositionId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                        name: "FK_Employee_Position_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Position",
                        principalColumn: "PositionId");
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "EmployeeShift",
                columns: table => new
                {
                    AssignedShiftId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    ShiftDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BreakStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BreakEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsFlexibleShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsFlexibleBreak = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoBreak = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                        name: "FK_EmployeeShift_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId");
                    table.ForeignKey(
                        name: "FK_EmployeeShift_Shift_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shift",
                        principalColumn: "ShiftId");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTimeLog",
                columns: table => new
                {
                    TimeLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    DateIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateBreakOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateBreakIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BreakOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BreakIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShiftStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BreakStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BreakEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsFlexibleShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsFlexibleBreak = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoBreak = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTimeLog", x => x.TimeLogId);
                    table.ForeignKey(
                        name: "FK_EmployeeTimeLog_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequest",
                columns: table => new
                {
                    LeaveRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "integer", nullable: false),
                    DateRequest = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestDays = table.Column<double>(type: "double precision", nullable: false),
                    LeaveReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "EmployeeAttendance",
                columns: table => new
                {
                    EmployeeAttendanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeLogId = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false),
                    TimeIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BreakIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BreakOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShiftStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BreakStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BreakEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShiftHours = table.Column<double>(type: "double precision", nullable: false),
                    RegularHour = table.Column<double>(type: "double precision", nullable: false),
                    TotalLoggedHours = table.Column<double>(type: "double precision", nullable: false),
                    ApprovedOT = table.Column<bool>(type: "boolean", nullable: false),
                    OTHours = table.Column<double>(type: "double precision", nullable: false),
                    NDHours = table.Column<double>(type: "double precision", nullable: false),
                    ShiftLate = table.Column<double>(type: "double precision", nullable: false),
                    ShiftUndertime = table.Column<double>(type: "double precision", nullable: false),
                    BreakLate = table.Column<double>(type: "double precision", nullable: false),
                    BreakUndertime = table.Column<double>(type: "double precision", nullable: false),
                    IsFlexibleShift = table.Column<bool>(type: "boolean", nullable: false),
                    IsFlexibleBreak = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoBreak = table.Column<bool>(type: "boolean", nullable: false),
                    IsNoShift = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedHolidayOT = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedSPHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedSPHolidayOT = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedRestDay = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedRestDayOT = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                        name: "FK_EmployeeAttendance_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.InsertData(
                table: "Department",
                columns: new[] { "DepartmentId", "CreatedBy", "DepartmentName", "DeptCode", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, null, "Faculty", "Fac", null, null },
                    { 2, null, "Administration", "Admin", null, null },
                    { 3, null, "Janitorial", "Jan", null, null },
                    { 4, null, "Maintenance", "Main", null, null },
                    { 5, null, "Accounting", "Acctg", null, null },
                    { 6, null, "Security", "Sec", null, null },
                    { 7, null, "Management", "Mgmt", null, null }
                });

            migrationBuilder.InsertData(
                table: "Holiday",
                columns: new[] { "HolidayId", "CreatedBy", "HolidayDate", "HolidayName", "HolidayType", "HolidayTypes", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year", 0, 0, null, null },
                    { 2, null, new DateTime(2023, 6, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Indepdence Day", 0, 0, null, null },
                    { 3, null, new DateTime(2022, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day", 0, 0, null, null },
                    { 4, null, new DateTime(2023, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Araw ng Kagitingan", 0, 0, null, null },
                    { 5, null, new DateTime(2023, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Labor Day", 0, 0, null, null },
                    { 6, null, new DateTime(2023, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Andres Bonifacio Day", 0, 0, null, null },
                    { 7, null, new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rizal Day", 0, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "RoleId", "CreatedBy", "Description", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, null, "Administrator", "Administrator", null, null },
                    { 2, null, "Admission", "Admission", null, null },
                    { 3, null, "Parent", "Parent", null, null },
                    { 4, null, "Registrar", "Registrar", null, null },
                    { 5, null, "HS Principal", "HS Principal", null, null },
                    { 6, null, "Elem Principal", "Elem Principal", null, null },
                    { 7, null, "HS Coordinator", "HS Coordinator", null, null },
                    { 8, null, "Elem Coordinator", "Elem Coordinator", null, null },
                    { 9, null, "Teacher Elem", "Teacher Elem", null, null },
                    { 10, null, "Teacher HS", "Teacher HS", null, null },
                    { 11, null, "Librian", "Librian", null, null },
                    { 12, null, "Supply Custodian", "Supply Custodian", null, null },
                    { 13, null, "Student", "Student", null, null },
                    { 14, null, "Other", "Other", null, null }
                });

            migrationBuilder.InsertData(
                table: "Shift",
                columns: new[] { "ShiftId", "BreakEnd", "BreakStart", "CreatedBy", "ShiftEnd", "ShiftName", "ShiftStart", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, null, null, null, new DateTime(2026, 1, 1, 16, 0, 0, 0, DateTimeKind.Unspecified), "Day Shift", new DateTime(2026, 1, 1, 7, 0, 0, 0, DateTimeKind.Unspecified), null, null });

            migrationBuilder.InsertData(
                table: "TimekeepingAdminSetup",
                columns: new[] { "SetupId", "BreakLateMinuteGracePeriod", "BreakLateTotalMinuteLimit", "CreatedBy", "NoLeaveAbsentCountLimit", "NoTimeLogCountLimit", "ShiftLateMinuteGracePeriod", "ShiftLateTotalMinuteLimit", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, 0, 0, null, 0, 0, 0, 0, null, null });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "ContactNumber", "CreatedBy", "Email", "FirstName", "IsApproved", "IsPasswordChanged", "LastName", "Locked", "LoginAttempts", "MiddleName", "Password", "Salt", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[] { 1, null, null, "superadmin@mail.com", "Super", true, false, "Admin", false, 0, null, "4DRtkqzRrxUk9Px/+Zu7vzTIk5f0dHc4mPgicSMkQzI=", "ml4A7caIeJit28zFyeiXVA==", null, null, "superadmin" });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "UserRoleId", "CreatedBy", "RoleId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { 1, null, 1, null, null, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DepartmentId",
                table: "Employee",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_PositionId",
                table: "Employee",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_UserId",
                table: "Employee",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_EmployeeId",
                table: "EmployeeAttendance",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_TimeLogId",
                table: "EmployeeAttendance",
                column: "TimeLogId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_DepartmentId",
                table: "EmployeeShift",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_EmployeeId",
                table: "EmployeeShift",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShift_ShiftId",
                table: "EmployeeShift",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeLog_EmployeeId",
                table: "EmployeeTimeLog",
                column: "EmployeeId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "Holiday");

            migrationBuilder.DropTable(
                name: "LeaveRequest");

            migrationBuilder.DropTable(
                name: "Module");

            migrationBuilder.DropTable(
                name: "TimekeepingAdminSetup");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "EmployeeTimeLog");

            migrationBuilder.DropTable(
                name: "Shift");

            migrationBuilder.DropTable(
                name: "LeaveType");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Position");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
