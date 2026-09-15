SELECT p.name, TYPE_NAME(p.user_type_id), p.max_length, p.parameter_id
FROM   sys.parameters p
WHERE  p.object_id IN (OBJECT_ID('dbo.USP_CLA_SaveClaimCodingData'),
                       OBJECT_ID('dbo.Usp_ClaimBillDetails_Insert'))
ORDER  BY p.object_id, p.parameter_id;
