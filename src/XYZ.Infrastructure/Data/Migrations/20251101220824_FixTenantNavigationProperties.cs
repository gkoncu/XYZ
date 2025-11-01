using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XYZ.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTenantNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Tenants_TenantId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Tenants_TenantId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Coaches_HeadCoachId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Tenants_TenantId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Coaches_Tenants_TenantId",
                table: "Coaches");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Tenants_TenantId",
                table: "Students");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Tenants_TenantId",
                table: "Admins",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Tenants_TenantId",
                table: "Announcements",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Coaches_HeadCoachId",
                table: "Classes",
                column: "HeadCoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Tenants_TenantId",
                table: "Classes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Coaches_Tenants_TenantId",
                table: "Coaches",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Tenants_TenantId",
                table: "Students",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Tenants_TenantId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Tenants_TenantId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Coaches_HeadCoachId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Tenants_TenantId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Coaches_Tenants_TenantId",
                table: "Coaches");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Tenants_TenantId",
                table: "Students");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Tenants_TenantId",
                table: "Admins",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Tenants_TenantId",
                table: "Announcements",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Coaches_HeadCoachId",
                table: "Classes",
                column: "HeadCoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Tenants_TenantId",
                table: "Classes",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Coaches_Tenants_TenantId",
                table: "Coaches",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Tenants_TenantId",
                table: "Students",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
