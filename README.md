SELECT HAS_PERMS_BY_NAME('dbo.ClaimBillDetails', 'TYPE', 'EXECUTE') AS bill,
       HAS_PERMS_BY_NAME('dbo.ClaimDeductionDetails', 'TYPE', 'EXECUTE') AS deduction,
       HAS_PERMS_BY_NAME('dbo.ClaimsServiceDetails', 'TYPE', 'EXECUTE') AS service;
