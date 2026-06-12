SELECT ID, ClaimLimit, CAST(Remarks AS NVARCHAR(MAX)) AS FullRemarks
FROM BPSIConditions WITH(NOLOCK)
WHERE ID IN (9934927, 9934929);   -- a 250000 row and a 450000 row (claim 4)
