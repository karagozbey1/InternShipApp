using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations._202508_AddOfferJobType
{
    public partial class AddOfferJobType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobType",
                table: "InternshipOffers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Internship");

            // Set default for existing rows if any null/missing
            migrationBuilder.Sql("UPDATE \"InternshipOffers\" SET \"JobType\" = 'Internship' WHERE \"JobType\" IS NULL OR \"JobType\" = ''; ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobType",
                table: "InternshipOffers");
        }
    }
}
