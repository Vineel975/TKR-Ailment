SELECT o.name
FROM   sys.sql_modules m
JOIN   sys.objects o ON o.object_id = m.object_id
WHERE  o.type = 'P'
  AND  o.name NOT LIKE '%[_][0-9][0-9][_][0-9][0-9]%'   -- skip dated backups
  AND  (m.definition LIKE '%Sanctionedamount%=%'
        OR m.definition LIKE '%set%Sanctionedamount%')
ORDER  BY o.name;
