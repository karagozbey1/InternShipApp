using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MigrationAddUniversityAndProvince : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniversityName",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "UniversityId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaxNumber",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "iller",
                columns: table => new
                {
                    il_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    il_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_iller", x => x.il_id);
                });

            migrationBuilder.CreateTable(
                name: "universiteler",
                columns: table => new
                {
                    uni_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    uni_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    il_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_universiteler", x => x.uni_id);
                    table.ForeignKey(
                        name: "FK_universiteler_iller_il_id",
                        column: x => x.il_id,
                        principalTable: "iller",
                        principalColumn: "il_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UniversityId",
                table: "Users",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_universiteler_il_id",
                table: "universiteler",
                column: "il_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_universiteler_UniversityId",
                table: "Users",
                column: "UniversityId",
                principalTable: "universiteler",
                principalColumn: "uni_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_universiteler_UniversityId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "universiteler");

            migrationBuilder.DropTable(
                name: "iller");

            migrationBuilder.DropIndex(
                name: "IX_Users_UniversityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UniversityId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "UniversityName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaxNumber",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
