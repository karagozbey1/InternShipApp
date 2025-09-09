using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddInternshipApprovalForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InternshipApprovalForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    AdvisorId = table.Column<int>(type: "integer", nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeeklyWorkHours = table.Column<int>(type: "integer", nullable: false),
                    WorkplaceAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InternshipCoordinator = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CoordinatorPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CoordinatorEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InternshipDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StudentNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdvisorNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternshipApprovalForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternshipApprovalForms_InternshipApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "InternshipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternshipApprovalForms_Users_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternshipApprovalForms_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InternshipApprovalForms_AdvisorId_Status",
                table: "InternshipApprovalForms",
                columns: new[] { "AdvisorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InternshipApprovalForms_ApplicationId",
                table: "InternshipApprovalForms",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternshipApprovalForms_StudentId_Status",
                table: "InternshipApprovalForms",
                columns: new[] { "StudentId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InternshipApprovalForms");
        }
    }
}
