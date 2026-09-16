document.querySelector('script[src*="_next"]') ? 'app loaded' : ''

SELECT ClaimID, Slno, BillingCorrection, PackageAmount, Sanctionedamount,
       DoctorNotes, ClaimDiagnosis
FROM   dbo.Claimsdetails WITH (NOLOCK)
WHERE  ClaimID = <claim> AND Slno = <slno> AND ISNULL(Deleted,0)=0;

SELECT ServiceID, BillAmount, DeductionAmount, EligibleAmount, SanctionedAmount
FROM   dbo.ClaimsServiceDetails WITH (NOLOCK)
WHERE  ClaimID = <claim> AND Slno = <slno> AND Deleted = 0
ORDER  BY ServiceID;
