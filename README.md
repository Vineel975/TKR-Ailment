USE Mcareplus_AI;
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Claims'
  AND (COLUMN_NAME LIKE '%Admission%' OR COLUMN_NAME LIKE '%Discharge%'
       OR COLUMN_NAME LIKE '%TimeOf%' OR COLUMN_NAME IN ('TOA', 'TOD', 'RoomDays', 'ICUDays', 'BillDate'))
ORDER BY COLUMN_NAME;

SELECT TOP 5 cd.ClaimID, cd.Slno, cd.RequestTypeID, cd.isFinal, cd.Sanctionedamount
FROM dbo.Claimsdetails cd WITH (NOLOCK)
WHERE cd.ClaimTypeID = 1
  AND ISNULL(cd.Deleted, 0) = 0
  AND (cd.RequestTypeID = 3 OR (cd.RequestTypeID IN (1, 2) AND cd.isFinal = 1))
  AND ISNULL(cd.Sanctionedamount, 0) > 0
  AND NOT EXISTS (SELECT 1 FROM dbo.ClaimAI_Results r WITH (NOLOCK) WHERE r.ClaimID = cd.ClaimID)
ORDER BY cd.ID DESC;


DECLARE @ClaimID BIGINT = <ClaimID>, @Slno TINYINT = <Slno>;
EXEC dbo.USP_ClaimBillDetails_Retrieve @ClaimID = @ClaimID, @Slno = @Slno;
EXEC dbo.Usp_ClaimCoding_Retrieve      @ClaimID = @ClaimID, @Slno = @Slno, @ClaimReqTypeID = 3;
