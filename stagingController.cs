using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DAL;
using Enrollment.Models;
using Enrollment.ViewModel;
using EnrollmentBAL;
using EnrollmentDAL.Utilities;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Resources;
using SpectraUtils;

namespace Enrollment.Controllers
{
    /// <summary>
    /// ClaimAI staging.
    ///
    /// Split out of MedicalScrutinyController because it shares nothing with it -
    /// no helper methods, no view-model fields. It is a batch job that happens to
    /// be reachable over HTTP, not part of the claim screen.
    ///
    /// TRIGGERED BY Spectra.StagingWorker.exe (Windows Task Scheduler), which GETs
    /// ProcessStagingClaims. The trigger used to be a System.Threading.Timer in
    /// Global.asax.cs, which tied staging to the IIS app pool - a recycle killed it
    /// and claims silently stopped being staged.
    ///
    /// ROUTE CHANGE: this is now /Staging/ProcessStagingClaims, NOT
    /// /MedicalScrutiny/ProcessStagingClaims. The worker's App.config must be
    /// updated to match, or every run returns 404 (exit code 1).
    ///
    /// All SQL goes through stored procedures - see
    /// sql/ALL_ClaimAI_Staging_Procs.sql. There is no inline SQL in this file.
    ///
    /// AUTO-APPROVAL SAVE IS DELIBERATELY ABSENT. It called Save*ForClaimAI
    /// endpoints that no longer exist - USP_ClaimAI_SaveClaimBundle replaced them -
    /// and reinstating a second writer is exactly what the migration removed. The
    /// auto-approval DECISION is still recorded, and BillingStateJson is still
    /// stored so the Approvals box is pre-checked when the doctor opens the claim.
    /// </summary>
    public class StagingController : Controller
    {

        /*  Class-level state the staging code depends on.

            These are FIELDS, not methods - which is why they were missed when the
            staging block was assembled by following method calls. */

        /// <summary>
        /// PSU insurer IDs: 5=UIIC, 6=NIC, 7=OIC, 8=NIAC
        /// </summary>
        private static readonly System.Collections.Generic.HashSet<int> PsuInsurerIds =
            new System.Collections.Generic.HashSet<int> { 5, 6, 7, 8 };

        /// <summary>
        /// Last tariff-selection decision, surfaced in the Spectra browser console
        /// when AI Summary is opened. Cross-check only.
        /// </summary>
        private string _lastTariffSelectionLog = "";

        /// <summary>
        /// Set true by ProcessStagingClaims while it calls GetMedicalBillDocument /
        /// GetTariffDocument internally, so those methods skip the user-session gate
        /// (the worker has no session). Always reset to false in a finally block.
        /// Browser-initiated calls never set it.
        /// </summary>
        private bool _stagingInternalCall = false;


        /*  Class-level state the staging code depends on.
            My first pass walked only METHOD calls, so these were missed - which is
            why the build reported "_stagingInternalCall does not exist". */


        /*  Batch staging for ClaimAI - 30 methods, the full transitive closure of
         *  ProcessStagingClaims.
         *
         *  TRIGGERED BY Spectra.StagingWorker.exe (Windows Task Scheduler), which
         *  GETs ProcessStagingClaims. It used to be a System.Threading.Timer in
         *  Global.asax.cs, which tied staging to the IIS app pool - a recycle
         *  killed it and claims silently stopped being staged.
         *
         *  AUTO-APPROVAL SAVE IS DELIBERATELY EXCLUDED, along with everything only
         *  it called (AutoPerformFullSaveForStaging, Save*ForClaimAI,
         *  SaveBillingForClaimAICore, RecordAutoSaveResult and their helpers).
         *  Those endpoints do not exist on this branch - USP_ClaimAI_SaveClaimBundle
         *  replaced them - and reinstating them would put a second writer back.
         *
         *  The auto-approval DECISION is still recorded (autoApproved/autoReason),
         *  and BillingStateJson is still stored so the Approvals box is pre-checked
         *  when the doctor opens the claim. Only the automatic money action is gone.
         */

        private byte[] BuildMultipartBody(
            string boundary,
            string claimId,
            string medFileName, byte[] medBytes,
            string tarFileName, byte[] tarBytes,
            string spectraFieldsJson,
            string socTarFileName = null, byte[] socTarBytes = null)
        {
            var enc = System.Text.Encoding.UTF8;
            using (var ms = new System.IO.MemoryStream())
            {
                WriteMultipartLine(ms, enc, "--" + boundary);
                WriteMultipartLine(ms, enc, "Content-Disposition: form-data; name=\"claimId\"");
                WriteMultipartLine(ms, enc, "");
                WriteMultipartLine(ms, enc, claimId);

                WriteMultipartLine(ms, enc, "--" + boundary);
                WriteMultipartLine(ms, enc, "Content-Disposition: form-data; name=\"medicalBill\"; filename=\"" + medFileName + "\"");
                WriteMultipartLine(ms, enc, "Content-Type: application/pdf");
                WriteMultipartLine(ms, enc, "");
                WriteMultipartBytes(ms, enc, medBytes);

                if (tarBytes != null && !string.IsNullOrWhiteSpace(tarFileName))
                {
                    WriteMultipartLine(ms, enc, "--" + boundary);
                    WriteMultipartLine(ms, enc, "Content-Disposition: form-data; name=\"tariffBill\"; filename=\"" + tarFileName + "\"");
                    WriteMultipartLine(ms, enc, "Content-Type: application/pdf");
                    WriteMultipartLine(ms, enc, "");
                    WriteMultipartBytes(ms, enc, tarBytes);
                }

                // GIPSA PPN->SOC two-pass: the SOC (non-PPN) fallback tariff file. Sent as a
                // SECOND tariff part ONLY when the trigger conditions held (PSU + GIPSA zip
                // containing both PPN and non-PPN/SOC files, non-monofocal lens). ClaimAI uses
                // it as the fallback when the PPN file has no exact procedure+lens combination.
                if (socTarBytes != null && !string.IsNullOrWhiteSpace(socTarFileName))
                {
                    WriteMultipartLine(ms, enc, "--" + boundary);
                    WriteMultipartLine(ms, enc, "Content-Disposition: form-data; name=\"socTariffBill\"; filename=\"" + socTarFileName + "\"");
                    WriteMultipartLine(ms, enc, "Content-Type: application/pdf");
                    WriteMultipartLine(ms, enc, "");
                    WriteMultipartBytes(ms, enc, socTarBytes);
                }

                if (!string.IsNullOrWhiteSpace(spectraFieldsJson))
                {
                    WriteMultipartLine(ms, enc, "--" + boundary);
                    WriteMultipartLine(ms, enc, "Content-Disposition: form-data; name=\"spectraFields\"");
                    WriteMultipartLine(ms, enc, "");
                    WriteMultipartLine(ms, enc, spectraFieldsJson);
                }

                var closing = enc.GetBytes("--" + boundary + "--\r\n");
                ms.Write(closing, 0, closing.Length);
                return ms.ToArray();
            }
        }


        private void WriteMultipartLine(System.IO.MemoryStream ms, System.Text.Encoding enc, string text)
        {
            var b = enc.GetBytes(text + "\r\n");
            ms.Write(b, 0, b.Length);
        }


        private void WriteMultipartBytes(System.IO.MemoryStream ms, System.Text.Encoding enc, byte[] data)
        {
            ms.Write(data, 0, data.Length);
            var crlf = enc.GetBytes("\r\n");
            ms.Write(crlf, 0, crlf.Length);
        }

        private string GetClaimDiseaseTypeForStaging(long claimId, string connStr)
        {
            // Specialties (TPAProcedures.Level1) that map to a supported disease.
            // Extend these sets to add diseases later.
            var cataractSpecialties = new System.Collections.Generic.HashSet<string>(
                StringComparer.OrdinalIgnoreCase) { "Ophtholmology" };
            var maternitySpecialties = new System.Collections.Generic.HashSet<string>(
                StringComparer.OrdinalIgnoreCase) { "Obstetrics and Gynecology", "OBG" };

            try
            {
                if (claimId <= 0 || string.IsNullOrWhiteSpace(connStr))
                    return "other";

                // Slno, procedure names and the fallback diagnosis text all come from
                // one round trip. The Level1/Level3 lookup needs level3Id, which comes
                // from USP_ClaimMedicalScrutiny_Retrieve below, so the inputs are read
                // in two passes: this one for Slno, and one after that call.
                int slNoInt = 1;
                string level1 = "", level3 = "", diagnosisTextForFallback = "";
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "USP_ClaimAI_GetDiseaseInputs";
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@ClaimID", claimId);
                    cmd.Parameters.Add("@Level3Id", System.Data.SqlDbType.Int).Value = DBNull.Value;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            if (rdr["SlNo"] != DBNull.Value) slNoInt = Convert.ToInt32(rdr["SlNo"]);
                            if (rdr["DiagnosisText"] != DBNull.Value)
                                diagnosisTextForFallback = rdr["DiagnosisText"].ToString().Trim();
                        }
                    }
                }

                // 2. level3_ID from the medical scrutiny row.
                int level3Id = 0;
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    cmd.CommandText = "USP_ClaimMedicalScrutiny_Retrieve";
                    cmd.Parameters.AddWithValue("@ClaimID", claimId);
                    cmd.Parameters.AddWithValue("@Slno", slNoInt);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            int ord = -1;
                            try { ord = rdr.GetOrdinal("level3_ID"); } catch { ord = -1; }
                            if (ord >= 0 && !rdr.IsDBNull(ord))
                                level3Id = Convert.ToInt32(rdr[ord]);
                        }
                    }
                }

                // The diagnosis text is already in hand from the call above, so the
                // fallback does not need its own connection. This is the path staged
                // claims almost always take, since a claim at StageID 52 has no coding
                // yet and level3Id is therefore 0.
                if (level3Id <= 0)
                    return ClassifyDiseaseByDiagnosisText(diagnosisTextForFallback, claimId);

                // 3. Level1 (specialty) + Level3 (procedure name) for that procedure.
                //    Same procedure as above, now with the resolved level3Id.
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "USP_ClaimAI_GetDiseaseInputs";
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@ClaimID", claimId);
                    cmd.Parameters.AddWithValue("@Level3Id", level3Id);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            level1 = rdr["Level1"] == DBNull.Value ? "" : rdr["Level1"].ToString().Trim();
                            level3 = rdr["Level3"] == DBNull.Value ? "" : rdr["Level3"].ToString().Trim();
                        }
                    }
                }
                string l3 = level3.ToLowerInvariant();
                if (l3.Contains("knee") && (l3.Contains("replacement") || l3.Contains("dislocation")))
                    return "tkr";

                if (string.IsNullOrWhiteSpace(level1)) return ClassifyDiseaseByDiagnosisTextForStaging(claimId, connStr);

                if (cataractSpecialties.Contains(level1)) return "cataract";
                if (maternitySpecialties.Contains(level1)) return "maternity";
                return ClassifyDiseaseByDiagnosisTextForStaging(claimId, connStr);
            }
            catch (Exception ex)
            {
                TariffLog("[Staging] GetClaimDiseaseTypeForStaging failed for claimId=" + claimId + ": " + ex.Message);
                return "other"; // safe default — claim goes straight to stage 5
            }
        }

        private bool IsReimbursementClaimForStaging(long claimId, string connStr)
        {
            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "USP_ClaimAI_GetClaimTypeInfo", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;
                        cmd.Parameters.AddWithValue("@ClaimID", claimId);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                int claimTypeId = rdr["ClaimTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ClaimTypeID"]);
                                int requestTypeId = rdr["RequestTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["RequestTypeID"]);
                                // Cashless = ClaimTypeID 1 AND RequestTypeID 1; 2 / 4 = reimbursement.
                                return claimTypeId == 2 || requestTypeId == 4;
                            }
                        }
                    }
                }
            }
            catch { /* default false */ }
            return false;
        }

        private string ClassifyDiseaseByDiagnosisTextForStaging(long claimId, string connStr)
        {
            try
            {
                if (claimId <= 0 || string.IsNullOrWhiteSpace(connStr))
                    return "other";

                // 1. Latest recorded diagnosis text for the claim.
                string diagnosisText = "";
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    using (var diagCmd = new System.Data.SqlClient.SqlCommand(
                            "USP_ClaimAI_GetClaimDiagnosisText", conn))
                    {
                        diagCmd.CommandType = System.Data.CommandType.StoredProcedure;
                        diagCmd.Parameters.AddWithValue("@ClaimID", claimId);
                        var diagVal = diagCmd.ExecuteScalar();
                        diagnosisText = diagVal != null && diagVal != DBNull.Value ? diagVal.ToString().Trim() : "";
                    }
                }

                return ClassifyDiseaseByDiagnosisText(diagnosisText, claimId);
            }
            catch (Exception ex)
            {
                TariffLog("[Staging] ClassifyDiseaseByDiagnosisTextForStaging failed for claimId=" + claimId + ": " + ex.Message);
                return "other";
            }
        }

        private string ClassifyDiseaseByDiagnosisText(string diagnosisText, long claimId)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(diagnosisText))
                    return "other";

                // 2. Classify the diagnosis text via ClaimAI (mirror GetClaimType:
                //    same endpoint, TLS 1.2 + cert-bypass settings).
                string claimAiUrl = (System.Configuration.ConfigurationManager.AppSettings["ClaimAIUrl"] ?? "").TrimEnd('/');
                if (string.IsNullOrEmpty(claimAiUrl))
                    return "other";

                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12 |
                    System.Net.SecurityProtocolType.Tls11 |
                    System.Net.SecurityProtocolType.Tls;
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    (sender, cert, chain, errors) => true;

                string classified = "other";
                using (var http = new System.Net.Http.HttpClient())
                {
                    http.Timeout = TimeSpan.FromSeconds(15);
                    var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { diagnosis = diagnosisText });
                    var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                    var res = http.PostAsync(claimAiUrl + "/api/classify-claim-type", content).GetAwaiter().GetResult();
                    if (res.IsSuccessStatusCode)
                    {
                        string respBody = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(respBody);
                        classified = (result?.claimType?.ToString() ?? "other").Trim().ToLowerInvariant();
                    }
                }

                // 3. Accept only known disease types; anything else stays "other".
                if (classified == "cataract" || classified == "maternity" || classified == "tkr")
                    return classified;
                return "other";
            }
            catch (Exception ex)
            {
                TariffLog("[Staging] ClassifyDiseaseByDiagnosisTextForStaging failed for claimId=" + claimId + ": " + ex.Message);
                return "other";
            }
        }

        private void GetClaimInsurerInfo(long claimId, string connStr,
            out int insurerIdOut, out string insurerCodeOut)
        {
            insurerIdOut = 0;
            insurerCodeOut = "";
            try
            {
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "USP_ClaimAI_GetClaimInsurerInfo";
                    cmd.Parameters.AddWithValue("@ClaimID", claimId);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            insurerIdOut = rdr["ID"] != DBNull.Value ? Convert.ToInt32(rdr["ID"]) : 0;
                            insurerCodeOut = (rdr["Code"] != DBNull.Value ? rdr["Code"].ToString().Trim() : "")
                                + " " +
                                (rdr["ShortName"] != DBNull.Value ? rdr["ShortName"].ToString().Trim() : "");
                        }
                    }
                }
            }
            catch { /* return defaults */ }
        }

        private void CollectTariffFilesFromZip(
            System.IO.Stream zipStream,
            string prefix,
            System.Collections.Generic.List<System.Tuple<string, DateTime, byte[]>> sink)
        {
            using (var zip = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Read, true))
            {
                foreach (var entry in zip.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name)) continue; // skip folder entries
                    var nameLow = entry.Name.ToLower();

                    if (nameLow.EndsWith(".zip"))
                    {
                        // Nested zip: extend the prefix with this zip's name
                        // (without ".zip") and recurse.
                        string innerBaseName = entry.Name.Substring(0, entry.Name.Length - 4); // strip ".zip"
                        string childPrefix = string.IsNullOrEmpty(prefix)
                            ? innerBaseName
                            : prefix + "_" + innerBaseName;
                        try
                        {
                            using (var entryStream = entry.Open())
                            using (var ms = new System.IO.MemoryStream())
                            {
                                entryStream.CopyTo(ms);
                                ms.Position = 0;
                                CollectTariffFilesFromZip(ms, childPrefix, sink);
                            }
                        }
                        catch (Exception zEx)
                        {
                            TariffLog("[Tariff] Failed to open nested zip " + entry.Name + ": " + zEx.Message);
                        }
                    }
                    else if (nameLow.EndsWith(".pdf") ||
                             nameLow.EndsWith(".xlsx") ||
                             nameLow.EndsWith(".xls"))
                    {
                        string displayName = string.IsNullOrEmpty(prefix)
                            ? entry.Name
                            : prefix + "_" + entry.Name;
                        using (var stream = entry.Open())
                        using (var ms = new System.IO.MemoryStream())
                        {
                            stream.CopyTo(ms);
                            sink.Add(System.Tuple.Create(displayName, entry.LastWriteTime.DateTime, ms.ToArray()));
                        }
                    }
                }
            }
        }

        private System.Tuple<string, byte[]> PickBestTariffFile(
            System.Collections.Generic.List<System.Tuple<string, DateTime, byte[]>> candidates,
            bool isPsu, string insurerCode)
        {
            if (candidates == null || candidates.Count == 0) return null;

            var code = (insurerCode ?? "").ToLower();

            // Build priority tiers
            var tiers = new System.Collections.Generic.List<
                System.Collections.Generic.List<System.Tuple<string, DateTime, byte[]>>>();

            if (isPsu)
            {
                // P1: insurer-specific
                tiers.Add(candidates.FindAll(f => !string.IsNullOrEmpty(code) &&
                    code.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Any(c2 => c2.Length > 1 && f.Item1.ToLower().Contains(c2))));
                // P2: GIPSA (not SOC)
                tiers.Add(candidates.FindAll(f => f.Item1.ToLower().Contains("gipsa") &&
                    !f.Item1.ToLower().Contains("soc")));
                // P3: GIPSA SOC
                tiers.Add(candidates.FindAll(f => f.Item1.ToLower().Contains("gipsa") &&
                    f.Item1.ToLower().Contains("soc")));
                // P4: All Insurers / Pvt / Private
                tiers.Add(candidates.FindAll(f => f.Item1.ToLower().Contains("all insurer") ||
                    f.Item1.ToLower().Contains("pvt insurer") ||
                    f.Item1.ToLower().Contains("private insurer")));
            }
            else
            {
                // P1: insurer-specific
                tiers.Add(candidates.FindAll(f => !string.IsNullOrEmpty(code) &&
                    code.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Any(c2 => c2.Length > 1 && f.Item1.ToLower().Contains(c2))));
                // P2: All Insurers / Pvt / Private
                tiers.Add(candidates.FindAll(f => f.Item1.ToLower().Contains("all insurer") ||
                    f.Item1.ToLower().Contains("pvt insurer") ||
                    f.Item1.ToLower().Contains("private insurer")));
            }

            // P_n: FHPL Rate List (fallback for all insurer types)
            tiers.Add(candidates.FindAll(f =>
            {
                var n = f.Item1.ToLower();
                return n.Contains("fhpl") || n.Contains("rate list") || n.Contains("ratelist");
            }));

            // P_n+1: filename contains "tariff"
            tiers.Add(candidates.FindAll(f => f.Item1.ToLower().Contains("tariff")));

            // P_last: any file
            tiers.Add(new System.Collections.Generic.List<System.Tuple<string, DateTime, byte[]>>(candidates));

            // Pick from first non-empty tier → latest modified → skip unreadable files
            foreach (var tier in tiers)
            {
                if (tier == null || tier.Count == 0) continue;
                tier.Sort((a, b) => b.Item2.CompareTo(a.Item2)); // latest first
                foreach (var candidate in tier)
                {
                    byte[] converted = EnsurePdf(candidate.Item1, candidate.Item3);
                    if (converted != null) return System.Tuple.Create(candidate.Item1, converted);
                }
            }

            return null;
        }

        private bool GipsaZipHasBothPpnAndSoc(
            System.Collections.Generic.List<System.Tuple<string, DateTime, byte[]>> candidates)
        {
            if (candidates == null || candidates.Count == 0) return false;
            // The fallback (non-PPN) file may be named "soc" OR "non-ppn"/"nonppn".
            // The PPN file is a GIPSA file that is NEITHER.
            System.Func<string, bool> isSoc = n =>
            {
                var x = (n ?? "").ToLower();
                return x.Contains("soc") || x.Contains("non-ppn") || x.Contains("nonppn") || x.Contains("non ppn");
            };
            bool hasPpn = candidates.Exists(f => f.Item1 != null &&
                f.Item1.ToLower().Contains("gipsa") && !isSoc(f.Item1));
            bool hasSoc = candidates.Exists(f => f.Item1 != null &&
                f.Item1.ToLower().Contains("gipsa") && isSoc(f.Item1));
            return hasPpn && hasSoc;
        }

        private System.Tuple<string, byte[]> PickBestSocTariffFile(
            System.Collections.Generic.List<System.Tuple<string, DateTime, byte[]>> candidates)
        {
            if (candidates == null || candidates.Count == 0) return null;
            // The fallback (non-PPN) file may be named "soc" OR "non-ppn"/"nonppn".
            System.Func<string, bool> isSoc = n =>
            {
                var x = (n ?? "").ToLower();
                return x.Contains("soc") || x.Contains("non-ppn") || x.Contains("nonppn") || x.Contains("non ppn");
            };
            var soc = candidates.FindAll(f => f.Item1 != null &&
                f.Item1.ToLower().Contains("gipsa") && isSoc(f.Item1));
            if (soc.Count == 0) return null;
            soc.Sort((a, b) => b.Item2.CompareTo(a.Item2)); // latest first
            foreach (var candidate in soc)
            {
                byte[] converted = EnsurePdf(candidate.Item1, candidate.Item3);
                if (converted != null) return System.Tuple.Create(candidate.Item1, converted);
            }
            return null;
        }
        public ActionResult ProcessStagingClaims(int batchSize = 10)
        {
            int processed = 0, skipped = 0, failed = 0;
            var details = new System.Collections.Generic.List<object>();

            try
            {
                // Optional auth: if StagingApiKey is configured, require the header.
                string stagingKey = System.Configuration.ConfigurationManager.AppSettings["StagingApiKey"] ?? "";
                if (!string.IsNullOrEmpty(stagingKey))
                {
                    string reqKey = Request.Headers["x-staging-key"] ?? Request.QueryString["key"] ?? "";
                    if (reqKey != stagingKey)
                        return Json(new { Success = false, Message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
                }

                string connStr = GetStagingConnString();
                if (string.IsNullOrWhiteSpace(connStr))
                    return Json(new { Success = false, Message = "No connection string." }, JsonRequestBehavior.AllowGet);

                if (batchSize <= 0 || batchSize > 50) batchSize = 10;

                // ── Find claims to process: StageID 52, no ClaimAI_Results row yet
                //    (or status NULL). 'processing'/'done'/'failed'/'skipped' are
                //    excluded so we never double-pick. ───────────────────────────

                // Batch + Slno in ONE round trip. The Slno probe used to run inside the
                // loop below, opening a fresh connection per claim, so a batch of 10 cost
                // 11 connections before any claim was touched.
                // The procedure also de-duplicates: ClaimAI_Results is keyed on
                // ClaimID+SlNo, so the old LEFT JOIN could emit one claim several times
                // and the worker would process it more than once in a single run.
                var claimBatch = new System.Collections.Generic.List<System.Tuple<long, int>>();
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "USP_ClaimAI_GetStagingBatch";
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@BatchSize", batchSize);
                    // Tunable without a redeploy. Defaults match the procedure's own.
                    cmd.Parameters.AddWithValue("@MinAgeMinutes",
                        Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ClaimAIMinClaimAgeMinutes"] ?? "15"));
                    cmd.Parameters.AddWithValue("@RetryAfterMinutes",
                        Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ClaimAIDocRetryMinutes"] ?? "20"));
                    cmd.Parameters.AddWithValue("@MaxDocAttempts",
                        Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ClaimAIMaxDocAttempts"] ?? "6"));
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            claimBatch.Add(System.Tuple.Create(
                                Convert.ToInt64(rdr["ClaimID"]),
                                rdr["SlNo"] != DBNull.Value ? Convert.ToInt32(rdr["SlNo"]) : 1));
                        }
                    }
                }
                TariffLog("[Staging] Batch picked " + claimBatch.Count + " claim(s) in one round trip.");

                foreach (var claimEntry in claimBatch)
                {
                    // Declared BEFORE the try, because the catch below references claimId.
                    // The original `foreach (long claimId in claimIds)` made it the loop
                    // variable, so it was visible to both blocks - putting it inside the
                    // try gives: error CS0103: The name 'claimId' does not exist in the
                    // current context.
                    long claimId = claimEntry.Item1;
                    int slNo = claimEntry.Item2;
                    try
                    {


                        // 1. Classify disease.
                        string disease = GetClaimDiseaseTypeForStaging(claimId, connStr);

                        // 3. Lock: mark 'processing' so a concurrent run won't re-pick.
                        UpsertStagingResult(connStr, claimId, slNo, disease, "processing",
                            null, null, null, null, null);

                        // 4. Fetch docs internally (no session) — reuse existing
                        //    env-based GetMedicalBillDocument / GetTariffDocument.
                        string billB64 = null, billName = null, tarB64 = null, tarName = null, billPageCatsJson = null, tarPageCatsJson = null;
                        string socB64 = null, socName = null, socPageCatsJson = null;
                        _stagingInternalCall = true;
                        try
                        {
                            ExtractDocBase64(GetMedicalBillDocument(claimId.ToString(), slNo.ToString()),
                                out billB64, out billName, out billPageCatsJson);
                            try
                            {
                                ExtractDocBase64(GetTariffDocument(claimId.ToString(), slNo.ToString()),
                                    out tarB64, out tarName, out tarPageCatsJson);
                            }
                            catch { /* tariff optional */ }
                            // GIPSA PPN->SOC two-pass: fetch the SOC fallback file. Returns empty
                            // unless PSU + GIPSA zip has both PPN and SOC (guarded inside the endpoint).
                            try
                            {
                                ExtractDocBase64(GetTariffDocument(claimId.ToString(), slNo.ToString(), true),
                                    out socB64, out socName, out socPageCatsJson);
                            }
                            catch { /* SOC optional */ }
                        }
                        finally
                        {
                            _stagingInternalCall = false;
                        }

                        if (string.IsNullOrEmpty(billB64))
                            throw new Exception("Medical bill not found for claimId=" + claimId);

                        // 5. Submit to ClaimAI (synchronous: returns jobId once done).
                        // Reimbursement flag (ClaimTypeID=2 / RequestTypeID=4) — mirrors
                        // IsClaimAISummaryAllowed; drives ClaimAI's Missing Documents section.
                        bool isReimbursement = IsReimbursementClaimForStaging(claimId, connStr);
                        string jobId = SubmitClaimToClaimAI(claimId.ToString(), billName, billB64,
                            tarName, tarB64, disease, isReimbursement, billPageCatsJson,
                            socName, socB64);
                        if (string.IsNullOrWhiteSpace(jobId))
                            throw new Exception("ClaimAI did not return a jobId.");

                        // 6. Pull the stored result.
                        string analysisJson, benefitPlan, pdfUrl, aiStatus;
                        GetStagingResultFromClaimAI(jobId, out aiStatus, out analysisJson,
                            out benefitPlan, out pdfUrl);

                        if (aiStatus == "error")
                            throw new Exception("ClaimAI reported processing error for jobId=" + jobId);

                        // 7. Fetch processed PDF bytes from the URL (best-effort).
                        byte[] pdfBytes = null;
                        if (!string.IsNullOrWhiteSpace(pdfUrl))
                        {
                            try { pdfBytes = DownloadBytes(pdfUrl); }
                            catch (Exception pdfEx)
                            { System.Diagnostics.Debug.WriteLine("[Staging] PDF download failed: " + pdfEx.Message); }
                        }

                        // 8. Auto-approval decision (Phase 1: RECORD ONLY — no money action).
                        //    ClaimAI returns analysis.autoApproval = { eligible, minAmount,
                        //    approvalBasis, approvedAmount, actionType, threshold, reason }.
                        //    We record whether the claim WOULD auto-approve; execution of the
                        //    save/approve is deferred to Phase 2+ (see Auto-Approval-Analysis.md).
                        bool? autoApproved = null;
                        string autoReason = null;
                        string autoBillingStateJson = null;
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(analysisJson))
                            {
                                dynamic _an = Newtonsoft.Json.JsonConvert.DeserializeObject(analysisJson);
                                var _aa = _an?.autoApproval;
                                if (_aa != null)
                                {
                                    autoApproved = (bool?)_aa.eligible ?? false;
                                    autoReason = (string)_aa.reason;
                                    // ClaimAI includes a ready-to-store billingState whenever ANY
                                    // approvals value is present (bill/tariff/sub-limit) — NOT only when
                                    // auto-approved. Persisting it to BillingStateJson makes the correct
                                    // Approvals checkbox ALREADY checked when the doctor opens the claim,
                                    // which is what enables Save. This is INDEPENDENT of autoApproved
                                    // (that only governs the post-save money action).
                                    if (_aa.billingState != null)
                                    {
                                        try { autoBillingStateJson = Newtonsoft.Json.JsonConvert.SerializeObject(_aa.billingState); }
                                        catch { autoBillingStateJson = null; }
                                    }
                                    // Hospital estimate + tariff total for the save payload
                                    // (baseInsurerPayable is the after-discount hospital figure).
                                    try
                                    {
                                        if (_an.tariffExtractionItem != null)
                                        {
                                            decimal _tsum = 0m;
                                            foreach (var _ti in _an.tariffExtractionItem)
                                            {
                                                try { _tsum += Convert.ToDecimal(_ti.amount); } catch { }
                                            }
                                        }
                                    }
                                    catch { }
                                    TariffLog("[AutoApproval] claim=" + claimId
                                        + " eligible=" + autoApproved
                                        + " min=" + (string)(_aa.minAmount != null ? _aa.minAmount.ToString() : "null")
                                        + " basis=" + (string)(_aa.approvalBasis ?? "null")
                                        + " action=" + (string)(_aa.actionType ?? "null")
                                        + " reason=" + (autoReason ?? ""));
                                }
                            }
                        }
                        catch (Exception aaEx)
                        {
                            TariffLog("[AutoApproval] decision parse failed for claim=" + claimId + ": " + aaEx.Message);
                        }

                        // 8b. Store results + flip to stage 5. AutoApproved records the
                        //     decision; the claim still goes to adjudication for a doctor
                        //     (Phase 1). Phase 2+ will execute the save/approve when eligible.
                        UpsertStagingResult(connStr, claimId, slNo, disease, "done",
                            jobId, analysisJson, benefitPlan, pdfBytes, null, autoApproved, autoBillingStateJson);
                        SetClaimStage(connStr, claimId, 5);

                        /* PHASE 2a auto-save REMOVED.

                           It called SaveHospitalizationDetailsForClaimAI,
                           SaveClinicalDetailsForClaimAI and SaveCodingRowForClaimAI -
                           endpoints this branch no longer has. Saving now goes through
                           USP_ClaimAI_SaveClaimBundle when the doctor clicks Save in the
                           iframe, and reinstating a second writer here is exactly what
                           the migration removed.

                           The auto-approval DECISION is still recorded above
                           (autoApproved / autoReason) and BillingStateJson is still
                           stored, so the Approvals box is pre-checked when the doctor
                           opens the claim. Only the automatic money action is gone. */

                        processed++;
                        details.Add(new { claimId, disease, status = "done", jobId, autoApproved, autoReason });
                    }
                    catch (Exception claimEx)
                    {
                        // Documents reach S3 slightly after the claim is created. A claim
                        // picked up inside that window has no medical bill YET - which is a
                        // timing miss, not a bad claim. Park it at stage 52 instead of
                        // failing it, and the batch query will offer it again after the
                        // retry window. Only once the attempts are exhausted is it treated
                        // as a real failure and sent to stage 5 for on-demand processing.
                        bool _docsMissing =
                            (claimEx.Message ?? "").IndexOf("No medical bill found",
                                StringComparison.OrdinalIgnoreCase) >= 0;

                        int _attempts = 0;
                        if (_docsMissing)
                        {
                            try
                            {
                                using (var _ac = new System.Data.SqlClient.SqlConnection(connStr))
                                {
                                    _ac.Open();
                                    var _cmd = _ac.CreateCommand();
                                    _cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                    _cmd.CommandText = "USP_ClaimAI_GetDocAttemptCount";
                                    _cmd.Parameters.AddWithValue("@ClaimID", claimId);
                                    var _v = _cmd.ExecuteScalar();
                                    if (_v != null && _v != DBNull.Value) _attempts = Convert.ToInt32(_v);
                                }
                            }
                            catch { /* treat an unreadable count as 0 - it will retry */ }
                        }

                        // Keep this in step with @MaxDocAttempts in USP_ClaimAI_GetStagingBatch.
                        int _maxDocAttempts = 6;
                        int.TryParse(System.Configuration.ConfigurationManager
                            .AppSettings["ClaimAIMaxDocAttempts"] ?? "6", out _maxDocAttempts);
                        if (_maxDocAttempts <= 0) _maxDocAttempts = 6;

                        bool _park = _docsMissing && (_attempts + 1) < _maxDocAttempts;

                        try
                        {
                            UpsertStagingResult(connStr, claimId, slNo: 1, diseaseType: null,
                                status: _park ? "awaiting_documents" : "failed",
                                jobId: null, analysisJson: null,
                                benefitPlan: null, pdfBytes: null, lastError: claimEx.Message);

                            // Only move to stage 5 when giving up. A parked claim MUST stay at
                            // 52, or the batch query will never see it again.
                            if (!_park) SetClaimStage(connStr, claimId, 5);
                        }
                        catch { /* swallow - never let one claim break the batch */ }

                        if (_park)
                        {
                            details.Add(new { claimId, status = "awaiting_documents", error = claimEx.Message });
                            TariffLog("[Staging] Claim " + claimId + " has no documents yet (attempt "
                                + (_attempts + 1) + " of " + _maxDocAttempts + ") - parked at stage 52 for retry.");
                        }
                        else
                        {
                            failed++;
                            details.Add(new { claimId, status = "failed", error = claimEx.Message });
                            TariffLog("[Staging] Claim " + claimId + " failed: " + claimEx.Message);
                        }
                    }
                }

                return Json(new
                {
                    Success = true,
                    Processed = processed,
                    Skipped = skipped,
                    Failed = failed,
                    Total = claimBatch.Count,
                    Details = details
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetStagingConnString()
        {
            string connStr = System.Configuration.ConfigurationManager
                .ConnectionStrings["McarePlusEntities"]?.ConnectionString ?? "";
            if (connStr.StartsWith("metadata=", StringComparison.OrdinalIgnoreCase))
            {
                var m = System.Text.RegularExpressions.Regex.Match(
                    connStr, "provider connection string=\\\"([^\\\"]+)\\\"",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (m.Success) connStr = m.Groups[1].Value.Replace("&quot;", "\"");
            }
            return connStr;
        }

        private void ExtractDocBase64(ActionResult result, out string base64, out string fileName, out string pageCategoriesJson)
        {
            base64 = null; fileName = null; pageCategoriesJson = null;
            if (result == null) return;
            try
            {
                // Case 1: ContentResult — a serialized JSON string.
                var cr = result as ContentResult;
                if (cr != null && !string.IsNullOrEmpty(cr.Content))
                {
                    dynamic parsed = Newtonsoft.Json.JsonConvert.DeserializeObject(cr.Content);
                    bool okC = (bool?)parsed?.Success ?? false;
                    if (!okC || parsed?.Data == null) return;
                    base64 = (string)parsed.Data.base64Content;
                    fileName = (string)parsed.Data.fileName;
                    try { if (parsed.Data.pageCategories != null) pageCategoriesJson = Newtonsoft.Json.JsonConvert.SerializeObject(parsed.Data.pageCategories); } catch { }
                    return;
                }

                // Case 2: JsonResult — read the typed object directly.
                var jr = result as JsonResult;
                if (jr != null && jr.Data != null)
                {
                    dynamic d = jr.Data;
                    bool ok = (bool?)d.Success ?? false;
                    if (!ok || d.Data == null) return;
                    base64 = (string)d.Data.base64Content;
                    fileName = (string)d.Data.fileName;
                    try { if (d.Data.pageCategories != null) pageCategoriesJson = Newtonsoft.Json.JsonConvert.SerializeObject(d.Data.pageCategories); } catch { }
                    return;
                }
            }
            catch { /* shape mismatch -> leave null */ }
        }

        private void UpsertStagingResult(
            string connStr, long claimId, int slNo, string diseaseType, string status,
            string jobId, string analysisJson, string benefitPlan, byte[] pdfBytes, string lastError,
            bool? autoApproved = null, string billingStateJson = null)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 300; // large ProcessedPdf blob can exceed the 30s default
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "USP_ClaimAI_UpsertStagingResult";

                cmd.Parameters.AddWithValue("@ClaimID", claimId);
                cmd.Parameters.AddWithValue("@SlNo", slNo);
                cmd.Parameters.AddWithValue("@DiseaseType", (object)diseaseType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@JobId", (object)jobId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AnalysisJson", (object)analysisJson ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BenefitPlan", (object)benefitPlan ?? DBNull.Value);
                var pdfParam = cmd.Parameters.Add("@ProcessedPdf", System.Data.SqlDbType.VarBinary, -1);
                pdfParam.Value = (object)pdfBytes ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@LastError",
                    (object)(lastError != null && lastError.Length > 2000 ? lastError.Substring(0, 2000) : lastError) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AutoApproved",
                    autoApproved.HasValue ? (object)autoApproved.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@BillingStateJson",
                    string.IsNullOrWhiteSpace(billingStateJson) ? (object)DBNull.Value : billingStateJson);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Moves a claim to a stage.
        ///
        /// Both statements - the Claims update AND the claimActionItems clone,
        /// which was ~56 lines of dynamic SQL in a C# string - now live in
        /// USP_ClaimAI_SetClaimStage. The procedure wraps them in its own
        /// transaction, so this no longer needs two commands on one connection.
        /// </summary>
        private void SetClaimStage(string connStr, long claimId, int stageId)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "USP_ClaimAI_SetClaimStage";
                cmd.CommandTimeout = 60;
                cmd.Parameters.AddWithValue("@ClaimID", claimId);
                cmd.Parameters.AddWithValue("@Stage", stageId);
                cmd.ExecuteNonQuery();
            }
        }

        private string SubmitClaimToClaimAI(
                string claimId, string billName, string billB64,
                string tarName, string tarB64, string disease, bool isReimbursement,
                string pageCategoriesJson = null,
                string socName = null, string socB64 = null)
        {
            byte[] medBytes = Convert.FromBase64String(billB64);
            byte[] tarBytes = null;
            if (!string.IsNullOrWhiteSpace(tarB64))
            {
                try { tarBytes = Convert.FromBase64String(tarB64); } catch { }
            }
            // GIPSA PPN->SOC two-pass: decode the SOC fallback file (may be absent).
            byte[] socBytes = null;
            if (!string.IsNullOrWhiteSpace(socB64))
            {
                try { socBytes = Convert.FromBase64String(socB64); } catch { }
            }

            // Minimal spectraFields — the browser builds these from the DOM, but the
            // worker has none. Pass claimType; ClaimAI fills the rest from its own DB.
            var _sf = new Newtonsoft.Json.Linq.JObject();
            _sf["claimType"] = disease;
            _sf["isReimbursement"] = isReimbursement;
            // Tariff file names for the GIPSA two-pass (ClaimAI tags the extraction with
            // whichever file the rows came from). Harmless when SOC is absent.
            if (!string.IsNullOrWhiteSpace(tarName)) _sf["tariffFileName"] = tarName;
            if (!string.IsNullOrWhiteSpace(socName)) _sf["socTariffFileName"] = socName;
            if (!string.IsNullOrWhiteSpace(pageCategoriesJson))
            {
                try { _sf["billPageCategories"] = Newtonsoft.Json.Linq.JArray.Parse(pageCategoriesJson); } catch { }
            }
            string spectraFieldsJson = _sf.ToString(Newtonsoft.Json.Formatting.None);

            string claimAiUrl = (System.Configuration.ConfigurationManager.AppSettings["ClaimAIUrl"]
                ?? "http://localhost:3000").TrimEnd('/') + "/api/audit/start";

            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 |
                System.Net.SecurityProtocolType.Tls11 |
                System.Net.SecurityProtocolType.Tls;
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, cert, chain, errors) => true;

            string boundary = "ClaimAIBoundary" + Guid.NewGuid().ToString("N");
            byte[] multipartBody = BuildMultipartBody(
                boundary, claimId, billName ?? "medical-bill.pdf", medBytes,
                tarName, tarBytes, spectraFieldsJson,
                socName, socBytes);

            using (var client = new System.Net.Http.HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(3);
                var req = new System.Net.Http.HttpRequestMessage(
                    System.Net.Http.HttpMethod.Post, claimAiUrl);
                var bodyContent = new System.Net.Http.ByteArrayContent(multipartBody);
                bodyContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
                    "multipart/form-data; boundary=" + boundary);
                req.Content = bodyContent;

                var resp = client.SendAsync(req).GetAwaiter().GetResult();
                string respBody = resp.Content.ReadAsStringAsync().Result;
                if (!resp.IsSuccessStatusCode)
                    throw new Exception("ClaimAI audit/start HTTP " + (int)resp.StatusCode + ": " + respBody);

                dynamic convexRes = Newtonsoft.Json.JsonConvert.DeserializeObject(respBody);
                return convexRes?.jobId?.ToString() ?? "";
            }
        }

        private void GetStagingResultFromClaimAI(
            string jobId, out string status, out string analysisJson,
            out string benefitPlan, out string processedPdfUrl)
        {
            status = null; analysisJson = null; benefitPlan = null; processedPdfUrl = null;

            string baseUrl = (System.Configuration.ConfigurationManager.AppSettings["ClaimAIUrl"]
                ?? "http://localhost:3000").TrimEnd('/');
            string url = baseUrl + "/api/staging/result?jobId=" + Uri.EscapeDataString(jobId);

            using (var client = new System.Net.Http.HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(2);
                var resp = client.GetAsync(url).GetAwaiter().GetResult();
                string respBody = resp.Content.ReadAsStringAsync().Result;
                if (!resp.IsSuccessStatusCode)
                    throw new Exception("staging/result HTTP " + (int)resp.StatusCode + ": " + respBody);

                dynamic r = Newtonsoft.Json.JsonConvert.DeserializeObject(respBody);
                status = r?.status?.ToString();
                // analysis is a JSON object — re-serialize it to store as text.
                if (r?.analysis != null)
                    analysisJson = Newtonsoft.Json.JsonConvert.SerializeObject(r.analysis);
                benefitPlan = r?.benefitPlan?.ToString();
                processedPdfUrl = r?.processedPdfUrl?.ToString();
            }
        }

        private byte[] DownloadBytes(string url)
        {
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 |
                System.Net.SecurityProtocolType.Tls11 |
                System.Net.SecurityProtocolType.Tls;
            using (var client = new System.Net.Http.HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(2);
                return client.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
        }

        private static void TariffLog(string message)
        {
            try
            {
                string logDir = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Logs");
                if (!System.IO.Directory.Exists(logDir))
                    System.IO.Directory.CreateDirectory(logDir);
                string logFile = System.IO.Path.Combine(logDir, "TariffSelection_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                string line = DateTime.Now.ToString("HH:mm:ss.fff") + " " + message + Environment.NewLine;
                System.IO.File.AppendAllText(logFile, line);
            }
            catch { /* never let logging break the main flow */ }
        }


        private System.Tuple<string, byte[]> PickTariffFileWithAI(
            System.Collections.Generic.List<System.Tuple<string, DateTime, byte[]>> candidates,
            bool isPsu, string insurerCode)
        {
            _lastTariffSelectionLog = "";
            if (candidates == null || candidates.Count == 0)
            {
                _lastTariffSelectionLog = "Sent 0 files (no tariff candidates). insurerCode=" + (insurerCode ?? "") + " isPsu=" + isPsu;
                return null;
            }
            if (candidates.Count == 1)
            {
                _lastTariffSelectionLog = "Sent 1 file: [" + candidates[0].Item1 + "] | insurerCode=" + (insurerCode ?? "") + " isPsu=" + isPsu + " -> selected: " + candidates[0].Item1 + " (only one file, no AI call)";
                byte[] converted = EnsurePdf(candidates[0].Item1, candidates[0].Item3);
                return converted != null ? System.Tuple.Create(candidates[0].Item1, converted) : null;
            }

            try
            {
                string claimAiUrl = System.Configuration.ConfigurationManager.AppSettings["claimAIUrl"] ?? "";
                // Strip any path — we only need the base (scheme + host + port)
                if (!string.IsNullOrWhiteSpace(claimAiUrl))
                {
                    try
                    {
                        var u = new System.Uri(claimAiUrl.TrimEnd('/'));
                        claimAiUrl = u.GetLeftPart(System.UriPartial.Authority);
                    }
                    catch { /* keep as-is if parsing fails */ }
                }
                TariffLog("[Tariff] AI CONFIG — ClaimAI base URL='" + claimAiUrl + "'");
                if (!string.IsNullOrWhiteSpace(claimAiUrl))
                {
                    string baseUrl = claimAiUrl; // already stripped to base URL above

                    var fileNames = candidates.ConvertAll(c => c.Item1);
                    _lastTariffSelectionLog = "Sent " + fileNames.Count + " files: [" + string.Join(" | ", fileNames) + "] | insurerCode=" + (insurerCode ?? "") + " isPsu=" + isPsu;
                    TariffLog("[Tariff] AI INPUT — Calling: " + baseUrl + "/api/tariff-file-selection");
                    TariffLog("[Tariff] AI INPUT — " + fileNames.Count + " files: " + string.Join(" | ", fileNames));
                    TariffLog("[Tariff] AI INPUT — InsurerCode=" + insurerCode + " IsPSU=" + isPsu);

                    var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        fileNames = fileNames,
                        insurerCode = insurerCode ?? "",
                        isPsu = isPsu
                    });

                    using (var http = new System.Net.Http.HttpClient())
                    {
                        http.Timeout = TimeSpan.FromSeconds(15);
                        var body = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                        var response = http.PostAsync(baseUrl + "/api/tariff-file-selection", body).GetAwaiter().GetResult();

                        if (response.IsSuccessStatusCode)
                        {
                            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                            string selectedFile = result?.selectedFile?.ToString();

                            _lastTariffSelectionLog += " -> AI selected: " + (selectedFile ?? "null") + " | tier=" + (result?.priorityTier?.ToString() ?? "") + " | reason=" + (result?.reason?.ToString() ?? "");
                            TariffLog("[Tariff] AI OUTPUT — Selected: " + selectedFile + " | Tier: " + result?.priorityTier + " | Reason: " + result?.reason);

                            if (!string.IsNullOrWhiteSpace(selectedFile))
                            {
                                var match = candidates.Find(c => c.Item1 == selectedFile);
                                if (match != null)
                                {
                                    byte[] converted = EnsurePdf(match.Item1, match.Item3);
                                    if (converted != null) return System.Tuple.Create(match.Item1, converted);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TariffLog("[Tariff] AI CALL FAILED — " + ex.GetType().Name + ": " + ex.Message);
                _lastTariffSelectionLog += " -> AI call FAILED: " + ex.GetType().Name + ": " + ex.Message;
            }

            // Fallback: use existing rule-based logic
            _lastTariffSelectionLog += " -> FALLBACK: rule-based selection used";
            TariffLog("[Tariff] FALLBACK — AI failed, using rule-based PickBestTariffFile");
            var fb = PickBestTariffFile(candidates, isPsu, insurerCode);
            _lastTariffSelectionLog += " -> fallback selected: " + (fb?.Item1 ?? "null");
            return fb;
        }

        private byte[] EnsurePdf(string fileName, byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0) return null;
            var nameLower = (fileName ?? "").ToLower();

            try
            {
                // Already a PDF — validate it. A valid PDF may not have "%PDF" at byte 0
                // (leading BOM / whitespace), so scan the first 1024 bytes for the marker
                // instead of only offset 0.
                if (nameLower.EndsWith(".pdf"))
                {
                    int scan = Math.Min(fileBytes.Length, 1024);
                    for (int i = 0; i + 3 < scan; i++)
                    {
                        if (fileBytes[i] == 0x25 && fileBytes[i + 1] == 0x50 &&
                            fileBytes[i + 2] == 0x44 && fileBytes[i + 3] == 0x46) // %PDF
                            return fileBytes;
                    }
                    // No %PDF marker in the header window — validate with iTextSharp.
                    // unethicalreading lets us read owner-password ("permissions") protected
                    // PDFs that open fine in a viewer but otherwise make PdfReader throw.
                    try
                    {
                        iTextSharp.text.pdf.PdfReader.unethicalreading = true;
                        var reader = new iTextSharp.text.pdf.PdfReader(fileBytes);
                        reader.Close();
                        return fileBytes;
                    }
                    catch (Exception pdfEx)
                    {
                        TariffLog("[Tariff] EnsurePdf REJECTED '" + fileName + "' — " + fileBytes.Length
                                  + " bytes, no %PDF marker in first " + scan + " bytes, PdfReader threw "
                                  + pdfEx.GetType().Name + ": " + pdfEx.Message);
                        return null;
                    }
                }

                // Excel file — convert to PDF
                if (nameLower.EndsWith(".xlsx") || nameLower.EndsWith(".xls"))
                {
                    return ConvertExcelToPdf(fileBytes, nameLower.EndsWith(".xlsx"));
                }

                // Unknown type — try as PDF anyway
                return fileBytes;
            }
            catch
            {
                return null; // unreadable — skip this file
            }
        }

        private byte[] ConvertExcelToPdf(byte[] excelBytes, bool isXlsx)
        {
            try
            {
                using (var excelStream = new System.IO.MemoryStream(excelBytes))
                using (var pdfStream = new System.IO.MemoryStream())
                {
                    var workbook = new ClosedXML.Excel.XLWorkbook(excelStream);

                    // Use A4 portrait — landscape causes text rotation issues in react-pdf
                    var pageSize = iTextSharp.text.PageSize.A4;
                    var document = new iTextSharp.text.Document(pageSize, 20f, 20f, 20f, 20f);
                    iTextSharp.text.pdf.PdfWriter.GetInstance(document, pdfStream);
                    document.Open();

                    // Fonts
                    var titleFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 13, iTextSharp.text.BaseColor.WHITE);
                    var headerFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA_BOLD, 9, iTextSharp.text.BaseColor.WHITE);
                    var cellFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 9, new iTextSharp.text.BaseColor(33, 33, 33));
                    var altCellFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, 9, new iTextSharp.text.BaseColor(33, 33, 33));

                    // Colors
                    var headerBg = new iTextSharp.text.BaseColor(30, 58, 95);   // dark navy
                    var altRowBg = new iTextSharp.text.BaseColor(240, 245, 255);  // light blue
                    var whiteBg = iTextSharp.text.BaseColor.WHITE;
                    var borderCol = new iTextSharp.text.BaseColor(200, 210, 230);

                    bool firstSheet = true;
                    foreach (var worksheet in workbook.Worksheets)
                    {
                        if (worksheet.IsEmpty()) continue;
                        if (!firstSheet) document.NewPage();
                        firstSheet = false;

                        var usedRange = worksheet.RangeUsed();
                        if (usedRange == null) continue;

                        int colCount = usedRange.ColumnCount();
                        int rowCount = usedRange.RowCount();

                        // Sheet title bar
                        var titleTable = new iTextSharp.text.pdf.PdfPTable(1) { WidthPercentage = 100, SpacingAfter = 6f };
                        var titleCell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(worksheet.Name, titleFont))
                        {
                            BackgroundColor = headerBg,
                            Padding = 8f,
                            Border = iTextSharp.text.Rectangle.NO_BORDER,
                            HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT
                        };
                        titleTable.AddCell(titleCell);
                        document.Add(titleTable);

                        // Compute relative column widths based on max content length
                        var colWidths = new float[colCount];
                        for (int col = 0; col < colCount; col++)
                        {
                            float maxLen = 4f;
                            for (int row = usedRange.FirstRow().RowNumber(); row <= usedRange.LastRow().RowNumber(); row++)
                            {
                                var cellVal = worksheet.Cell(row, usedRange.FirstColumn().ColumnNumber() + col).GetString();
                                if (cellVal.Length > maxLen) maxLen = cellVal.Length;
                            }
                            colWidths[col] = Math.Min(maxLen, 40f); // cap at 40 chars wide
                        }
                        var dataTable = new iTextSharp.text.pdf.PdfPTable(colCount)
                        {
                            WidthPercentage = 100,
                            SpacingAfter = 12f
                        };
                        dataTable.SetWidths(colWidths);

                        int dataRow = 0;
                        for (int row = usedRange.FirstRow().RowNumber(); row <= usedRange.LastRow().RowNumber(); row++)
                        {
                            bool isHeaderRow = (dataRow == 0);
                            bool isAltRow = (!isHeaderRow && dataRow % 2 == 0);
                            var rowBg = isHeaderRow ? headerBg : (isAltRow ? altRowBg : whiteBg);

                            for (int col = usedRange.FirstColumn().ColumnNumber(); col <= usedRange.LastColumn().ColumnNumber(); col++)
                            {
                                var excelCell = worksheet.Cell(row, col);
                                string val = excelCell.IsEmpty() ? "" : excelCell.GetString();

                                var pdfCell = new iTextSharp.text.pdf.PdfPCell(
                                    new iTextSharp.text.Phrase(val, isHeaderRow ? headerFont : (isAltRow ? altCellFont : cellFont)))
                                {
                                    BackgroundColor = rowBg,
                                    Padding = 5f,
                                    BorderColor = borderCol,
                                    BorderWidth = 0.5f,
                                    HorizontalAlignment = isHeaderRow
                                        ? iTextSharp.text.Element.ALIGN_CENTER
                                        : iTextSharp.text.Element.ALIGN_LEFT,
                                    VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE,
                                };
                                dataTable.AddCell(pdfCell);
                            }
                            dataRow++;
                        }

                        document.Add(dataTable);
                    }

                    document.Close();
                    return pdfStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(
                    new Exception("Excel to PDF conversion failed: " + ex.Message)));
                return null;
            }
        }


        public ActionResult GetMedicalBillDocument(string claimId = null, string slNo = null)
        {
            var res = new ApiResponse<object>();
            try
            {
                // Session check applies to browser-initiated calls only. The staging
                // worker (ProcessStagingClaims) calls this method internally with no
                // user session — _stagingInternalCall is set true for that path so we
                // skip the session gate. Browser calls leave it false → gate enforced.
                if (!_stagingInternalCall && Session[SessionValue.UserRegionID] == null)
                {
                    res.Success = false; res.ErrorCode = "ErrorCode#1";
                    res.Message = "Session expired.";
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                string cId = (claimId ?? "").Trim();
                string tarFileName = null; // will be set to actual picked file name
                string sNo = (slNo ?? "1").Trim();

                if (string.IsNullOrWhiteSpace(cId))
                {
                    res.Success = false;
                    res.Message = "ClaimID is required.";
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                string env = (System.Configuration.ConfigurationManager.AppSettings["Enviroment"] ?? "dev").ToLower().Trim();
                bool isProdOrPreprod = env == "prod" || env == "preprod" || env == "live";

                if (!isProdOrPreprod)
                {
                    // CHECKPOINT 2a (Local/QA/UAT): Load medical bill from zip file
                    // Path: ~/ClaimAIDocs/{claimId}/medicalbill.zip
                    // FAILURE HERE means:
                    //   - Zip file not placed in correct folder on server
                    //   - ClaimAIDocs folder not created under Enrollment project root
                    //   - Wrong claimId passed
                    string localBase = Server.MapPath("~/ClaimAIDocs/");
                    string zipPath = System.IO.Path.Combine(localBase, cId, "medicalbill.zip");

                    if (!System.IO.File.Exists(zipPath))
                    {
                        res.Success = false;
                        res.Message = "CHECKPOINT 2a FAILED: Medical bill zip not found at: " + zipPath
                            + ". Create folder ClaimAIDocs/" + cId + "/ and place medicalbill.zip inside it.";
                        return Json(res, JsonRequestBehavior.AllowGet);
                    }

                    // Extract all PDFs, deduplicate by content hash (MD5)
                    // Handles same content in different filenames
                    var pdfBytesList = new System.Collections.Generic.List<byte[]>();
                    var docCategoriesLocal = new System.Collections.Generic.List<string>();
                    var seenHashes = new System.Collections.Generic.HashSet<string>();

                    using (var zip = System.IO.Compression.ZipFile.OpenRead(zipPath))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            string ext = System.IO.Path.GetExtension(entry.Name).TrimStart('.').ToLower();
                            bool isImg = ext == "jpg" || ext == "jpeg" || ext == "png" || ext == "gif" || ext == "bmp" || ext == "tif" || ext == "tiff";
                            if (ext != "pdf" && !isImg) continue;
                            using (var stream = entry.Open())
                            using (var ms = new System.IO.MemoryStream())
                            {
                                stream.CopyTo(ms);
                                byte[] rawBytes = ms.ToArray();

                                // Compute MD5 hash of file content
                                string hash;
                                using (var md5 = System.Security.Cryptography.MD5.Create())
                                    hash = BitConverter.ToString(md5.ComputeHash(rawBytes)).Replace("-", "");

                                if (!seenHashes.Add(hash)) continue; // skip duplicate content

                                // Every document must render — PDFs as-is, images wrapped to PDF.
                                byte[] docPdf = null;
                                if (!isImg) { try { new iTextSharp.text.pdf.PdfReader(rawBytes).Close(); docPdf = rawBytes; } catch { docPdf = null; } }
                                if (docPdf == null) { try { docPdf = ImageToPdf(rawBytes); } catch { docPdf = null; } }
                                if (docPdf == null) continue;

                                pdfBytesList.Add(docPdf);
                                string localCat = System.IO.Path.GetFileNameWithoutExtension(entry.Name);
                                if (string.IsNullOrWhiteSpace(localCat)) localCat = "Other";
                                docCategoriesLocal.Add(localCat);
                            }
                        }
                    }

                    if (pdfBytesList.Count == 0)
                    {
                        res.Success = false;
                        res.Message = "No PDFs found in medicalbill.zip for claimId=" + cId;
                        return Json(res, JsonRequestBehavior.AllowGet);
                    }

                    // Merge and compress all pages — no page cap
                    // Large files are handled by passing URL to AI directly (not loading into Convex action memory)
                    System.Collections.Generic.List<object> pageCategoriesLocal;
                    byte[] mergedLocal = MergePdfsWithCategories(pdfBytesList, docCategoriesLocal, out pageCategoriesLocal);
                    byte[] compressedLocal = CompressPdf(mergedLocal);
                    int totalPages = new iTextSharp.text.pdf.PdfReader(mergedLocal).NumberOfPages;
                    double sizeMb = Math.Round(compressedLocal.Length / 1048576.0, 2);
                    res.Success = true;
                    res.Message = "Medical bill loaded from zip. Files: " + pdfBytesList.Count + " | Pages: " + totalPages + " | Size: " + sizeMb + "MB";
                    res.Data = new { fileName = cId + "-medicalbill.pdf", base64Content = Convert.ToBase64String(compressedLocal), pageCategories = pageCategoriesLocal };
                    var sl = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                    return Content(sl.Serialize(res), "application/json");
                }

                // ── Prod/Preprod/Live: DMS API ────────────────────────────────────────
                string dmsBaseUrl = System.Configuration.ConfigurationManager.AppSettings["DMSApiURL"].TrimEnd('/');
                string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientID"];
                string apiKey = System.Configuration.ConfigurationManager.AppSettings["DMSAPIKey"];

                System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12 |
                    System.Net.SecurityProtocolType.Tls11 |
                    System.Net.SecurityProtocolType.Tls;
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    (sender, cert, chain, errors) => true;

                string token = "";
                {
                    var client = new System.Net.Http.HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var jsonDoc = Newtonsoft.Json.JsonConvert.SerializeObject(new { clientId, apiKey });
                    var request = new System.Net.Http.HttpRequestMessage(
                        System.Net.Http.HttpMethod.Post, dmsBaseUrl + "/api/Auth/generatetoken");
                    request.Content = new System.Net.Http.StringContent(jsonDoc, null, "application/json");
                    var response = client.SendAsync(request).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                        token = response.Content.ReadAsStringAsync().Result.Trim().Trim('"');
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    res.Success = false;
                    res.Message = "DMS token generation failed.";
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                string docsJson = "";
                {
                    var client = new System.Net.Http.HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    client.DefaultRequestHeaders.Add("accept", "*/*");
                    string url = string.Format("{0}/api/Document/claimdocumenturls?claimId={1}&claimExtNo={2}",
                        dmsBaseUrl, cId, sNo);
                    var response = client.GetAsync(url).GetAwaiter().GetResult();
                    docsJson = response.Content.ReadAsStringAsync().Result;
                }

                if (string.IsNullOrWhiteSpace(docsJson) || !docsJson.TrimStart().StartsWith("["))
                {
                    res.Success = false;
                    res.Message = "No documents found in DMS for claimId=" + cId + ". Response: " + docsJson;
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                var docsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(docsJson);
                if (docsList == null || docsList.Count == 0)
                {
                    res.Success = false;
                    res.Message = "Empty document list for claimId=" + cId;
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                var pdfBytesListProd = new System.Collections.Generic.List<byte[]>();
                var docCategoriesProd = new System.Collections.Generic.List<string>();
                foreach (var doc in docsList)
                {
                    string docUrl = (doc.documentUrl ?? doc.DocumentUrl ?? "").ToString();
                    if (string.IsNullOrWhiteSpace(docUrl)) continue;
                    string docCategory = "Other";
                    try
                    {
                        string c = (doc.documentCategory ?? doc.DocumentCategory ?? "").ToString();
                        if (!string.IsNullOrWhiteSpace(c)) docCategory = c.Trim();
                    }
                    catch { docCategory = "Other"; }

                    byte[] rawBytes;
                    try
                    {
                        using (var wc = new System.Net.WebClient())
                            rawBytes = wc.DownloadData(docUrl);
                    }
                    catch { continue; }

                    // EVERY document must render. If it's already a PDF keep it as-is;
                    // if it's an image (or anything PdfReader can't open) wrap it into a
                    // single-page PDF so it still merges and renders. Only a document that
                    // is neither a valid PDF nor a valid image is skipped (last resort).
                    byte[] docPdf = null;
                    try { new iTextSharp.text.pdf.PdfReader(rawBytes).Close(); docPdf = rawBytes; }
                    catch
                    {
                        try { docPdf = ImageToPdf(rawBytes); }
                        catch { docPdf = null; }
                    }
                    if (docPdf == null) continue;

                    pdfBytesListProd.Add(docPdf);
                    docCategoriesProd.Add(docCategory);
                }

                if (pdfBytesListProd.Count == 0)
                {
                    res.Success = false;
                    res.Message = "Could not download any documents from DMS for claimId=" + cId;
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                System.Collections.Generic.List<object> pageCategoriesProd;
                byte[] mergedProd = MergePdfsWithCategories(pdfBytesListProd, docCategoriesProd, out pageCategoriesProd);
                res.Success = true;
                res.Message = "Medical bill loaded from DMS. Files: " + pdfBytesListProd.Count;
                res.Data = new { fileName = cId + "-" + sNo + "-medical-bill.pdf", base64Content = Convert.ToBase64String(mergedProd), pageCategories = pageCategoriesProd };
                var s = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                return Content(s.Serialize(res), "application/json");
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                res.Success = false;
                res.Message = "Error loading medical bill: " + ex.Message;
                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTariffDocument(string claimId = null, string slNo = null, bool wantSoc = false)
        {
            var res = new ApiResponse<object>();
            try
            {
                // See GetMedicalBillDocument: skip the session gate for internal
                // staging-worker calls (no user session); enforce it for browser calls.
                if (!_stagingInternalCall && Session[SessionValue.UserRegionID] == null)
                {
                    res.Success = false; res.ErrorCode = "ErrorCode#1";
                    res.Message = "Session expired.";
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                string cId = (claimId ?? "").Trim();
                string tarFileName = null; // will be set to actual picked file name

                // Get insurer info from DB
                string connStr = System.Configuration.ConfigurationManager
                    .ConnectionStrings["McarePlusEntities"].ConnectionString;
                if (connStr.StartsWith("metadata=", StringComparison.OrdinalIgnoreCase))
                {
                    var m = System.Text.RegularExpressions.Regex.Match(
                        connStr, "provider connection string=\\\"([^\\\"]+)\\\"",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (m.Success) connStr = m.Groups[1].Value.Replace("&quot;", "\"");
                }

                int insurerId = 0;
                string insurerCode = "";
                if (!string.IsNullOrWhiteSpace(cId) && long.TryParse(cId, out long cIdLong))
                    GetClaimInsurerInfo(cIdLong, connStr, out insurerId, out insurerCode);

                bool isPsu = PsuInsurerIds.Contains(insurerId);

                string env = (System.Configuration.ConfigurationManager.AppSettings["Enviroment"] ?? "dev").ToLower().Trim();
                bool isProdOrPreprod = env == "prod" || env == "preprod" || env == "live";

                if (!isProdOrPreprod)
                {
                    string localBase = Server.MapPath("~/ClaimAIDocs/");
                    string zipPath = System.IO.Path.Combine(localBase, cId, "tariff.zip");

                    if (!System.IO.File.Exists(zipPath))
                    {
                        res.Success = false;
                        res.Message = "CHECKPOINT 3a FAILED: Tariff zip not found at: " + zipPath +
                            ". Create folder ClaimAIDocs/" + cId + "/ and place tariff.zip inside it.";
                        return Json(res, JsonRequestBehavior.AllowGet);
                    }

                    var allTariffCandidates = new System.Collections.Generic.List<System.Tuple<string, DateTime, byte[]>>();
                    using (var rootStream = System.IO.File.OpenRead(zipPath))
                    {
                        CollectTariffFilesFromZip(rootStream, "", allTariffCandidates);
                    }

                    // A "discount" sheet is not a tariff — never let it become a selection
                    // candidate (so neither the AI nor the rule-based fallback can pick it).
                    int beforeDiscFilter = allTariffCandidates.Count;
                    allTariffCandidates.RemoveAll(f => f.Item1 != null &&
                        f.Item1.IndexOf("discount", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (allTariffCandidates.Count != beforeDiscFilter)
                        TariffLog("[Tariff] Excluded " + (beforeDiscFilter - allTariffCandidates.Count) + " discount file(s) — not tariffs.");

                    byte[] tariffBytes = null;

                    TariffLog("[Tariff] STEP 1 — Zip scanned recursively. Total tariff files=" + allTariffCandidates.Count);
                    TariffLog("[Tariff] STEP 1 — InsurerCode=" + insurerCode + " IsPSU=" + isPsu + " ClaimID=" + cId);

                    if (allTariffCandidates.Count > 0)
                    {
                        TariffLog("[Tariff] STEP 2 — All candidate file names collected (" + allTariffCandidates.Count + "): " + string.Join(" | ", allTariffCandidates.ConvertAll(f => f.Item1 + " [" + f.Item2.ToString("yyyy-MM-dd") + "]")));

                        // GIPSA PPN->SOC two-pass (SOC request): return the SOC fallback file
                        // ONLY for PSU claims whose GIPSA zip contains BOTH a PPN and a SOC file.
                        // Otherwise return an explicit "no SOC" so the caller sends only the PPN.
                        if (wantSoc)
                        {
                            if (isPsu && GipsaZipHasBothPpnAndSoc(allTariffCandidates))
                            {
                                var socPick = PickBestSocTariffFile(allTariffCandidates);
                                if (socPick != null)
                                {
                                    tariffBytes = socPick.Item2;
                                    tarFileName = socPick.Item1;
                                    TariffLog("[Tariff] SOC PASS — selected SOC file: " + tarFileName);
                                }
                            }
                            if (tariffBytes == null)
                            {
                                res.Success = true;
                                res.Message = "No SOC tariff (not PSU, or GIPSA zip lacks both PPN and SOC).";
                                res.Data = new { fileName = (string)null, base64Content = (string)null };
                                var slSoc = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                                return Content(slSoc.Serialize(res), "application/json");
                            }
                        }
                        else
                        {
                            TariffLog("[Tariff] STEP 3 — Sending to AI for selection...");
                            var aiPicked = PickTariffFileWithAI(allTariffCandidates, isPsu, insurerCode);
                            tariffBytes = aiPicked?.Item2;
                            tarFileName = aiPicked?.Item1 ?? tarFileName;
                            TariffLog("[Tariff] STEP 4 — AI selected: " + (tarFileName ?? "null"));
                        }
                    }

                    if (tariffBytes == null)
                    {
                        res.Success = false;
                        res.Message = "Could not pick tariff file from zip. InsurerID=" + insurerId + " Code=" + insurerCode;
                        return Json(res, JsonRequestBehavior.AllowGet);
                    }

                    res.Success = true;
                    res.Message = "Tariff loaded from zip. IsPSU=" + isPsu + " InsurerCode=" + insurerCode
                                + " || TARIFF SELECTION: " + _lastTariffSelectionLog;
                    res.Data = new { fileName = tarFileName ?? (cId + "-tariff.pdf"), base64Content = Convert.ToBase64String(tariffBytes) };
                    var sl2 = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                    return Content(sl2.Serialize(res), "application/json");
                }

                long claimIdLong2 = 0;
                long.TryParse(cId, out claimIdLong2);

                int slNoInt = 1;
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandTimeout = 300;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "USP_ClaimAI_ResolveClaimRow";
                    cmd.Parameters.AddWithValue("@ClaimID", claimIdLong2);
                    cmd.Parameters.Add("@SlNo", System.Data.SqlDbType.Int).Value = DBNull.Value;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read() && rdr["LowestSlNo"] != DBNull.Value)
                            slNoInt = Convert.ToInt32(rdr["LowestSlNo"]);
                    }
                }

                long providerId = 0;
                string mouId = "";
                using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    cmd.CommandText = "USP_ClaimMedicalScrutiny_Retrieve";
                    cmd.Parameters.AddWithValue("@ClaimID", claimIdLong2);
                    cmd.Parameters.AddWithValue("@Slno", slNoInt);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            if (!rdr.IsDBNull(rdr.GetOrdinal("ProviderID")))
                                providerId = Convert.ToInt64(rdr["ProviderID"]);
                            if (!rdr.IsDBNull(rdr.GetOrdinal("MOUID")))
                                mouId = rdr["MOUID"].ToString();
                        }
                    }
                }

                if (providerId == 0)
                {
                    res.Success = false;
                    res.Message = "ProviderID not found for claimId=" + cId;
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                // S3 location config. The download client below is region-only -> IAM-role auth
                // (same as the working "Tariff Documents" screen). The ProviderDocaccesskey/
                // ProviderDocsecretkey reads were removed (they were the credential root cause).
                // NOTE: ProviderTariffDocumentPath is CASE-SENSITIVE in S3 (e.g. "tariffdocs/"
                // vs "TariffDocs/") — it must match the real bucket folder exactly.
                string s3Bucket = System.Configuration.ConfigurationManager.AppSettings["ProviderDocbucketname"] ?? "prod-spectra-app-s3-provider-docs";
                string docPath = System.Configuration.ConfigurationManager.AppSettings["ProviderTariffDocumentPath"] ?? "TariffDocs/";
                string webShare2 = System.Configuration.ConfigurationManager.AppSettings["ProviderTariffDocumentPathWebShare"] ?? "WebShareDocs/";

                // Enumerate ALL tariff DOC IDs mapped to this provider, restricted to file types
                // the PDF pipeline can consume (.pdf and .zip; .xls/.xlsx are skipped). We do NOT
                // filter by MOU here: a tariff mapped under a different MOU is still a real file,
                // and the previous "AND (Mp.MOUID = @MOUID OR Mp.MOUID = 0)" filter was discarding
                // valid tariffs (incl. the current one). Insurer relevance is decided downstream
                // by PickTariffFileWithAI from the filenames, not by this query. Each doc's REAL
                // S3 key is resolved per-doc below from Usp_TariffUploadDoc_FillDetails — the same
                // source the working "Tariff Documents" screen uses.
                // Build the candidate list the SAME way the working "All Tariffs" popup
                // does (Common/ProviderTariff -> GetTariffDocsInfo(providerId, MOUID, ...)).
                //
                // WHY THIS CHANGED: the previous raw ProviderTariffDocs/ProviderTariff_Map
                // query enumerated EVERY tariff mapped to the provider IGNORING MOU. A single
                // zip mapped across many MOUs could therefore be picked for a claim whose real
                // MOU points at a different tariff — the wrong-tariff bug. The working popup
                // never uses that raw query: it delegates to the stored procedure
                // Usp_TariffUploadDoc_FillDetails WITH the claim's MOU, and the SP returns the
                // MOU-appropriate documents. We do exactly that here so extraction and the
                // popup resolve to the same file. Per-doc S3 key resolution below is unchanged
                // (it already calls the same SP by @TariffDocId, like Provider/ViewTariffDoc).
                //
                // "Mapped" = tariffs mapped to this provider+MOU. If that yields nothing we fall
                // back to "NotMapped" (the popup shows both), and only then to the old
                // provider-wide raw query, so we never regress a claim that has no MOU-mapped
                // tariff. Columns returned by the SP: FileId, FileName, SystemFileName,
                // ProviderId, isOldDoc, UpdateDate (same columns Provider/ViewTariffDoc reads).
                var tariffDocs = new System.Collections.Generic.List<System.Tuple<long, string, DateTime>>();

                // Local helper: append rows from a GetTariffDocsInfo DataTable into tariffDocs,
                // keeping only PDF/ZIP system files (xls/xlsx are not consumable downstream).
                System.Action<System.Data.DataTable> addFromDocsInfo = (dtDocs) =>
                {
                    if (dtDocs == null) return;
                    foreach (System.Data.DataRow dr in dtDocs.Rows)
                    {
                        if (dr["FileId"] == null || dr["FileId"] == DBNull.Value) continue;
                        long docId = Convert.ToInt64(dr["FileId"]);

                        string sysFn = dtDocs.Columns.Contains("SystemFileName") && dr["SystemFileName"] != DBNull.Value
                            ? dr["SystemFileName"].ToString() : "";
                        // Only PDFs and ZIPs are usable by the extraction pipeline.
                        if (!(sysFn.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                              || sysFn.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)))
                            continue;

                        string displayName = dtDocs.Columns.Contains("FileName") && dr["FileName"] != DBNull.Value
                            ? dr["FileName"].ToString() : "";

                        DateTime updateDate = DateTime.MinValue;
                        if (dtDocs.Columns.Contains("UpdateDate") && dr["UpdateDate"] != DBNull.Value)
                            DateTime.TryParse(dr["UpdateDate"].ToString(), out updateDate);

                        // Skip duplicates by DocId (NotMapped may repeat a Mapped row).
                        if (tariffDocs.Exists(t => t.Item1 == docId)) continue;

                        tariffDocs.Add(System.Tuple.Create(docId, displayName, updateDate));
                    }
                };

                try
                {
                    var providerVm = new Enrollment.ViewModel.ProviderViewModel();

                    // 1) MOU-mapped tariffs (identical call to the working popup).
                    System.Data.DataTable dtMapped =
                        providerVm.GetTariffDocsInfo(providerId, (mouId ?? "").ToString(), 0, "Mapped");
                    addFromDocsInfo(dtMapped);
                    TariffLog("[Tariff] GetTariffDocsInfo Mapped (ProviderID=" + providerId
                              + ", MOUID=" + (mouId ?? "") + ") -> " + tariffDocs.Count + " usable doc(s).");

                    // 2) NotMapped fallback ONLY if nothing mapped — mirrors the popup showing both.
                    if (tariffDocs.Count == 0)
                    {
                        System.Data.DataTable dtNotMapped =
                            providerVm.GetTariffDocsInfo(providerId, (mouId ?? "").ToString(), 0, "NotMapped");
                        addFromDocsInfo(dtNotMapped);
                        TariffLog("[Tariff] GetTariffDocsInfo NotMapped fallback -> " + tariffDocs.Count + " usable doc(s).");
                    }
                }
                catch (Exception docsEx)
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(
                        new Exception("GetTariffDocsInfo failed for ProviderID=" + providerId
                                      + " MOUID=" + (mouId ?? "") + " — " + docsEx.Message, docsEx)));
                }

                // 3) Last-resort fallback: the original provider-wide raw query. Only runs when
                // the SP returned nothing at all (e.g. mapping edge cases), so behaviour for such
                // claims is no worse than before this change.
                if (tariffDocs.Count == 0)
                {
                    using (var conn = new System.Data.SqlClient.SqlConnection(connStr))
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandType = System.Data.CommandType.Text;

                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "USP_ClaimAI_GetProviderTariffDocs";

                        cmd.Parameters.AddWithValue("@ProviderID", providerId);

                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                if (rdr.IsDBNull(rdr.GetOrdinal("DocId"))) continue;
                                long docId = Convert.ToInt64(rdr["DocId"]);
                                string displayName = rdr["DisplayName"]?.ToString() ?? "";

                                DateTime updateDate = rdr.IsDBNull(rdr.GetOrdinal("UpdateDate"))
                                    ? DateTime.MinValue
                                    : Convert.ToDateTime(rdr["UpdateDate"]);

                                tariffDocs.Add(System.Tuple.Create(docId, displayName, updateDate));
                            }
                        }
                    }
                    TariffLog("[Tariff] Provider-wide raw-query last-resort fallback -> " + tariffDocs.Count + " doc(s).");
                }

                if (tariffDocs.Count == 0)
                {
                    res.Success = false;
                    res.Message = "No tariff files found for providerId=" + providerId;
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                // For each mapped doc, resolve its S3 key the SAME way the working "Tariff
                // Documents" screen does (DownloadTariffDoc / ViewTariffDocument):
                //   DataTable dt = GetTariffDocsInfo(0, "", FileId);   // Usp_TariffUploadDoc_FillDetails
                //   string buckName = dt["ProviderId"] + "/" + ProviderTariffDocumentPath;
                //   if (dt["isOldDoc"] == "yes") buckName = ProviderTariffDocumentPathWebShare;
                //   key = buckName + dt["SystemFileName"];
                // i.e. the owner ProviderId and isOldDoc come straight from the SP — identical
                // logic, so the key matches the working screen exactly.
                //
                // Download auth is the region-only IAM-role client (same as the screen's
                // DownloadFileAsync). We use synchronous GetObject instead of awaiting
                // DownloadFileAsync only because this action is synchronous — the auth and the
                // resulting bytes are identical. A downloaded .zip is expanded via
                // CollectTariffFilesFromZip (same as the local-zip branch) so PickTariffFileWithAI
                // always receives PDFs. Per-doc S3 errors are logged to Elmah.

                // Details for EVERY tariff document in one round trip, instead of a new
                // connection and a new Usp_TariffUploadDoc_FillDetails call per document
                // inside the loop below. Keyed by ProviderTariff_Map.Id (the "FileId"),
                // which is what that SP filters on.
                var tariffDocInfo = new System.Collections.Generic.Dictionary<
                    long, System.Tuple<string, string, string>>();   // ProviderId, SystemFileName, isOldDoc
                if (tariffDocs.Count > 0)
                {
                    try
                    {
                        string idCsv = string.Join(",", tariffDocs.ConvertAll(x => x.Item1.ToString()).ToArray());
                        using (var infoConn = new System.Data.SqlClient.SqlConnection(connStr))
                        {
                            infoConn.Open();
                            var batchCmd = infoConn.CreateCommand();
                            batchCmd.CommandType = System.Data.CommandType.StoredProcedure;
                            batchCmd.CommandText = "USP_ClaimAI_GetTariffDocDetails";
                            batchCmd.CommandTimeout = 60;
                            batchCmd.Parameters.AddWithValue("@TariffDocIds", idCsv);
                            using (var br = batchCmd.ExecuteReader())
                            {
                                while (br.Read())
                                {
                                    long key = Convert.ToInt64(br["TariffDocId"]);
                                    tariffDocInfo[key] = System.Tuple.Create(
                                        br["ProviderId"] != DBNull.Value ? br["ProviderId"].ToString() : null,
                                        br["SystemFileName"] != DBNull.Value ? br["SystemFileName"].ToString() : null,
                                        (br["IsOldDoc"] != DBNull.Value ? br["IsOldDoc"].ToString() : "no").ToLower().Trim());
                                }
                            }
                        }
                        TariffLog("[Tariff] Batch doc details: asked " + tariffDocs.Count
                                  + ", got " + tariffDocInfo.Count + " in one round trip.");
                    }
                    catch (Exception batchEx)
                    {
                        // Leave the dictionary empty and let the loop log per-doc as before.
                        Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(
                            new Exception("USP_ClaimAI_GetTariffDocDetails failed: " + batchEx.Message, batchEx)));
                    }
                }

                var s3TariffCandidates = new System.Collections.Generic.List<System.Tuple<string, DateTime, byte[]>>();

                using (var s3Client = new Amazon.S3.AmazonS3Client(Amazon.RegionEndpoint.APSouth1))
                {
                    foreach (var td in tariffDocs)
                    {
                        long docId = td.Item1;
                        string displayName = td.Item2;
                        DateTime modDate = td.Item3;

                        string ownerProviderId = null;
                        string sysName = null;
                        string isOldDoc = "no";

                        // Fetched in one batch above. Same three values the per-document
                        // Usp_TariffUploadDoc_FillDetails call used to return.
                        System.Tuple<string, string, string> di;
                        if (tariffDocInfo.TryGetValue(docId, out di))
                        {
                            ownerProviderId = di.Item1;
                            sysName = di.Item2;
                            isOldDoc = di.Item3;
                        }

                        if (string.IsNullOrWhiteSpace(sysName))
                        {
                            Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(
                                new Exception("Tariff doc has no SystemFileName from SP. DocId=" + docId)));
                            continue;
                        }

                        // Build the key the working screen's way (SP isOldDoc decides the
                        // primary location), but ALSO keep the other location as a fallback so a
                        // wrong/edge isOldDoc doesn't lose a file that actually exists. Try the
                        // authoritative location first, the other second; first hit wins.
                        //   provider-folder : {ownerProviderId}/{ProviderTariffDocumentPath}{SystemFileName}
                        //   legacy flat     : {ProviderTariffDocumentPathWebShare}{SystemFileName}
                        string folderKey = ownerProviderId + "/" + docPath + sysName;
                        string webShareKey = webShare2 + sysName;
                        string[] candidateKeys = (isOldDoc == "yes")
                            ? new[] { webShareKey, folderKey }   // SP says old -> webshare first
                            : new[] { folderKey, webShareKey };  // SP says new -> provider folder first

                        byte[] objBytes = null;
                        string usedKey = null;
                        foreach (var key in candidateKeys)
                        {
                            try
                            {
                                var getReq = new Amazon.S3.Model.GetObjectRequest { BucketName = s3Bucket, Key = key };
                                using (var getResp = s3Client.GetObject(getReq))
                                using (var ms = new System.IO.MemoryStream())
                                {
                                    getResp.ResponseStream.CopyTo(ms);
                                    objBytes = ms.ToArray();
                                    usedKey = key;
                                }
                                if (objBytes != null && objBytes.Length > 0) break; // got it
                            }
                            catch (Amazon.S3.AmazonS3Exception s3ex)
                            {
                                // NoSuchKey / 404 -> wrong location, try the fallback key.
                                if (s3ex.ErrorCode == "NoSuchKey" || (int)s3ex.StatusCode == 404)
                                    continue;
                                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(
                                    new Exception("Tariff S3 GetObject error. Bucket=" + s3Bucket +
                                                  " Key=" + key + " DocId=" + docId + " — " + s3ex.Message, s3ex)));
                            }
                            catch (Exception s3ex)
                            {
                                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(
                                    new Exception("Tariff S3 GetObject error. Bucket=" + s3Bucket +
                                                  " Key=" + key + " DocId=" + docId + " — " + s3ex.Message, s3ex)));
                            }
                        }

                        if (objBytes == null || objBytes.Length == 0)
                        {
                            Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(
                                new Exception("Tariff S3: no object found for DocId=" + docId +
                                              " SystemFileName=" + sysName +
                                              " (tried: " + string.Join("  |  ", candidateKeys) + ")")));
                            continue;
                        }

                        // Diagnostic: record which S3 key actually served the bytes and the
                        // content's magic header. Makes a "valid in S3 but rejected here" case
                        // visible — e.g. a wrong/stale copy or a non-PDF body under a .pdf name.
                        {
                            int mb = Math.Min(objBytes.Length, 8);
                            var magic = new System.Text.StringBuilder();
                            for (int b = 0; b < mb; b++) magic.Append(objBytes[b].ToString("X2")).Append(' ');
                            string ascii = System.Text.Encoding.ASCII.GetString(objBytes, 0, mb);
                            TariffLog("[Tariff] S3 GET ok — DocId=" + docId + " usedKey=" + (usedKey ?? "null")
                                      + " bytes=" + objBytes.Length + " magic=[" + magic.ToString().Trim()
                                      + "] ascii=[" + ascii + "]");
                        }

                        // Carry this doc's display FileName (the one with the insurer/hospital/
                        // dates, e.g. "...HDFC Ergo (20-SEP-2025)") onto its candidate(s), so the
                        // AI selector gets the same meaningful "folder_file" context the local
                        // branch gets from inner-zip names — and so files with the same inner name
                        // across different tariffs don't collide. Fall back to SystemFileName if
                        // the display name is missing.
                        string ctx = !string.IsNullOrWhiteSpace(displayName) ? displayName : sysName;
                        if (ctx.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                            ctx = ctx.Substring(0, ctx.Length - 4);

                        if (sysName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            // Expand the zip, then prefix each just-extracted file with ctx
                            // (independent of CollectTariffFilesFromZip's own prefix handling).
                            int beforeExpand = s3TariffCandidates.Count;
                            using (var zipStream = new System.IO.MemoryStream(objBytes))
                            {
                                CollectTariffFilesFromZip(zipStream, "", s3TariffCandidates);
                            }
                            for (int i = beforeExpand; i < s3TariffCandidates.Count; i++)
                            {
                                var ex = s3TariffCandidates[i];
                                s3TariffCandidates[i] = System.Tuple.Create(ctx + "_" + ex.Item1, ex.Item2, ex.Item3);
                            }
                        }
                        else
                        {
                            // Direct file (not a zip): use the display FileName as the candidate name.
                            string directName = !string.IsNullOrWhiteSpace(displayName) ? displayName : sysName;
                            s3TariffCandidates.Add(System.Tuple.Create(directName, modDate, objBytes));
                        }
                    }
                }

                // A "discount" sheet is not a tariff — drop any such file (including files
                // expanded out of a zip) so it can't be selected by AI or the rule fallback.
                s3TariffCandidates.RemoveAll(f => f.Item1 != null &&
                    f.Item1.IndexOf("discount", StringComparison.OrdinalIgnoreCase) >= 0);

                if (s3TariffCandidates.Count == 0)
                {
                    res.Success = false;
                    res.Message = "Could not download any tariff files from S3 for providerId=" + providerId;
                    return Json(res, JsonRequestBehavior.AllowGet);
                }

                // Log the full candidate names (including the FileName prefix carried onto zip
                // contents) — same visibility the local branch gives via STEP 2 / STEP 4.
                TariffLog("[Tariff] STEP 1 (S3) — Files downloaded=" + s3TariffCandidates.Count
                          + " InsurerCode=" + insurerCode + " IsPSU=" + isPsu + " ProviderID=" + providerId);
                TariffLog("[Tariff] STEP 2 (S3) — All candidate file names collected (" + s3TariffCandidates.Count + "): "
                          + string.Join(" | ", s3TariffCandidates.ConvertAll(f => f.Item1 + " [" + f.Item2.ToString("yyyy-MM-dd") + "]")));
                // GIPSA PPN->SOC two-pass (SOC request): return the SOC fallback file ONLY
                // for PSU claims whose GIPSA zip contains BOTH a PPN and a SOC file. Otherwise
                // an explicit "no SOC" so the caller sends only the PPN.
                if (wantSoc)
                {
                    byte[] socBytes = null; string socName = null;
                    if (isPsu && GipsaZipHasBothPpnAndSoc(s3TariffCandidates))
                    {
                        var socPick = PickBestSocTariffFile(s3TariffCandidates);
                        if (socPick != null) { socBytes = socPick.Item2; socName = socPick.Item1; }
                    }
                    res.Success = true;
                    if (socBytes != null)
                    {
                        TariffLog("[Tariff] SOC PASS (S3) — selected SOC file: " + socName);
                        res.Message = "SOC tariff loaded from S3. InsurerCode=" + insurerCode;
                        res.Data = new { fileName = socName, base64Content = Convert.ToBase64String(socBytes) };
                    }
                    else
                    {
                        res.Message = "No SOC tariff (not PSU, or GIPSA zip lacks both PPN and SOC).";
                        res.Data = new { fileName = (string)null, base64Content = (string)null };
                    }
                    var s3Soc = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                    return Content(s3Soc.Serialize(res), "application/json");
                }

                TariffLog("[Tariff] STEP 3 (S3) — Sending to AI for selection...");

                var bestTariffResult = PickTariffFileWithAI(s3TariffCandidates, isPsu, insurerCode);
                byte[] bestTariff = bestTariffResult?.Item2;
                string bestTariffName = bestTariffResult?.Item1 ?? (providerId + "-tariff.pdf");
                TariffLog("[Tariff] STEP 4 (S3) — AI selected: " + (bestTariffResult?.Item1 ?? "null"));
                if (bestTariff == null)
                {
                    res.Success = false;
                    res.Message = "Selected tariff file could not be read as a valid PDF "
                                + "(it may be corrupt, password-protected, or not actually a PDF). "
                                + "Files=" + s3TariffCandidates.Count
                                + " || TARIFF SELECTION: " + _lastTariffSelectionLog;
                    TariffLog("[Tariff] STEP 4 (S3) — FAILED: selected tariff bytes are null "
                              + "(EnsurePdf rejected the file). " + _lastTariffSelectionLog);
                    return Json(res, JsonRequestBehavior.AllowGet);
                }
                res.Success = true;
                res.Message = "Tariff loaded from S3. IsPSU=" + isPsu + " InsurerCode=" + insurerCode + " Files=" + s3TariffCandidates.Count
                            + " || TARIFF SELECTION: " + _lastTariffSelectionLog;
                res.Data = new { fileName = bestTariffName, base64Content = Convert.ToBase64String(bestTariff) };
                var s3 = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                return Content(s3.Serialize(res), "application/json");
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                res.Success = false;
                res.Message = "Error loading tariff: " + ex.Message;
                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        private byte[] CompressPdf(byte[] pdfBytes)
        {
            try
            {
                using (var input = new System.IO.MemoryStream(pdfBytes))
                using (var output = new System.IO.MemoryStream())
                {
                    var reader = new iTextSharp.text.pdf.PdfReader(input);
                    // Reduce image quality to compress scanned PDFs
                    reader.RemoveUnusedObjects();
                    var document = new iTextSharp.text.Document();
                    var writer = new iTextSharp.text.pdf.PdfCopy(document, output)
                    {
                        CompressionLevel = iTextSharp.text.pdf.PdfStream.BEST_COMPRESSION
                    };
                    writer.SetFullCompression();
                    document.Open();
                    for (int p = 1; p <= reader.NumberOfPages; p++)
                        writer.AddPage(writer.GetImportedPage(reader, p));
                    document.Close();
                    reader.Close();
                    byte[] compressed = output.ToArray();
                    // Only use compressed if it's actually smaller
                    return compressed.Length < pdfBytes.Length ? compressed : pdfBytes;
                }
            }
            catch { return pdfBytes; }
        }

        private byte[] ImageToPdf(byte[] imageBytes)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                var img = iTextSharp.text.Image.GetInstance(imageBytes);
                var pageSize = new iTextSharp.text.Rectangle(img.Width, img.Height);
                var doc = new iTextSharp.text.Document(pageSize, 0, 0, 0, 0);
                iTextSharp.text.pdf.PdfWriter.GetInstance(doc, ms);
                doc.Open();
                img.SetAbsolutePosition(0, 0);
                doc.Add(img);
                doc.Close();
                return ms.ToArray();
            }
        }

        private byte[] MergePdfsWithCategories(
            System.Collections.Generic.List<byte[]> pdfList,
            System.Collections.Generic.List<string> categories,
            out System.Collections.Generic.List<object> pageCategories)
        {
            pageCategories = new System.Collections.Generic.List<object>();
            try
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    var document = new iTextSharp.text.Document();
                    var writer = new iTextSharp.text.pdf.PdfCopy(document, ms);
                    document.Open();
                    // NO page dedup here: image documents wrapped to PDF share an
                    // identical content stream (only the embedded image XObject differs),
                    // so hashing GetPageContent would wrongly collapse every scanned page
                    // into one. Every page of every document must render.
                    int outPage = 0;
                    for (int i = 0; i < pdfList.Count; i++)
                    {
                        string cat = (i < categories.Count && !string.IsNullOrWhiteSpace(categories[i])) ? categories[i] : "Other";
                        try
                        {
                            var reader = new iTextSharp.text.pdf.PdfReader(pdfList[i]);
                            for (int p = 1; p <= reader.NumberOfPages; p++)
                            {
                                writer.AddPage(writer.GetImportedPage(reader, p));
                                outPage++;
                                pageCategories.Add(new { pageNumber = outPage, category = cat });
                            }
                            reader.Close();
                        }
                        catch
                        {
                            // A single bad document must not drop the rest — skip only it.
                        }
                    }
                    document.Close();
                    return ms.ToArray();
                }
            }
            catch
            {
                pageCategories = new System.Collections.Generic.List<object>();
                return pdfList.Count > 0 ? pdfList[0] : new byte[0];
            }
        }
    }
}
