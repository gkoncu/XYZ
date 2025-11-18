using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XYZ.Application.Data.Migrations
{
    /// <inheritdoc />
    public partial class EntitiesUpdateForAttendanceImp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_ClassSchedules_ClassScheduleId",
                table: "Attendances");

            migrationBuilder.DropTable(
                name: "ClassSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_ClassScheduleId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "ClassScheduleId",
                table: "Attendances",
                newName: "ClassSessionId");

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CoachComment",
                table: "Attendances",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Attendances",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Attendances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClassEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CoachNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassSessions_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ClassId",
                table: "Attendances",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ClassSessionId_StudentId",
                table: "Attendances",
                columns: new[] { "ClassSessionId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_ClassId_StudentId_StartDate",
                table: "ClassEnrollments",
                columns: new[] { "ClassId", "StudentId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_StudentId",
                table: "ClassEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_ClassId_Date",
                table: "ClassSessions",
                columns: new[] { "ClassId", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_ClassSessions_ClassSessionId",
                table: "Attendances",
                column: "ClassSessionId",
                principalTable: "ClassSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Classes_ClassId",
                table: "Attendances",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_ClassSessions_ClassSessionId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Classes_ClassId",
                table: "Attendances");

            migrationBuilder.DropTable(
                name: "ClassEnrollments");

            migrationBuilder.DropTable(
                name: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_ClassId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_ClassSessionId_StudentId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CoachComment",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "ClassSessionId",
                table: "Attendances",
                newName: "ClassScheduleId");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Attendances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClassSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassSchedules_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ClassScheduleId",
                table: "Attendances",
                column: "ClassScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_ClassId",
                table: "ClassSchedules",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_ClassSchedules_ClassScheduleId",
                table: "Attendances",
                column: "ClassScheduleId",
                principalTable: "ClassSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
