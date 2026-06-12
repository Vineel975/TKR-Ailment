SELECT ID, ClaimLimit,
       LEN(Remarks)               AS RemarksLen,
       SUBSTRING(Remarks, 200, 900) AS RemarksTail
FROM BPSIConditions WITH(NOLOCK)
WHERE ID IN (9934927, 9934929);
