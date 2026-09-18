SELECT c.name FROM sys.columns c
WHERE  c.object_id = OBJECT_ID('dbo.Claimsdetails')
  AND  c.name LIKE '%Facilitychanged%' OR c.name LIKE '%AprvFacility%';
