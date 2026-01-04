using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddInternEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InternEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationId = table.Column<int>(type: "integer", nullable: false),
                    EvaluatorUserId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    TechnicalSkillsRating = table.Column<int>(type: "integer", nullable: false),
                    CommunicationRating = table.Column<int>(type: "integer", nullable: false),
                    TeamworkRating = table.Column<int>(type: "integer", nullable: false),
                    PunctualityRating = table.Column<int>(type: "integer", nullable: false),
                    InitiativeRating = table.Column<int>(type: "integer", nullable: false),
                    ProblemSolvingRating = table.Column<int>(type: "integer", nullable: false),
                    OverallRating = table.Column<int>(type: "integer", nullable: false),
                    StrengthsComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ImprovementAreasComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GeneralComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RecommendationComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WouldRecommendForHire = table.Column<bool>(type: "boolean", nullable: false),
                    WouldAcceptAgain = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsViewedByStudent = table.Column<bool>(type: "boolean", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternEvaluations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternEvaluations_InternshipApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "InternshipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternEvaluations_Users_EvaluatorUserId",
                        column: x => x.EvaluatorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternEvaluations_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InternEvaluations_ApplicationId",
                table: "InternEvaluations",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_InternEvaluations_CompanyId",
                table: "InternEvaluations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InternEvaluations_EvaluatorUserId",
                table: "InternEvaluations",
                column: "EvaluatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InternEvaluations_StudentId",
                table: "InternEvaluations",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InternEvaluations");
        }
    }
}
