using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentIdToInternshipDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ApplicationId",
                table: "InternshipDocuments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "InternshipDocuments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternshipDocuments_StudentId",
                table: "InternshipDocuments",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDocuments_Users_StudentId",
                table: "InternshipDocuments",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDocuments_Users_StudentId",
                table: "InternshipDocuments");

            migrationBuilder.DropIndex(
                name: "IX_InternshipDocuments_StudentId",
                table: "InternshipDocuments");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "InternshipDocuments");

            migrationBuilder.AlterColumn<int>(
                name: "ApplicationId",
                table: "InternshipDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
