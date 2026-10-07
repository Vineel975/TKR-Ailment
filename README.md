USE Mcareplus_AI;
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('Claimsdetails', 'ClaimsCoding', 'ClaimAI_Results', 'ClaimAdditionalDetails')
  AND (   COLUMN_NAME LIKE '%PackageType%'
       OR COLUMN_NAME LIKE '%Admission%'
       OR COLUMN_NAME LIKE '%Discharge%'
       OR COLUMN_NAME LIKE '%TimeOf%'
       OR COLUMN_NAME IN ('TOA', 'TOD', 'RoomDays', 'ICUDays', 'BillDate', 'BillNo', 'SlNo', 'Slno', 'IsFinal'))
ORDER BY TABLE_NAME, COLUMN_NAME;
