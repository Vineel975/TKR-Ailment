SELECT cd.ID           AS ClaimDetailsID,
       cd.ApprovedFacilityID,
       cad.ClaimDetailsId,
       cad.IsAprvFacilitychanged
FROM   dbo.Claimsdetails cd WITH (NOLOCK)
LEFT   JOIN dbo.ClaimAdditionalDetails cad WITH (NOLOCK)
       ON cad.ClaimDetailsId = cd.ID
WHERE  cd.ClaimID = 26082743931 AND cd.Slno = 1 AND ISNULL(cd.Deleted,0) = 0;
