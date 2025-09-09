using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations.SeedInternshipPeriods
{
    public partial class SeedDefaultInternshipPeriods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InternshipPeriods",
                columns: new[] { "PeriodId", "PeriodName", "StartDate", "EndDate", "StudentId" },
                values: new object[,]
                {
                    { 1, "15 Haziran - 20 Aðustos 2025", new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 20, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, "01 Temmuz - 31 Ekim 2025", new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 31, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, "01 Eylül - 31 Aralýk 2025", new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc), null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InternshipPeriods",
                keyColumn: "PeriodId",
                keyValues: new object[] { 1, 2, 3 });
        }
    }
}
