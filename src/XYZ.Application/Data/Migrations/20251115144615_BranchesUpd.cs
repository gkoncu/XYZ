using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XYZ.Application.Data.Migrations
{
    /// <inheritdoc />
    public partial class BranchesUpd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branch_Tenants_TenantId",
                table: "Branch");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Branch_BranchId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Coaches_Branch_BranchId",
                table: "Coaches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Branch",
                table: "Branch");

            migrationBuilder.RenameTable(
                name: "Branch",
                newName: "Branches");

            migrationBuilder.RenameIndex(
                name: "IX_Branch_TenantId",
                table: "Branches",
                newName: "IX_Branches_TenantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Branches",
                table: "Branches",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Tenants_TenantId",
                table: "Branches",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Branches_BranchId",
                table: "Classes",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Coaches_Branches_BranchId",
                table: "Coaches",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Tenants_TenantId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Branches_BranchId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Coaches_Branches_BranchId",
                table: "Coaches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Branches",
                table: "Branches");

            migrationBuilder.RenameTable(
                name: "Branches",
                newName: "Branch");

            migrationBuilder.RenameIndex(
                name: "IX_Branches_TenantId",
                table: "Branch",
                newName: "IX_Branch_TenantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Branch",
                table: "Branch",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Branch_Tenants_TenantId",
                table: "Branch",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Branch_BranchId",
                table: "Classes",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Coaches_Branch_BranchId",
                table: "Coaches",
                column: "BranchId",
                principalTable: "Branch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
