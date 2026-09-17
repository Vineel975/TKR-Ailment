SELECT ID, ClaimID, Slno, ICDCode, EligibleAmount, Deleted
FROM   dbo.ClaimsCoding WITH (NOLOCK)
WHERE  ClaimID = <claim> AND Slno = <slno>
ORDER  BY ID;

SELECT c.name, TYPE_NAME(c.user_type_id) AS type
FROM   sys.columns c
WHERE  c.object_id = OBJECT_ID('dbo.Claimsdetails')
  AND  c.name LIKE '%iagnos%'
ORDER  BY c.column_id;
