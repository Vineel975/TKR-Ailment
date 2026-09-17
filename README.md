SELECT o.name
FROM   sys.sql_modules m JOIN sys.objects o ON o.object_id = m.object_id
WHERE  m.definition LIKE '%Sanctionedamount%' AND o.type = 'P'
ORDER  BY o.name;
