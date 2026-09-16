SELECT ClaimID, Slno, DoctorNotes, ClaimDiagnosis, BillingCorrection
FROM   dbo.Claimsdetails WITH (NOLOCK)
WHERE  ClaimID = <your claim> AND Slno = 1 AND ISNULL(Deleted,0) = 0;


await fetch('/api/claim/save', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    claimId: "<your claim>",
    slNo: 1,
    userId: 1,
    context: { roleId: 1, regionId: 1, createdUserRegionId: 1 },
    clinical: {
      patientConditionId: 269,
      doctorNotes: "ClaimAI bundle write test - " + new Date().toISOString()
    }
  })
}).then(r => r.json())



SELECT ClaimID, Slno, DoctorNotes, BillingCorrection
FROM   dbo.Claimsdetails WITH (NOLOCK)
WHERE  ClaimID = <your claim> AND Slno = 1 AND ISNULL(Deleted,0) = 0;
