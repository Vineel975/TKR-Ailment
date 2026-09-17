SELECT TOP 3 TPAProcedureID, ICDCode, BillingType_P51,
       BillAmount, EligibleAmount, DisallowedAmount, PayableAmount
FROM   dbo.ClaimsCoding WITH (NOLOCK)
WHERE  ClaimID = <claim> AND Slno = <slno>
ORDER  BY ID DESC;

SELECT ClaimDiagnosis, ProvisionalDiagnosis, FinalDiagnosis
FROM   dbo.Claimsdetails WITH (NOLOCK)
WHERE  ClaimID = <claim> AND Slno = <slno> AND ISNULL(Deleted,0)=0;
