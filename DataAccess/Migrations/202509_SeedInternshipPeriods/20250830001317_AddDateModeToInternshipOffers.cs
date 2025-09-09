using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations._202509_SeedInternshipPeriods
{
    /// <inheritdoc />
    public partial class AddDateModeToInternshipOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure DateMode column exists: add it with default if missing
            migrationBuilder.AddColumn<int>(
                name: "DateMode",
                table: "InternshipOffers",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "AvailablePositions",
                table: "InternshipOffers",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AvailablePositions",
                table: "InternshipOffers",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            // Remove DateMode column added in Up
            migrationBuilder.DropColumn(
                name: "DateMode",
                table: "InternshipOffers");
        }
    }
}
