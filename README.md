SELECT SUBSTRING(AnalysisJson, 2700, 700) AS around_condition_tests
FROM   dbo.ClaimAI_Results WITH (NOLOCK)
WHERE  ClaimID = <claim> AND SlNo = 1;
