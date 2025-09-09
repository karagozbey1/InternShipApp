using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations._202508_FixAvailablePositionsDefaults
{
    /// <inheritdoc />
    public partial class FixAvailablePositionsDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create backup table to record problematic values, then safely convert AvailablePositions to integer
            migrationBuilder.Sql(@"
                -- Create a table to keep original non-numeric values for later inspection
                CREATE TABLE IF NOT EXISTS ""InternshipOffers_AvailablePositions_Issues"" (
                    ""Id"" serial PRIMARY KEY,
                    ""OfferId"" integer,
                    ""OriginalValue"" text
                );

                -- Insert any non-numeric textual values into the issues table for manual review
                INSERT INTO ""InternshipOffers_AvailablePositions_Issues"" (""OfferId"", ""OriginalValue"")
                SELECT ""Id"", (""AvailablePositions"")::text FROM ""InternshipOffers""
                WHERE NOT ((""AvailablePositions"")::text ~ '^[0-9]+$');

                -- Create temporary integer column and populate based on current textual content.
                -- For textual non-numeric values attempt simple mappings for common words, otherwise fallback to 1.
                ALTER TABLE ""InternshipOffers"" ADD COLUMN ""AvailablePositions_temp"" integer DEFAULT 1;

                UPDATE ""InternshipOffers""
                SET ""AvailablePositions_temp"" = CASE
                    WHEN ((""AvailablePositions"")::text ~ '^[0-9]+$') THEN ((""AvailablePositions"")::integer)
                    WHEN lower((""AvailablePositions"")::text) LIKE '%çok%' THEN 10
                    WHEN lower((""AvailablePositions"")::text) LIKE '%cok%' THEN 10
                    WHEN lower((""AvailablePositions"")::text) LIKE '%iki%' THEN 2
                    WHEN lower((""AvailablePositions"")::text) LIKE '%üç%' THEN 3
                    WHEN lower((""AvailablePositions"")::text) LIKE '%uc%' THEN 3
                    ELSE 1
                END;

                -- Swap columns
                ALTER TABLE ""InternshipOffers"" DROP COLUMN IF EXISTS ""AvailablePositions"";
                ALTER TABLE ""InternshipOffers"" RENAME COLUMN ""AvailablePositions_temp"" TO ""AvailablePositions"";
                ALTER TABLE ""InternshipOffers"" ALTER COLUMN ""AvailablePositions"" SET NOT NULL;
                ALTER TABLE ""InternshipOffers"" ALTER COLUMN ""AvailablePositions"" SET DEFAULT 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert integer back to text and drop the issues table
            migrationBuilder.Sql(@"
                ALTER TABLE ""InternshipOffers"" ADD COLUMN ""AvailablePositions_temp"" text;
                UPDATE ""InternshipOffers"" SET ""AvailablePositions_temp"" = ""AvailablePositions""::text;
                ALTER TABLE ""InternshipOffers"" DROP COLUMN IF EXISTS ""AvailablePositions"";
                ALTER TABLE ""InternshipOffers"" RENAME COLUMN ""AvailablePositions_temp"" TO ""AvailablePositions"";

                -- Optionally drop the issues table if present (keep if you want to preserve data)
                DROP TABLE IF EXISTS ""InternshipOffers_AvailablePositions_Issues"";
            ");
        }
    }
}
