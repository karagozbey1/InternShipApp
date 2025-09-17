using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MiıgrationFovorities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SliderItems_Slider_SliderId",
                table: "SliderItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Slider_TempId",
                table: "Slider");

            migrationBuilder.RenameTable(
                name: "Slider",
                newName: "Sliders");

            migrationBuilder.RenameColumn(
                name: "TempId",
                table: "Sliders",
                newName: "DisplayOrder");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Sliders",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Sliders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Sliders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Sliders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sliders",
                table: "Sliders",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "OfferFavorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    OfferId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferFavorites_InternshipOffers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "InternshipOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferFavorites_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfferFavorites_OfferId",
                table: "OfferFavorites",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferFavorites_StudentId_OfferId",
                table: "OfferFavorites",
                columns: new[] { "StudentId", "OfferId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SliderItems_Sliders_SliderId",
                table: "SliderItems",
                column: "SliderId",
                principalTable: "Sliders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SliderItems_Sliders_SliderId",
                table: "SliderItems");

            migrationBuilder.DropTable(
                name: "OfferFavorites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sliders",
                table: "Sliders");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Sliders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Sliders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Sliders");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Sliders");

            migrationBuilder.RenameTable(
                name: "Sliders",
                newName: "Slider");

            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                table: "Slider",
                newName: "TempId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Slider_TempId",
                table: "Slider",
                column: "TempId");

            migrationBuilder.AddForeignKey(
                name: "FK_SliderItems_Slider_SliderId",
                table: "SliderItems",
                column: "SliderId",
                principalTable: "Slider",
                principalColumn: "TempId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
