   SELECT p.name, TYPE_NAME(p.user_type_id) AS type
   FROM   sys.parameters p
   WHERE  p.object_id = OBJECT_ID('dbo.USP_ClaimQuery_Insert')
   ORDER  BY p.parameter_id;

      SELECT m.definition FROM sys.sql_modules m
   WHERE m.object_id = OBJECT_ID('dbo.USP_ClaimQuery_Insert');
