SELECT ID, DiseaseCode, Description, Level
FROM   dbo.ICD10 WITH (NOLOCK)
WHERE  DiseaseCode IN ('H25.12','H25.1','H25','H2612') OR ID = 13323;
