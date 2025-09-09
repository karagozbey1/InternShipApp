using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixInternshipApplicationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InternshipApplications_InternshipOfferId",
                table: "InternshipApplications");

            migrationBuilder.DropIndex(
                name: "IX_InternshipApplications_StudentId",
                table: "InternshipApplications");

            migrationBuilder.AlterColumn<string>(
                name: "StudentNote",
                table: "InternshipApplications",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "InternshipApplications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_InternshipApplications_InternshipOfferId_Status",
                table: "InternshipApplications",
                columns: new[] { "InternshipOfferId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InternshipApplications_StudentId_Status",
                table: "InternshipApplications",
                columns: new[] { "StudentId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InternshipApplications_InternshipOfferId_Status",
                table: "InternshipApplications");

            migrationBuilder.DropIndex(
                name: "IX_InternshipApplications_StudentId_Status",
                table: "InternshipApplications");

            migrationBuilder.AlterColumn<string>(
                name: "StudentNote",
                table: "InternshipApplications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "InternshipApplications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_InternshipApplications_InternshipOfferId",
                table: "InternshipApplications",
                column: "InternshipOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_InternshipApplications_StudentId",
                table: "InternshipApplications",
                column: "StudentId");
        }
    }
}
