using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInternshipDocumentRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDocuments_Companies_CompanyId",
                table: "InternshipDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDocuments_Users_StudentId",
                table: "InternshipDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDocuments_Companies_CompanyId",
                table: "InternshipDocuments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDocuments_Users_StudentId",
                table: "InternshipDocuments",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDocuments_Companies_CompanyId",
                table: "InternshipDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDocuments_Users_StudentId",
                table: "InternshipDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDocuments_Companies_CompanyId",
                table: "InternshipDocuments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDocuments_Users_StudentId",
                table: "InternshipDocuments",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
