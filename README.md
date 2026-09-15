SELECT tt.name AS tvp, c.column_id, c.name,
       TYPE_NAME(c.user_type_id) AS type, c.max_length, c.is_nullable
FROM   sys.table_types tt
JOIN   sys.columns c ON c.object_id = tt.type_table_object_id
WHERE  tt.name IN ('ClaimBillDetails','ClaimDeductionDetails','ClaimsServiceDetails')
ORDER  BY tt.name, c.column_id;
