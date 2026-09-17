SELECT c.name, TYPE_NAME(c.user_type_id) AS type, c.is_nullable
FROM   sys.columns c
WHERE  c.object_id = OBJECT_ID('dbo.ClaimsCoding')
ORDER  BY c.column_id;

SELECT m.definition
FROM   sys.sql_modules m
WHERE  m.object_id = OBJECT_ID('dbo.USP_CLA_SaveClaimCodingData');
