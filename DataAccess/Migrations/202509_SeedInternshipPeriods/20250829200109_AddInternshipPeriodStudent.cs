using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations._202509_SeedInternshipPeriods
{
    /// <inheritdoc />
    public partial class AddInternshipPeriodStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipPeriods_Users_StudentId",
                table: "InternshipPeriods");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "InternshipPeriods",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipPeriods_StudentId",
                table: "InternshipPeriods",
                newName: "IX_InternshipPeriods_UserId");

            migrationBuilder.CreateTable(
                name: "InternshipPeriodStudents",
                columns: table => new
                {
                    InternshipPeriodId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternshipPeriodStudents", x => new { x.InternshipPeriodId, x.StudentId });
                    table.ForeignKey(
                        name: "FK_InternshipPeriodStudents_InternshipPeriods_InternshipPeriod~",
                        column: x => x.InternshipPeriodId,
                        principalTable: "InternshipPeriods",
                        principalColumn: "PeriodId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternshipPeriodStudents_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InternshipPeriodStudents_StudentId",
                table: "InternshipPeriodStudents",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipPeriods_Users_UserId",
                table: "InternshipPeriods",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipPeriods_Users_UserId",
                table: "InternshipPeriods");

            migrationBuilder.DropTable(
                name: "InternshipPeriodStudents");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "InternshipPeriods",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipPeriods_UserId",
                table: "InternshipPeriods",
                newName: "IX_InternshipPeriods_StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipPeriods_Users_StudentId",
                table: "InternshipPeriods",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
