
DECLARE @ClientID varchar(200) = 'CLAIMAI';
SELECT COUNT(*) AS matches FROM dbo.auth_keys_mst WITH (NOLOCK) WHERE ClientID = @ClientID;

SELECT c.name, t.name AS type, c.max_length, c.collation_name
FROM sys.columns c JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.auth_keys_mst');
