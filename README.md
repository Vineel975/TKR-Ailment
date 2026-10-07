USE Mcareplus_AI;
SELECT dp.name AS principal, perm.permission_name, perm.state_desc,
       perm.class_desc, OBJECT_NAME(perm.major_id) AS object_name
FROM sys.database_permissions perm
JOIN sys.database_principals dp ON dp.principal_id = perm.grantee_principal_id
WHERE dp.name = 'FHPL\satyavineel.k';

SELECT r.name AS role_name
FROM sys.database_role_members rm
JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
JOIN sys.database_principals m ON m.principal_id = rm.member_principal_id
WHERE m.name = 'FHPL\satyavineel.k';
