SELECT ClaimID,
       CHARINDEX('conditionTests', AnalysisJson) AS found_anywhere,
       DATALENGTH(AnalysisJson)                  AS json_size
FROM   dbo.ClaimAI_Results WITH (NOLOCK)
WHERE  ClaimID = <claim> AND SlNo = 1;
