using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations._202508_AddOfferDurationEnum
{
    /// <inheritdoc />
    public partial class AddOfferDurationEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "InternshipOffers");

            // Do not alter AvailablePositions to avoid casting issues with existing data

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "InternshipOffers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Kisa");

            // Ensure existing rows get a sensible default
            migrationBuilder.Sql("UPDATE \"InternshipOffers\" SET \"Duration\" = 'Kisa' WHERE \"Duration\" IS NULL OR \"Duration\" = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "InternshipOffers");

            migrationBuilder.AddColumn<string>(
                name: "EducationLevel",
                table: "InternshipOffers",
                type: "text",
                nullable: true);
        }
    }
}
