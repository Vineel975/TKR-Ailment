SELECT ClaimDiagnosis, DoctorNotes, AdditionalRemarks
FROM   dbo.Claimsdetails WITH (NOLOCK)
WHERE  ClaimID = <claim> AND Slno = <slno> AND ISNULL(Deleted,0)=0;

SELECT TPAProcedureID, IssueID, ICD10Code, BillAmount, EligibleAmount
FROM   dbo.ClaimsCoding WITH (NOLOCK)
WHERE  ClaimID = <claim> AND Slno = <slno> ORDER BY ID DESC;
