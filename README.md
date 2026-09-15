SELECT OBJECT_ID('dbo.ClaimAI_Results') AS table_exists;   -- NULL = missing

EXEC dbo.USP_ClaimMedicalScrutiny_retrieve
     @ClaimID = <a failing claim>, @Slno = 1;
