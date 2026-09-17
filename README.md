SELECT ID, TPAProcedureID, TPALevel1, TPALevel2, TPALevel3, PCSCode, ICDCode
FROM   dbo.ClaimsCoding WITH (NOLOCK)
WHERE  ID = 25355911;


SELECT Diagnosis, claimdiagnosis
FROM   dbo.Claimsdetails WITH (NOLOCK)
WHERE  ClaimID = 26082743934 AND Slno = 1 AND ISNULL(Deleted,0)=0;
