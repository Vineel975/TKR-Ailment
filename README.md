SELECT cd.RequestTypeID, cd.ClaimTypeID, COUNT(DISTINCT r.ClaimID) AS StagedClaims
FROM   dbo.ClaimAI_Results r WITH (NOLOCK)
JOIN   dbo.Claimsdetails cd WITH (NOLOCK)
       ON cd.ClaimID = r.ClaimID AND cd.Slno = ISNULL(r.SlNo, 1) AND ISNULL(cd.Deleted, 0) = 0
GROUP BY cd.RequestTypeID, cd.ClaimTypeID
ORDER BY StagedClaims DESC;

SELECT a.RequestTypeID, a.ClaimTypeID, COUNT(*) AS OpenInStage52,
       SUM(CASE WHEN EXISTS (SELECT 1 FROM dbo.ClaimAI_Results r WITH (NOLOCK) WHERE r.ClaimID = a.ClaimID)
                THEN 1 ELSE 0 END) AS AlreadyHaveAResult
FROM   dbo.claimActionItems a WITH (NOLOCK)
WHERE  a.ClaimStageID = 52 AND a.CloseDate IS NULL
GROUP BY a.RequestTypeID, a.ClaimTypeID;
