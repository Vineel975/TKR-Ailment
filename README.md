SELECT Sanctionedamount, EligibleAmount, BillingType_P51
FROM   dbo.Claimsdetails WITH (NOLOCK)
WHERE  ClaimID = <a recently saved claim> AND Slno = 1 AND ISNULL(Deleted,0)=0;
