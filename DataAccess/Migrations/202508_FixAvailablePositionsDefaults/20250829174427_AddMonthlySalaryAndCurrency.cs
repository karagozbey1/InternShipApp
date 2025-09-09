using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations._202508_FixAvailablePositionsDefaults
{
    /// <inheritdoc />
    public partial class AddMonthlySalaryAndCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySalary",
                table: "InternshipOffers",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonthlySalaryCurrency",
                table: "InternshipOffers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlySalary",
                table: "InternshipOffers");

            migrationBuilder.DropColumn(
                name: "MonthlySalaryCurrency",
                table: "InternshipOffers");
        }
    }
}
