using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ExtendDiaryEntryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdvisorFeedback",
                table: "DiaryEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "DiaryEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "DiaryEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Challenges",
                table: "DiaryEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "DiaryEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LearnedTopics",
                table: "DiaryEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MentorName",
                table: "DiaryEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "DiaryEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solutions",
                table: "DiaryEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WorkingHours",
                table: "DiaryEntries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvisorFeedback",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "Challenges",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "LearnedTopics",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "MentorName",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "Solutions",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "WorkingHours",
                table: "DiaryEntries");
        }
    }
}
