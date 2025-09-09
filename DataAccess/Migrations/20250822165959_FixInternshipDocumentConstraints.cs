using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixInternshipDocumentConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDocuments_InternshipApplications_ApplicationId",
                table: "InternshipDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDocuments_InternshipApplications_ApplicationId",
                table: "InternshipDocuments",
                column: "ApplicationId",
                principalTable: "InternshipApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDocuments_InternshipApplications_ApplicationId",
                table: "InternshipDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDocuments_InternshipApplications_ApplicationId",
                table: "InternshipDocuments",
                column: "ApplicationId",
                principalTable: "InternshipApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
