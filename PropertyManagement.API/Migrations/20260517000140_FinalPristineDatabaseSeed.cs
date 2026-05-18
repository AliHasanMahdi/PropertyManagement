using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PropertyManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class FinalPristineDatabaseSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Buildings",
                columns: new[] { "Id", "Address", "City", "Name", "Type" },
                values: new object[,]
                {
                    { 1, "101 Luxury Way", "Manama", "Grandview Heights", "Residential" },
                    { 2, "202 Timber Lane", "Seef", "Maple Wood Tower", "Commercial" }
                });

            migrationBuilder.InsertData(
                table: "MaintenanceStaffs",
                columns: new[] { "Id", "AvailabilityStatus", "Email", "FullName", "Phone", "SkillType" },
                values: new object[] { 1, "Available", "staff@example.com", "Bob Builder", "555-0122", "Plumbing" });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CPR", "DateRegistered", "Email", "FullName", "Phone" },
                values: new object[,]
                {
                    { 1, "990112345", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "tenant1@example.com", "John Doe", "555-0199" },
                    { 2, "950554321", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "tenant2@example.com", "Jane Smith", "555-0144" }
                });

            migrationBuilder.InsertData(
                table: "Units",
                columns: new[] { "Id", "Amenities", "BuildingId", "Rent", "Size", "Status", "Type", "UnitNumber" },
                values: new object[,]
                {
                    { 1, "Balcony, AC", 1, 1200.00m, 85.5, "Occupied", "Apartment", "101A" },
                    { 2, "Furnished", 1, 1350.00m, 45.0, "Available", "Studio", "102B" },
                    { 3, "Conference Room", 2, 2450.00m, 120.0, "Occupied", "Office", "201" }
                });

            migrationBuilder.InsertData(
                table: "Leases",
                columns: new[] { "Id", "EndDate", "MonthlyRent", "StartDate", "Status", "TenantId", "UnitId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1200.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 1, 1 },
                    { 2, new DateTime(2027, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2450.00m, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 2, 3 }
                });

            migrationBuilder.InsertData(
                table: "MaintenanceRequests",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "MaintenanceStaffId", "Priority", "ResolvedAt", "Status", "TenantId", "TicketNumber", "Title", "UnitId" },
                values: new object[,]
                {
                    { 1, "Plumbing", new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "The pipe below the kitchen sink is constantly dripping water onto the cabinet base.", 1, "High", null, "Assigned", 1, "TKT-1001", "Leaky Kitchen Sink", 1 },
                    { 2, "Electrical", new DateTime(2026, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "The bedroom toggle switch clicks but the light fixtures do not respond.", 1, "Medium", null, "InProgress", 2, "TKT-1002", "Broken Light Switch", 3 }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "Amount", "LeaseId", "Notes", "PaymentDate", "Status" },
                values: new object[,]
                {
                    { 1, 1200.00m, 1, "Rent payment for May", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Paid" },
                    { 2, 2450.00m, 2, "First month rent deposit", new DateTime(2026, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Paid" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MaintenanceRequests",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Units",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Leases",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Leases",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MaintenanceStaffs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Units",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Units",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Buildings",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
