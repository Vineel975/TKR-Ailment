-- 1. Was the proc changed, and when?
SELECT o.name, o.create_date, o.modify_date
FROM   sys.objects o
WHERE  o.name LIKE '%GetIsClaimCopay%';

-- 2. What does it do?
SELECT m.definition
FROM   sys.sql_modules m
JOIN   sys.objects o ON o.object_id = m.object_id
WHERE  o.name LIKE '%GetIsClaimCopay%';

-- 3. Run it as the app does
EXEC dbo.<proc_name> @LoginUserID = '<your user id>';


SELECT COUNT(*) FROM <that_table>;
SELECT TOP 10 * FROM <that_table>;
