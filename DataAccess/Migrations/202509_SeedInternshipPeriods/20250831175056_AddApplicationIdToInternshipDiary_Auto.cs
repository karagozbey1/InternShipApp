using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations._202509_SeedInternshipPeriods
{
    /// <inheritdoc />
    public partial class AddApplicationIdToInternshipDiary_Auto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationId",
                table: "InternshipDiaries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternshipDiaries_ApplicationId",
                table: "InternshipDiaries",
                column: "ApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDiaries_InternshipApplications_ApplicationId",
                table: "InternshipDiaries",
                column: "ApplicationId",
                principalTable: "InternshipApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDiaries_InternshipApplications_ApplicationId",
                table: "InternshipDiaries");

            migrationBuilder.DropIndex(
                name: "IX_InternshipDiaries_ApplicationId",
                table: "InternshipDiaries");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "InternshipDiaries");
        }
    }
}
