USE Mcareplus_AI;

-- 1. Permissions granted directly to each login
SELECT dp.name                    AS db_user,
       sp.name                    AS login_name,
       perm.permission_name,
       perm.state_desc,
       OBJECT_NAME(perm.major_id) AS object_name
FROM sys.database_permissions perm
JOIN sys.database_principals dp ON dp.principal_id = perm.grantee_principal_id
LEFT JOIN sys.server_principals sp ON sp.sid = dp.sid
WHERE sp.name IN ('ZAPSIGHTREPORTINGUSER', 'FHPL\svc_claimai')   -- Spectra/worker, ClaimAI
ORDER BY login_name, object_name;

-- 2. Database roles each login belongs to (access can also come from roles)
SELECT sp.name AS login_name, dp.name AS db_user, r.name AS role_name
FROM sys.database_role_members rm
JOIN sys.database_principals r  ON r.principal_id  = rm.role_principal_id
JOIN sys.database_principals dp ON dp.principal_id = rm.member_principal_id
LEFT JOIN sys.server_principals sp ON sp.sid = dp.sid
WHERE sp.name IN ('ZAPSIGHTREPORTINGUSER', 'FHPL\svc_claimai')
ORDER BY login_name, role_name;
