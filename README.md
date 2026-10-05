SELECT ClientID,
       LEN(ClientID)        AS len,
       DATALENGTH(ClientID) AS bytes,
       CONVERT(varbinary(100), ClientID) AS raw_bytes,
       ReqUrl,
       CASE WHEN ApiKey     IS NULL OR ApiKey     = '' THEN 'EMPTY' ELSE 'set' END AS ApiKey,
       CASE WHEN PrivateKey IS NULL OR PrivateKey = '' THEN 'EMPTY' ELSE 'set' END AS PrivateKey
FROM dbo.auth_keys_mst WITH (NOLOCK);
