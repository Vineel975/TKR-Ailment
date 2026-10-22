-- 1. Does the login exist on the server, and is it SQL or Windows?
SELECT name                AS LoginName,
       type_desc           AS LoginType,      -- SQL_LOGIN = SQL Server login, WINDOWS_LOGIN = Windows/domain account
       is_disabled,
       default_database_name,
       create_date
FROM   sys.server_principals
WHERE  name = N'PRD_ZPST_DW_USR' OR name LIKE N'%\PRD_ZPST_DW_USR';

-- 2. Is the login mapped to a user in McarePlus?
USE [McarePlus];
SELECT dp.name AS UserName, dp.type_desc AS UserType, sp.name AS MappedToLogin
FROM   sys.database_principals dp
JOIN   sys.server_principals sp ON sp.sid = dp.sid
WHERE  sp.name = N'PRD_ZPST_DW_USR' OR sp.name LIKE N'%\PRD_ZPST_DW_USR';

-- 3. Is the login mapped to a user in Mcareplus_Audit?
USE [Mcareplus_Audit];
SELECT dp.name AS UserName, dp.type_desc AS UserType, sp.name AS MappedToLogin
FROM   sys.database_principals dp
JOIN   sys.server_principals sp ON sp.sid = dp.sid
WHERE  sp.name = N'PRD_ZPST_DW_USR' OR sp.name LIKE N'%\PRD_ZPST_DW_USR';
