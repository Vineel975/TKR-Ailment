# Add — anonymous `GetBSIForAI` endpoint (approach A for Balance Sum Insured)

So the ClaimAI Summary shows the **same** Reserved / Blocked / Utilized as Spectra, the AI now asks Spectra for its own computed BSI (`Main.GetBSI`) instead of reimplementing it. Add one small endpoint that ClaimAI can call server-to-server.

**File:** `Enrollment/Controllers/MedicalScrutinyController.cs`

## New action

```csharp
// GET /MedicalScrutiny/GetBSIForAI?ClaimID=<id>
// Anonymous variant of GetBSI for the ClaimAI engine. Resolves MemberPolicyID /
// SITypeID / SlNo the SAME way the Medical Scrutiny screen does (via
// ClaimMedicalScrutiny_LoadVM -> USP_ClaimMedicalScrutiny_Retrieve, i.e. basicData),
// then returns Main.GetBSI(...) — so Reserved/Blocked/Utilized match Spectra exactly.
[HttpGet]
[AllowAnonymous]
[OverrideAuthorization]
public string GetBSIForAI(long ClaimID, byte SlNo = 0)
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

        // MemberPolicyID + SITypeID from the SAME source the screen uses (basicData),
        // so SITypeID is exactly what the UI passes to GetBSI (5 = floater, 6 = individual).
        long memberPolicyId = 0;
        int  siTypeId       = 0;
        System.Data.DataTable dtBasic = _objMadicalScrutinyVM.ClaimMedicalScrutiny_LoadVM(ClaimID, SlNo, false);
        if (dtBasic != null && dtBasic.Rows.Count > 0)
        {
            var r = dtBasic.Rows[0];
            if (dtBasic.Columns.Contains("SITypeID") && r["SITypeID"] != DBNull.Value)
                siTypeId = Convert.ToInt32(r["SITypeID"]);
            if (dtBasic.Columns.Contains("MemberPolicyID") && r["MemberPolicyID"] != DBNull.Value)
                memberPolicyId = Convert.ToInt64(r["MemberPolicyID"]);
            else if (dtBasic.Columns.Contains("MemberpolicyID") && r["MemberpolicyID"] != DBNull.Value)
                memberPolicyId = Convert.ToInt64(r["MemberpolicyID"]);
        }

        // Fallback: MemberPolicyID straight from Claims if basicData didn't carry it.
        if (memberPolicyId == 0)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT TOP 1 CAST(MemberPolicyID AS BIGINT) FROM Claims WITH (NOLOCK) WHERE ID=@cid AND ISNULL(Deleted,0)=0";
                cmd.Parameters.AddWithValue("@cid", ClaimID);
                var v = cmd.ExecuteScalar();
                if (v != null && v != DBNull.Value) memberPolicyId = Convert.ToInt64(v);
            }
        }

        if (memberPolicyId == 0 || siTypeId == 0)
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { error = "Could not resolve MemberPolicyID/SITypeID for claim " + ClaimID });

        // Real ClaimID + SlNo so the balance excludes this claim, exactly like the
        // Medical Scrutiny BSI panel (GETBSIDetails passes hdnClaimID / hdnClaimSlNo).
        BSIinfo objBSI = new Main().GetBSI(memberPolicyId, siTypeId, ClaimID, SlNo);
        return Newtonsoft.Json.JsonConvert.SerializeObject(objBSI);
    }
    catch (Exception ex)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(new { error = ex.Message });
    }
}
```

## ClaimAI side (already done)

`db.ts` → `getBalanceSumInsured` now calls this endpoint first and maps the result; if it's unreachable it falls back to the DB reimplementation. Set two env vars on the ClaimAI app:

```
SPECTRA_BASE_URL = https://<your-spectra-host>          # e.g. https://spectra.internal or http://localhost:PORT
STAGING_API_KEY  = <same value as Spectra's StagingApiKey>   # optional; only if you set StagingApiKey
```

## Result

- ClaimAI Summary's **Reserved / Blocked / Utilized** are now Spectra's own numbers (Type-4 / Type-2 / Type-3 sums from `Main.GetBSI`) — they match by construction.
- If `SPECTRA_BASE_URL` isn't set or Spectra is unreachable, ClaimAI transparently falls back to the DB query (Blocked correct; Utilized/Reserved as before).

## Notes / confirm
- **SITypeID (fixed):** now resolved from `ClaimMedicalScrutiny_LoadVM` (the `basicData` proc `USP_ClaimMedicalScrutiny_Retrieve`) — the exact source the screen uses for `hdnSITypeID` (`5` = floater, `6` = individual). Confirmed against your live call: MemberPolicyID 113522792 → SITypeID 5.
- **Column names:** reads `SITypeID` and `MemberPolicyID` from that DataTable defensively (with a `Claims` fallback for MemberPolicyID). If `ClaimMedicalScrutiny_LoadVM` names the member-policy column differently, adjust the two `Columns.Contains(...)` checks.
- **ClaimID/SlNo:** passed real (not 0), so the balance excludes the current claim — matching the Medical Scrutiny panel. Your test used `ClaimID=0` (member-level total, which includes this claim's own block); the panel value uses the real ClaimID.
- Endpoint is anonymous but guarded by the same `StagingApiKey` header as the staging scheduler; keep them consistent.
- Spectra change → rebuild/redeploy Spectra. ClaimAI change (`db.ts`) → redeploy ClaimAI. No reprocessing — `/api/bsi` runs live per claim.
