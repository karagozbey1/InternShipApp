-- Manual SQL migration stub. Run EF migrations normally using dotnet ef migrations add
-- This file is informational and not applied automatically.

-- Add MonthlySalary and MonthlySalaryCurrency columns to InternshipOffers
ALTER TABLE "InternshipOffers"
ADD COLUMN "MonthlySalary" numeric(18,2) NULL;

ALTER TABLE "InternshipOffers"
ADD COLUMN "MonthlySalaryCurrency" integer NOT NULL DEFAULT 1;

-- Ensure PeriodId is NOT NULL (only if table already has periods for all offers)
-- ALTER TABLE "InternshipOffers"
-- ALTER COLUMN "PeriodId" SET NOT NULL;
