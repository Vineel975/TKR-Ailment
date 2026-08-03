-- Phase 2a: record the auto-save outcome so failures are queryable.
-- Staging flips the claim to stage 5 BEFORE the auto-save runs; if the save
-- partially fails, these columns capture it (the stage is not rolled back).
-- Run once against the Spectra DB.

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ClaimAI_Results' AND COLUMN_NAME = 'AutoSaveStatus'
)
BEGIN
    ALTER TABLE dbo.ClaimAI_Results ADD AutoSaveStatus NVARCHAR(20) NULL;
    -- values: 'success', 'failed', 'skipped', or NULL (auto-save not attempted)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ClaimAI_Results' AND COLUMN_NAME = 'AutoSaveError'
)
BEGIN
    ALTER TABLE dbo.ClaimAI_Results ADD AutoSaveError NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ClaimAI_Results' AND COLUMN_NAME = 'AutoSaveAt'
)
BEGIN
    ALTER TABLE dbo.ClaimAI_Results ADD AutoSaveAt DATETIME NULL;
END
GO

-- Handy query to find claims where the auto-save did not fully succeed:
--   SELECT ClaimID, SlNo, AutoApproved, AutoSaveStatus, AutoSaveError, AutoSaveAt
--   FROM dbo.ClaimAI_Results
--   WHERE AutoSaveStatus IS NOT NULL AND AutoSaveStatus <> 'success'
--   ORDER BY AutoSaveAt DESC;
