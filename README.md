SELECT Doc.Id, Doc.FileName, Doc.SystemFileName, Doc.isOldDoc, Mp.ProviderID
   FROM ProviderTariffDocs Doc
   JOIN ProviderTariff_Map Mp ON Doc.Id = Mp.DocumentId
   WHERE Mp.ProviderID = <thisProviderId> AND Doc.Status=1 AND Mp.Status=1
   ORDER BY Doc.FileName;
