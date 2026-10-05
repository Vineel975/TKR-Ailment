   SELECT ClientID, LEN(ClientID) AS len, ReqUrl,
          CASE WHEN ApiKey     IS NULL OR ApiKey     = '' THEN 'EMPTY' ELSE 'set' END AS ApiKey,
          CASE WHEN PrivateKey IS NULL OR PrivateKey = '' THEN 'EMPTY' ELSE 'set' END AS PrivateKey
   FROM dbo.auth_keys_mst WITH (NOLOCK)
   WHERE ClientID LIKE '%FHPL%';
