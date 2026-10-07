<img width="314" height="236" alt="image" src="https://github.com/user-attachments/assets/4d971bc2-d088-4778-ab16-e665fe75339d" />


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
