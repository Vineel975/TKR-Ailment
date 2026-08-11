SELECT Type, StageID, COUNT(*) AS Cnt, SUM(Amount) AS TotalAmt
FROM MemberUtilization WITH (NOLOCK)      -- adjust table if the rows live elsewhere
WHERE ISNULL(Deleted,0) = 0
GROUP BY Type, StageID
ORDER BY Type, StageID;
