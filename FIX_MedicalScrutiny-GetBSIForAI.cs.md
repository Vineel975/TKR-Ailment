# Add — anonymous `GetBSIForAI` endpoint (approach A for Balance Sum Insured)

So the ClaimAI Summary shows the **same** Reserved / Blocked / Utilized as Spectra, the AI now asks Spectra for its own computed BSI (`Main.GetBSI`) instead of reimplementing it. Add one small endpoint that ClaimAI can call server-to-server.

**File:** `Enrollment/Controllers/MedicalScrutinyController.cs`

## New action

```csharp
// GET /MedicalScrutiny/GetBSIForAI?ClaimID=<id>
// Anonymous variant of GetBSI for the ClaimAI engine. Resolves MemberPolicyID /
// SITypeID / SlNo from the claim, then returns Main.GetBSI(...) exactly as the UI
// does — so ClaimAI shows the same Reserved/Blocked/Utilized as Spectra.
[HttpGet]
[AllowAnonymous]
[OverrideAuthorization]
public string GetBSIForAI(long ClaimID)
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
        long memberPolicyId = 0;
        int  siTypeId = 6;   // 6 = self, 5 = family floater
        byte slNo = 1;

        using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
        {
            conn.Open();

            // MemberPolicyID + latest SlNo for this claim
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT TOP 1 CAST(c.MemberPolicyID AS BIGINT) AS MemberPolicyID,
                             ISNULL(cd.Slno, 1) AS Slno
                FROM Claims c WITH (NOLOCK)
                LEFT JOIN Claimsdetails cd WITH (NOLOCK)
                       ON cd.ClaimID = c.ID AND ISNULL(cd.Deleted,0)=0
                WHERE c.ID = @cid AND ISNULL(c.Deleted,0)=0
                ORDER BY cd.Slno";
            cmd.Parameters.AddWithValue("@cid", ClaimID);
            using (var rdr = cmd.ExecuteReader())
                if (rdr.Read())
                {
                    if (rdr["MemberPolicyID"] != DBNull.Value) memberPolicyId = Convert.ToInt64(rdr["MemberPolicyID"]);
                    if (rdr["Slno"] != DBNull.Value)          slNo          = Convert.ToByte(rdr["Slno"]);
                }

            // SITypeID from the base SI (same resolution ClaimAI's fallback uses)
            if (memberPolicyId > 0)
            {
                var cmd2 = conn.CreateCommand();
                cmd2.CommandText = @"
                    SELECT TOP 1 ISNULL(bps.SITypeID, 6) AS SITypeID
                    FROM MemberSI ms WITH (NOLOCK)
                    JOIN BPSumInsured bps WITH (NOLOCK)
                      ON bps.ID = ms.BPSIID AND ISNULL(bps.Deleted,0)=0
                    WHERE ms.MemberPolicyID = @mp AND ISNULL(ms.Deleted,0)=0
                    ORDER BY CASE WHEN bps.SICategoryID_P20 = 69 THEN 0 ELSE 1 END, ms.ID DESC";
                cmd2.Parameters.AddWithValue("@mp", memberPolicyId);
                var v = cmd2.ExecuteScalar();
                if (v != null && v != DBNull.Value) siTypeId = Convert.ToInt32(v);
            }
        }

        if (memberPolicyId == 0)
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { error = "MemberPolicyID not found" });

        BSIinfo objBSI = new Main().GetBSI(memberPolicyId, siTypeId, ClaimID, slNo);
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
- **SITypeID:** I resolve it from the base SI (`bps.SITypeID`), the same way ClaimAI's fallback does. If the UI determines `hdnSITypeID` differently (e.g. always 6 for self, 5 for floater by a policy flag), match that here so a family-floater claim resolves the same as the Spectra screen.
- Endpoint is anonymous but guarded by the same `StagingApiKey` header as the staging scheduler; keep them consistent.
- Spectra change → rebuild/redeploy Spectra. ClaimAI change (`db.ts`) → redeploy ClaimAI. No reprocessing — `/api/bsi` runs live per claim.
