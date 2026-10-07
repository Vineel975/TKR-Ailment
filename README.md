USE Mcareplus_AI;
SELECT d.referenced_entity_name AS name,
       d.referenced_class_desc  AS kind,
       o.type_desc
FROM sys.sql_expression_dependencies d
LEFT JOIN sys.objects o ON o.object_id = d.referenced_id
WHERE d.referencing_id = OBJECT_ID('dbo.USP_ClaimAI_SaveClaimBundle')
ORDER BY kind, name;
