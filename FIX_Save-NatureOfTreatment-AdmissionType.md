# Spectra - save Nature of Treatment, Type of Nature of Treatment and Admission Type on iframe Save

Two files change: `Index.cshtml` and `MedicalScrutinyController.cs`.
**No ClaimAI change** - all three are fixed values, so nothing new crosses the postMessage.

---

## Where the columns actually live

Confirmed from your schema dump:

```
Claims    NatureofTreatmentType_P43    int
Claims    AdmissionTypeID              tinyint
```

Both are on **Claims**, not Claimsdetails - so they do NOT belong in
`SaveClinicalDetailsForClaimAI` (which updates Claimsdetails). They go into
`SaveHospitalizationDetailsForClaimAI`, which already runs
`UPDATE Claims ... WHERE ID = @ClaimID` for BillNo / RoomDays / ICUDays and already
receives `claimId`. No column resolver and no extra lookup needed.

Two notes carried over from the code you gave me earlier:

- Nature of Treatment and Type of Nature of Treatment share ONE column. In
  `Fill_OPDHospitalizationDetails`, when `NatureofTreatmentType_P43` is `196` both
  dropdowns are set to `196`; when it is 197-200 the parent is set to `267` (AYUSH)
  and the child holds the value. The column stores the CHILD, so Allopathy is a
  single write.
- `AdmissionTypeID` is **tinyint**, so the "Planned" property value must be 1-255.
  The patch guards that range rather than letting an out-of-range value throw.

Values, from your spec:

| Field | Dropdown id | Value |
|---|---|---|
| Nature of Treatment | `#ddlNatureofTreatmentType` | Allopathy |
| Type of Nature of Treatment | `#ddlTypeofNatureofTreatmentType` | Allopathy |
| Admission Type | `#ddlAdmissionType` | Planned |

The dropdowns are set **by visible option text**, not by hard-coded ids - the same
approach `setSelectByText` already uses in the auto-sequence.

---

## Step 1 - Index.cshtml

Inside the `setClinicalDetails` handler, in the block that saves hospitalization
details (the one that sets ApprovedFacility and BillNo).

### FIND

```javascript
                        // 4. Bill No - use existing value if present, else default to '135'
```

and insert IMMEDIATELY ABOVE that line:

```javascript
                        // Nature of Treatment / Type of Nature of Treatment / Admission Type.
                        // Selected BY VISIBLE TEXT so no property-value id is hard-coded here.
                        // Set in the DOM as well as sent, so the doctor sees the same values
                        // and a later native Spectra save keeps them.
                        var claimAI_setDdlByText = function (selector, text) {
                            var $d = $(selector), val = null;
                            if (!$d.length) return null;
                            $d.find('option').each(function () {
                                if ($.trim($(this).text()).toLowerCase() === text.toLowerCase()) {
                                    val = $(this).val();
                                    return false;
                                }
                            });
                            if (val !== null && val !== '') { $d.val(val).trigger('change'); }
                            return val;
                        };
                        // Parent first: its change handler rebuilds the Type list synchronously,
                        // so the Allopathy option exists by the time the next line runs.
                        claimAI_setDdlByText('#ddlNatureofTreatmentType', 'Allopathy');
                        var claimAI_natureTypeId    = claimAI_setDdlByText('#ddlTypeofNatureofTreatmentType', 'Allopathy');
                        var claimAI_admissionTypeId = claimAI_setDdlByText('#ddlAdmissionType', 'Planned');
                        console.log('[ClaimAI] NatureOfTreatmentType=' + claimAI_natureTypeId
                            + ' AdmissionType=' + claimAI_admissionTypeId);

```

### FIND

```javascript
                            data: {
                                claimId:           $('#hdnClaimID').val(),
                                slNo:              $('#hdnClaimSlNo').val(),
                                approvedFacilityId: claimAI_approvedFacId,
                                billNo:            claimAI_billVal
                            },
```

### REPLACE

```javascript
                            data: {
                                claimId:           $('#hdnClaimID').val(),
                                slNo:              $('#hdnClaimSlNo').val(),
                                approvedFacilityId: claimAI_approvedFacId,
                                billNo:            claimAI_billVal,
                                natureOfTreatmentTypeId: claimAI_natureTypeId || '',
                                admissionTypeId:         claimAI_admissionTypeId || ''
                            },
```

---

## Step 2 - MedicalScrutinyController.cs

### 2a. Signature

FIND:

```csharp
        public ActionResult SaveHospitalizationDetailsForClaimAI(
            string claimId = null,
            string slNo = null,
            string approvedFacilityId = null,
            string billNo = null)
```

REPLACE:

```csharp
        public ActionResult SaveHospitalizationDetailsForClaimAI(
            string claimId = null,
            string slNo = null,
            string approvedFacilityId = null,
            string billNo = null,
            string natureOfTreatmentTypeId = null,
            string admissionTypeId = null)
```

### 2b. The UPDATE Claims statement

FIND:

```csharp
                    // 1. Update Claims table: BillNo, and ensure RoomDays/ICUDays are non-zero
                    //    (some validations block save when both are 0)
                    var cmdClaims = conn.CreateCommand();
                    cmdClaims.CommandText = @"
                        UPDATE Claims
                        SET    BillNo  = @BillNo,
                               RoomDays = CASE WHEN ISNULL(RoomDays, 0) = 0 AND ISNULL(ICUDays, 0) = 0 THEN 1 ELSE RoomDays END,
                               ICUDays  = CASE WHEN ISNULL(RoomDays, 0) = 0 AND ISNULL(ICUDays, 0) = 0 THEN 1 ELSE ICUDays  END
                        WHERE  ID = @ClaimID
                          AND  ISNULL(Deleted, 0) = 0";
                    cmdClaims.Parameters.AddWithValue("@BillNo", billNoVal);
                    cmdClaims.Parameters.AddWithValue("@ClaimID", claimIdLong);
                    rowsAffected += cmdClaims.ExecuteNonQuery();
```

REPLACE:

```csharp
                    // 1. Update Claims table: BillNo, RoomDays/ICUDays non-zero (some
                    //    validations block save when both are 0), plus Nature of Treatment
                    //    and Admission Type.
                    //
                    //    Nature of Treatment and Type of Nature of Treatment are TWO dropdowns
                    //    but ONE column: it stores the CHILD (type) value - 196 for Allopathy -
                    //    and the parent is derived from it on load (see Fill_OPDHospitalizationDetails).
                    //
                    //    COALESCE leaves each column untouched when the caller sends nothing,
                    //    so the staging auto-save path (which has no dropdowns to read) keeps
                    //    working exactly as before.
                    int natureTypeIdVal;
                    object natureParam =
                        !string.IsNullOrWhiteSpace(natureOfTreatmentTypeId)
                        && int.TryParse(natureOfTreatmentTypeId.Trim(), out natureTypeIdVal)
                        && natureTypeIdVal > 0
                            ? (object)natureTypeIdVal
                            : DBNull.Value;

                    // AdmissionTypeID is tinyint - anything outside 1-255 would throw on
                    // ExecuteNonQuery and lose the whole hospitalization save, so skip it
                    // instead and log.
                    int admitIdVal;
                    object admitParam = DBNull.Value;
                    if (!string.IsNullOrWhiteSpace(admissionTypeId)
                        && int.TryParse(admissionTypeId.Trim(), out admitIdVal))
                    {
                        if (admitIdVal > 0 && admitIdVal <= 255)
                            admitParam = (object)(byte)admitIdVal;
                        else
                            TariffLog("[ClaimAI] AdmissionTypeID " + admitIdVal
                                + " is outside tinyint range - not saved.");
                    }

                    var cmdClaims = conn.CreateCommand();
                    cmdClaims.CommandText = @"
                        UPDATE Claims
                        SET    BillNo  = @BillNo,
                               RoomDays = CASE WHEN ISNULL(RoomDays, 0) = 0 AND ISNULL(ICUDays, 0) = 0 THEN 1 ELSE RoomDays END,
                               ICUDays  = CASE WHEN ISNULL(RoomDays, 0) = 0 AND ISNULL(ICUDays, 0) = 0 THEN 1 ELSE ICUDays  END,
                               NatureofTreatmentType_P43 = COALESCE(@NatureOfTreatmentTypeID, NatureofTreatmentType_P43),
                               AdmissionTypeID           = COALESCE(@AdmissionTypeID, AdmissionTypeID)
                        WHERE  ID = @ClaimID
                          AND  ISNULL(Deleted, 0) = 0";
                    cmdClaims.Parameters.AddWithValue("@BillNo", billNoVal);
                    cmdClaims.Parameters.Add("@NatureOfTreatmentTypeID", System.Data.SqlDbType.Int).Value = natureParam;
                    cmdClaims.Parameters.Add("@AdmissionTypeID", System.Data.SqlDbType.TinyInt).Value = admitParam;
                    cmdClaims.Parameters.AddWithValue("@ClaimID", claimIdLong);
                    rowsAffected += cmdClaims.ExecuteNonQuery();
                    TariffLog("[ClaimAI] Claims updated - NatureofTreatmentType_P43="
                        + (natureParam == DBNull.Value ? "unchanged" : natureParam.ToString())
                        + " AdmissionTypeID="
                        + (admitParam == DBNull.Value ? "unchanged" : admitParam.ToString()));
```

---

## Step 3 - the staged auto-save

`AutoPerformFullSaveForStaging` runs session-less with no DOM, so it cannot read the
dropdowns. It passes the ids directly instead.

### 3a. Two config-backed constants

Paste next to `AutoPerformFullSaveForStaging`:

```csharp
        /// <summary>
        /// Nature of Treatment / Type of Nature of Treatment for staged auto-saves.
        /// 196 = Allopathy - the value Index.cshtml itself hard-codes when it rebuilds
        /// the Type list, and the value Fill_OPDHospitalizationDetails tests for.
        /// Override with AppSettings["ClaimAINatureOfTreatmentTypeID"] if it ever changes.
        /// </summary>
        private static string ClaimAiNatureOfTreatmentTypeId()
        {
            var v = System.Configuration.ConfigurationManager.AppSettings["ClaimAINatureOfTreatmentTypeID"];
            return string.IsNullOrWhiteSpace(v) ? "196" : v.Trim();
        }

        /// <summary>
        /// Admission Type for staged auto-saves - "Planned" for every disease, per spec.
        /// 1 = Planned, 2 = Emergency (read off #ddlAdmissionType). Claims.AdmissionTypeID
        /// is tinyint, so 1 is well inside range. Override with
        /// AppSettings["ClaimAIAdmissionTypeID"] - set it to "" to stop writing the column.
        /// </summary>
        private static string ClaimAiAdmissionTypeId()
        {
            var v = System.Configuration.ConfigurationManager.AppSettings["ClaimAIAdmissionTypeID"];
            return v == null ? "1" : v.Trim();   // null = key absent -> default; "" = deliberately off
        }
```

### 3b. Pass them on the staged hospitalization save

FIND:

```csharp
                    SaveHospitalizationDetailsForClaimAI(
                        claimId.ToString(), slNo.ToString(), resolvedFacilityId, /*billNo*/ "AI");
                    TariffLog("[AutoApproval][Save] hospitalization saved claim=" + claimId);
```

REPLACE:

```csharp
                    // Nature of Treatment = Allopathy, Admission Type = Planned for every
                    // staged claim. No dropdowns exist on this path, so the ids come from
                    // config; an unset Admission Type id leaves that column untouched.
                    string stgNature = ClaimAiNatureOfTreatmentTypeId();
                    string stgAdmit  = ClaimAiAdmissionTypeId();
                    if (string.IsNullOrWhiteSpace(stgAdmit))
                        TariffLog("[AutoApproval][Save] AppSettings ClaimAIAdmissionTypeID not set - "
                            + "Admission Type NOT saved for claim=" + claimId);
                    SaveHospitalizationDetailsForClaimAI(
                        claimId.ToString(), slNo.ToString(), resolvedFacilityId, /*billNo*/ "AI",
                        stgNature, stgAdmit);
                    TariffLog("[AutoApproval][Save] hospitalization saved claim=" + claimId
                        + " nature=" + stgNature + " admissionType=" + (stgAdmit == "" ? "(unset)" : stgAdmit));
```

### 3c. Web.config

```xml
    <add key="ClaimAINatureOfTreatmentTypeID" value="196" />
    <add key="ClaimAIAdmissionTypeID" value="1" />   <!-- 1 = Planned, 2 = Emergency -->
```

---

## The ids, confirmed

| Field | Column (on `Claims`) | Value written |
|---|---|---|
| Nature of Treatment + Type of Nature of Treatment | `NatureofTreatmentType_P43` (int) | `196` = Allopathy |
| Admission Type | `AdmissionTypeID` (tinyint) | `1` = Planned |

`#ddlAdmissionType` holds `1 = Planned`, `2 = Emergency`. Both ids are in Web.config,
so switching a staged claim to Emergency later is a config change, not a rebuild.

Nature and Type of Nature are two dropdowns but one column: it stores the CHILD value
(196 for Allopathy; 197-200 are the AYUSH types, whose parent is 267), so Allopathy is
a single write.

---

## Verify

Save from the AI Summary and check the Spectra console:

```
[ClaimAI] NatureOfTreatmentType=196 AdmissionType=<id>
```

Then `App_Data/Logs/TariffSelection_<date>.log`:

```
[ClaimAI] Claims updated - NatureofTreatmentType_P43=196 AdmissionTypeID=<id>
```

Reload the claim: Processing Tabs > Hospitalization Details should show Nature of
Treatment = Allopathy and Type of Nature of Treatment = Allopathy, and Clinical
Details should show Admission Type = Planned.

If a console value comes back `null`, that dropdown's option text is not exactly
"Allopathy" / "Planned" - send me the option list and I will match the real text.
If the log says the id is outside tinyint range, `Mst_PropertyValues` has an id above
255 for that option and the column type needs a look.
