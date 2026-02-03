using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XYZ.Application.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProgressRecords_DynamicMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProgressRecords_StudentId",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "BodyFatPercentage",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "Endurance",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "Flexibility",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "MentalScore",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "PhysicalScore",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "SprintTime",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "TacticalScore",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "TechnicalScore",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "VerticalJump",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "ProgressRecords");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "RecordDate",
                table: "ProgressRecords",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "ProgressRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByDisplayName",
                table: "ProgressRecords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "ProgressRecords",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "ProgressRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProgressMetricDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressMetricDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressMetricDefinitions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgressRecordValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgressRecordId = table.Column<int>(type: "int", nullable: false),
                    ProgressMetricDefinitionId = table.Column<int>(type: "int", nullable: false),
                    DecimalValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    IntValue = table.Column<int>(type: "int", nullable: true),
                    TextValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressRecordValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressRecordValues_ProgressMetricDefinitions_ProgressMetricDefinitionId",
                        column: x => x.ProgressMetricDefinitionId,
                        principalTable: "ProgressMetricDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgressRecordValues_ProgressRecords_ProgressRecordId",
                        column: x => x.ProgressRecordId,
                        principalTable: "ProgressRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressRecords_BranchId",
                table: "ProgressRecords",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressRecords_StudentId_BranchId_RecordDate",
                table: "ProgressRecords",
                columns: new[] { "StudentId", "BranchId", "RecordDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressRecords_StudentId_BranchId_RecordDate_Sequence",
                table: "ProgressRecords",
                columns: new[] { "StudentId", "BranchId", "RecordDate", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressMetricDefinitions_BranchId_Name",
                table: "ProgressMetricDefinitions",
                columns: new[] { "BranchId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressRecordValues_ProgressMetricDefinitionId",
                table: "ProgressRecordValues",
                column: "ProgressMetricDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressRecordValues_ProgressRecordId_ProgressMetricDefinitionId",
                table: "ProgressRecordValues",
                columns: new[] { "ProgressRecordId", "ProgressMetricDefinitionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressRecords_Branches_BranchId",
                table: "ProgressRecords",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressRecords_Branches_BranchId",
                table: "ProgressRecords");

            migrationBuilder.DropTable(
                name: "ProgressRecordValues");

            migrationBuilder.DropTable(
                name: "ProgressMetricDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_ProgressRecords_BranchId",
                table: "ProgressRecords");

            migrationBuilder.DropIndex(
                name: "IX_ProgressRecords_StudentId_BranchId_RecordDate",
                table: "ProgressRecords");

            migrationBuilder.DropIndex(
                name: "IX_ProgressRecords_StudentId_BranchId_RecordDate_Sequence",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "CreatedByDisplayName",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ProgressRecords");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "ProgressRecords");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RecordDate",
                table: "ProgressRecords",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<decimal>(
                name: "BodyFatPercentage",
                table: "ProgressRecords",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Endurance",
                table: "ProgressRecords",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Flexibility",
                table: "ProgressRecords",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "ProgressRecords",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MentalScore",
                table: "ProgressRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhysicalScore",
                table: "ProgressRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SprintTime",
                table: "ProgressRecords",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TacticalScore",
                table: "ProgressRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicalScore",
                table: "ProgressRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VerticalJump",
                table: "ProgressRecords",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "ProgressRecords",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressRecords_StudentId",
                table: "ProgressRecords",
                column: "StudentId");
        }
    }
}
