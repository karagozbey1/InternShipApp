using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MigrationUpdateServicesAndEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "InternshipPeriods",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "InternshipDocuments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "InternshipDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InternshipDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CommentStatus",
                table: "InternshipComments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InternshipApplications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RecipientId",
                table: "ChatMessages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternshipPeriods_StudentId",
                table: "InternshipPeriods",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_InternshipDocuments_CompanyId",
                table: "InternshipDocuments",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipDocuments_Companies_CompanyId",
                table: "InternshipDocuments",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipPeriods_Users_StudentId",
                table: "InternshipPeriods",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipDocuments_Companies_CompanyId",
                table: "InternshipDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipPeriods_Users_StudentId",
                table: "InternshipPeriods");

            migrationBuilder.DropIndex(
                name: "IX_InternshipPeriods_StudentId",
                table: "InternshipPeriods");

            migrationBuilder.DropIndex(
                name: "IX_InternshipDocuments_CompanyId",
                table: "InternshipDocuments");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "InternshipPeriods");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "InternshipDocuments");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "InternshipDocuments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "InternshipDocuments");

            migrationBuilder.DropColumn(
                name: "CommentStatus",
                table: "InternshipComments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InternshipApplications");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "ChatMessages");
        }
    }
}
