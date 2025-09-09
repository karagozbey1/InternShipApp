using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixAvailablePositionsType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely convert text AvailablePositions to integer: create temp column, populate only numeric values, swap
            migrationBuilder.Sql(@"
                ALTER TABLE ""InternshipOffers"" ADD COLUMN ""AvailablePositions_temp"" integer DEFAULT 0;
                UPDATE ""InternshipOffers""
                SET ""AvailablePositions_temp"" = CASE WHEN ""AvailablePositions"" ~ '^[0-9]+$' THEN (""AvailablePositions""::integer) ELSE 0 END;
                ALTER TABLE ""InternshipOffers"" DROP COLUMN ""AvailablePositions"";
                ALTER TABLE ""InternshipOffers"" RENAME COLUMN ""AvailablePositions_temp"" TO ""AvailablePositions"";
                ALTER TABLE ""InternshipOffers"" ALTER COLUMN ""AvailablePositions"" SET NOT NULL;
                ALTER TABLE ""InternshipOffers"" ALTER COLUMN ""AvailablePositions"" SET DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert integer back to text safely
            migrationBuilder.Sql(@"
                ALTER TABLE ""InternshipOffers"" ADD COLUMN ""AvailablePositions_temp"" text;
                UPDATE ""InternshipOffers"" SET ""AvailablePositions_temp"" = ""AvailablePositions""::text;
                ALTER TABLE ""InternshipOffers"" DROP COLUMN ""AvailablePositions"";
                ALTER TABLE ""InternshipOffers"" RENAME COLUMN ""AvailablePositions_temp"" TO ""AvailablePositions"";
            ");
        }
    }
}
