using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MigrationAddDİaryEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvailablePositions",
                table: "InternshipOffers",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            // Status'u integer yapmak için güvenli SQL ekle
            migrationBuilder.Sql(@"
                ALTER TABLE ""InternshipApplications"" ADD COLUMN ""StatusTemp"" integer;

                UPDATE ""InternshipApplications""
                SET ""StatusTemp"" = CASE ""Status""
                    WHEN 'Pending' THEN 0
                    WHEN 'Approved' THEN 1
                    WHEN 'Rejected' THEN 2
                    ELSE 0
                END;

                ALTER TABLE ""InternshipApplications"" DROP COLUMN ""Status"";
                ALTER TABLE ""InternshipApplications"" RENAME COLUMN ""StatusTemp"" TO ""Status"";
            ");

            migrationBuilder.CreateTable(
                name: "DiaryEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryEntries_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_StudentId",
                table: "DiaryEntries",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "AvailablePositions",
                table: "InternshipOffers",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            // Status'u tekrar string yapmak için güvenli SQL ekle
            migrationBuilder.Sql(@"
                ALTER TABLE ""InternshipApplications"" ADD COLUMN ""StatusTemp"" text;

                UPDATE ""InternshipApplications""
                SET ""StatusTemp"" = CASE ""Status""
                    WHEN 0 THEN 'Pending'
                    WHEN 1 THEN 'Approved'
                    WHEN 2 THEN 'Rejected'
                    ELSE 'Pending'
                END;

                ALTER TABLE ""InternshipApplications"" DROP COLUMN ""Status"";
                ALTER TABLE ""InternshipApplications"" RENAME COLUMN ""StatusTemp"" TO ""Status"";
            ");
        }
    }
}
