using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class FixStaffSeedEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix seed data: align MaintenanceStaff email with Identity user email
            // DbSeeder creates Identity user with staff@property.com
            // but seed data had staff@example.com - controller looks up by email so they must match
            migrationBuilder.UpdateData(
                table: "MaintenanceStaffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "Email",
                value: "staff@property.com");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MaintenanceStaffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "Email",
                value: "staff@example.com");
        }
    }
}
