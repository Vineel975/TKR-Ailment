SELECT Doc.Id, Doc.FileName, Doc.SystemFileName, Doc.isOldDoc, Mp.ProviderID
   FROM ProviderTariffDocs Doc
   JOIN ProviderTariff_Map Mp ON Doc.Id = Mp.DocumentId
   WHERE Mp.ProviderID = <thisProviderId> AND Doc.Status=1 AND Mp.Status=1
   ORDER BY Doc.FileName;


SELECT TOP 1 Slno FROM Claimsdetails
WHERE ClaimID = <yourClaimID> AND ISNULL(Deleted,0)=0
ORDER BY Slno;

EXEC USP_ClaimMedicalScrutiny_Retrieve @ClaimID = <yourClaimID>, @Slno = <slno>;
