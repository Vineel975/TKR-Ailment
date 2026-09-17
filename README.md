SET NOCOUNT ON;
SELECT m.definition FROM sys.sql_modules m
WHERE m.object_id = OBJECT_ID('dbo.USP_CLA_SaveClaimCodingData');


SELECT claimdiagnosis FROM dbo.Claimsdetails WITH (NOLOCK)
WHERE ClaimID = 26082743934 AND Slno = 1 AND ISNULL(Deleted,0) = 0;
