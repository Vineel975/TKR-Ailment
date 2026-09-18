SELECT o.name AS table_name, c.name AS column_name
FROM   sys.columns c
JOIN   sys.objects o ON o.object_id = c.object_id
WHERE  c.name IN ('IsAprvFacilitychanged', 'IsFacilityChanged')
  AND  o.type = 'U'
ORDER  BY o.name, c.name;


SELECT m.definition FROM sys.sql_modules m
WHERE  m.object_id = OBJECT_ID('dbo.USP_ClaimMedicalScrutiny_retrieve');
