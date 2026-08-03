-- Phase 1: auto-approval decision column on the staging results table.
-- 1 = staging determined the claim WOULD auto-approve (min-of-three < threshold,
--     clean approve). 0 = staging ran but auto-approval did not apply (doctor acts).
-- NULL = legacy rows / not evaluated.
--
-- Phase 1 RECORDS this decision only; no money-moving action is executed yet.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'AutoApproved'
      AND Object_ID = Object_ID(N'dbo.ClaimAI_Results')
)
BEGIN
    ALTER TABLE dbo.ClaimAI_Results ADD AutoApproved BIT NULL;
END
GO

-- BillingStateJson already exists in ClaimAI_Results (written by the iframe Save
-- flow). Staging now ALSO writes it when a claim is auto-approval eligible, so the
-- correct Approvals checkbox is pre-checked when the doctor opens the claim.
-- This guard only adds the column if, in some environment, it is missing.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'BillingStateJson'
      AND Object_ID = Object_ID(N'dbo.ClaimAI_Results')
)
BEGIN
    ALTER TABLE dbo.ClaimAI_Results ADD BillingStateJson NVARCHAR(MAX) NULL;
END
GO
