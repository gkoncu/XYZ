using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XYZ.Application.Data.Migrations
{
    /// <inheritdoc />
    public partial class Documents_DocumentDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Documents",
                newName: "DocumentDefinitionId");

            migrationBuilder.CreateTable(
                name: "DocumentDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentDefinitions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentDefinitionId",
                table: "Documents",
                column: "DocumentDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDefinitions_TenantId",
                table: "DocumentDefinitions",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_DocumentDefinitions_DocumentDefinitionId",
                table: "Documents",
                column: "DocumentDefinitionId",
                principalTable: "DocumentDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_DocumentDefinitions_DocumentDefinitionId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "DocumentDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DocumentDefinitionId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "DocumentDefinitionId",
                table: "Documents",
                newName: "Type");
        }
    }
}
