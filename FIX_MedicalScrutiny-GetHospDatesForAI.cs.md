# Add — anonymous `GetHospDatesForAI` endpoint (approach A for DOA / DOD)

Same pattern as `GetBSIForAI`, but for admission/discharge dates. ClaimAI now asks Spectra for the authoritative DOA/DOD (from `Usp_ClaimHospitalizationDetails` — the exact query the Hospitalization Details tab uses) instead of collecting them in the browser or re-querying itself.

**File:** `Enrollment/Controllers/MedicalScrutinyController.cs`

## New action

```csharp
// GET /MedicalScrutiny/GetHospDatesForAI?ClaimID=<id>
// Anonymous endpoint returning the admission/discharge dates for a claim, read from
// Usp_ClaimHospitalizationDetails (first result set — DateofAdmission / DateofDischarge),
// the same source the Hospitalization Details tab uses. Returned as dd-MMM-yyyy strings.
[HttpGet]
[AllowAnonymous]
[OverrideAuthorization]
public string GetHospDatesForAI(long ClaimID, byte SlNo = 0)
{
    try
    {
        // Optional shared secret — same key the staging scheduler uses.
        string stagingKey = System.Configuration.ConfigurationManager.AppSettings["StagingApiKey"] ?? "";
        if (!string.IsNullOrEmpty(stagingKey))
        {
            string reqKey = Request.Headers["x-staging-key"] ?? Request.QueryString["key"] ?? "";
            if (reqKey != stagingKey)
                return Newtonsoft.Json.JsonConvert.SerializeObject(new { error = "Unauthorized" });
        }

        string connStr = GetStagingConnString();

        // Latest SlNo for this claim (if not supplied)
        if (SlNo == 0)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT TOP 1 Slno FROM Claimsdetails WITH (NOLOCK) WHERE ClaimID=@cid AND ISNULL(Deleted,0)=0 ORDER BY Slno";
                cmd.Parameters.AddWithValue("@cid", ClaimID);
                var v = cmd.ExecuteScalar();
                SlNo = (v != null && v != DBNull.Value) ? Convert.ToByte(v) : (byte)1;
            }
        }

        string doa = null, dod = null;
        using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
        {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "Usp_ClaimHospitalizationDetails";
            cmd.CommandTimeout = 120;
            cmd.Parameters.AddWithValue("@ClaimID", ClaimID);
            cmd.Parameters.AddWithValue("@Slno", SlNo);
            using (var rdr = cmd.ExecuteReader())
            {
                // First result set, first row (the hospital details block).
                if (rdr.Read())
                {
                    int iDoa = ColIndex(rdr, "DateofAdmission");
                    int iDod = ColIndex(rdr, "DateofDischarge");
                    if (iDoa >= 0 && !rdr.IsDBNull(iDoa)) doa = Convert.ToDateTime(rdr.GetValue(iDoa)).ToString("dd-MMM-yyyy");
                    if (iDod >= 0 && !rdr.IsDBNull(iDod)) dod = Convert.ToDateTime(rdr.GetValue(iDod)).ToString("dd-MMM-yyyy");
                }
            }
        }

        return Newtonsoft.Json.JsonConvert.SerializeObject(new { admissionDate = doa, dischargeDate = dod });
    }
    catch (Exception ex)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(new { error = ex.Message });
    }
}

// Helper — case-insensitive column index, -1 if absent.
private static int ColIndex(System.Data.IDataReader rdr, string name)
{
    for (int i = 0; i < rdr.FieldCount; i++)
        if (string.Equals(rdr.GetName(i), name, StringComparison.OrdinalIgnoreCase))
            return i;
    return -1;
}
```

## ClaimAI side (already done)

`db.ts` → `getHospitalizationDates` now calls this endpoint first (via `fetchSpectraHospDates`, hard 3s timeout, safe fallback) and uses `{ admissionDate, dischargeDate }`. If the endpoint isn't set/reachable it falls back to the direct DB read. Uses the same env vars as BSI:

```
SPECTRA_BASE_URL = https://spectra-ai.fhpl.net
STAGING_API_KEY  = <same as Spectra's StagingApiKey>   # optional
```

The audit/start route already calls `getHospitalizationDates` and overrides `spectraFields.admissionDate` / `dischargeDate` with the result — so this flows straight into the DOA/DOD validation. The cataract "discharge = admission" rule still applies on top.

## What this replaces

- The browser DOA/DOD collection in `newer_index_UPDATED.cshtml` is now **redundant** — the AI gets the dates server-side from Spectra. You can leave it (harmless, the endpoint overrides it) or revert those two blocks.
- The direct DB read inside `getHospitalizationDates` is now the **fallback**, used only if the endpoint is unreachable.

## Notes / confirm
- Reads the **first** result set of `Usp_ClaimHospitalizationDetails` (the hospital-details block with `C.DateofAdmission` / `C.DateofDischarge`), matching the tab.
- Returns `dd-MMM-yyyy`; the AI uses the strings as-is.
- Spectra change → rebuild/redeploy Spectra. ClaimAI (`db.ts`) → redeploy. Reprocess claims so the corrected DOA/DOD are captured in the validation.
