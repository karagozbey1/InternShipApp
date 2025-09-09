using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations._202509_SeedInternshipPeriods
{
    /// <inheritdoc />
    public partial class AddApprovalFormIdToInternshipDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalFormId",
                table: "InternshipDocuments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternshipDocuments_ApprovalFormId",
                table: "InternshipDocuments",
                column: "ApprovalFormId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDocuments_InternshipApprovalForms_ApprovalFormId",
                table: "InternshipDocuments",
                column: "ApprovalFormId",
                principalTable: "InternshipApprovalForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDocuments_InternshipApprovalForms_ApprovalFormId",
                table: "InternshipDocuments");

            migrationBuilder.DropIndex(
                name: "IX_InternshipDocuments_ApprovalFormId",
                table: "InternshipDocuments");

            migrationBuilder.DropColumn(
                name: "ApprovalFormId",
                table: "InternshipDocuments");
        }
    }
}
