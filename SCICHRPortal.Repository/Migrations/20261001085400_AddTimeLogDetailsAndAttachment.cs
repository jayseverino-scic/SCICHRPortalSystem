using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeLogDetailsAndAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var column in new[] { "ProjectTimeIn", "ProjectTimeOut", "DeviceTimeIn", "DeviceTimeOut" })
                migrationBuilder.Sql($"UPDATE \"EmployeeTimeLog\" SET \"{column}\" = NULL WHERE btrim(\"{column}\", E' \\t\\r\\n') = '';");
            foreach (var column in new[] { "ProjectName", "DeviceName" })
                migrationBuilder.Sql($"UPDATE \"BiometricsLog\" SET \"{column}\" = NULL WHERE btrim(\"{column}\", E' \\t\\r\\n') = '';");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "EmployeeTimeLog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOB",
                table: "EmployeeTimeLog",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "EmployeeTimeLog",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ImportSource",
                table: "BiometricsLog",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TimeLogAttachment",
                columns: table => new
                {
                    TimeLogAttachmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeLogId = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    UploadedBy = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeLogAttachment", x => x.TimeLogAttachmentId);
                    table.ForeignKey(
                        name: "FK_TimeLogAttachment_EmployeeTimeLog_TimeLogId",
                        column: x => x.TimeLogId,
                        principalTable: "EmployeeTimeLog",
                        principalColumn: "TimeLogId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeLogAttachment_TimeLogId",
                table: "TimeLogAttachment",
                column: "TimeLogId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeLogAttachment");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "IsOB",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "EmployeeTimeLog");

            migrationBuilder.DropColumn(
                name: "ImportSource",
                table: "BiometricsLog");
        }
    }
}
