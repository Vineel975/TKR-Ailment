# Fix — DOA and DOD: read both from the Hospitalization Details fields  (Index.cshtml)

**File:** `Enrollment/Views/MedicalScrutiny/Index.cshtml`
**In:** the `spectraFields` collection block (~line 9180)

## Problem
The AI validation was comparing the extracted DOA/DOD against the **wrong Spectra source**:
- **Admission** came from `basicData[0].dateofadmission` — occasionally wrong.
- **Discharge** came from `GetClaimFieldsForValidation` (`fields.dischargeDate`) — wrong.

The correct values are the ones shown in the **Hospitalization Details** tab: `#txtHospDOA` and `#txtHospDOD` (populated from `data[0].DateofAdmission` / `data[0].DateofDischarge`). Both should be sourced from there.

## Edit 1 — Admission from `#txtHospDOA`

### BEFORE
```javascript
            // Admission date — basicData[0].dateofadmission confirmed column name
            if (typeof basicData !== 'undefined' && basicData[0] && basicData[0].dateofadmission) {
                spectraFields.admissionDate = basicData[0].dateofadmission.toString().trim();
            }
```

### AFTER
```javascript
            // Admission date — read ONLY from the Hospitalization Details field
            // #txtHospDOA (the value the doctor sees, populated from
            // data[0].DateofAdmission). No fallback: if the field is blank, leave
            // admissionDate blank rather than sourcing a possibly-wrong value.
            (function () {
                var _doa = getInputVal('txtHospDOA');
                if (_doa) spectraFields.admissionDate = _doa.toString().trim();
            })();
```

## Edit 2 — Discharge from `#txtHospDOD`

If your file still reads discharge from `GetClaimFieldsForValidation`, replace that with the field read (this was already applied in the updated file; shown here for completeness).

### BEFORE (if present)
```javascript
                    if (fields.dischargeDate && !spectraFields.dischargeDate)
                                             spectraFields.dischargeDate = fields.dischargeDate;
```

### AFTER — add this right after the admission block, and remove the line above
```javascript
            // Discharge date — read from the Hospitalization Details field #txtHospDOD
            // (the value the doctor sees, populated from data[0].DateofDischarge). This
            // is the authoritative DOD; the GetClaimFieldsForValidation 'dischargeDate'
            // source was returning the wrong value.
            (function () {
                var _dod = getInputVal('txtHospDOD');
                if (_dod) spectraFields.dischargeDate = _dod.toString().trim();
            })();
```

## Result
- Both `spectraFields.admissionDate` and `spectraFields.dischargeDate` now come from the **Hospitalization Details** fields the adjudicator sees (`#txtHospDOA` / `#txtHospDOD`).
- The AI-extracted DOA/DOD are compared against those exact values.
- **No fallback:** if `#txtHospDOA` is blank, `admissionDate` is left blank (nothing wrong is substituted); the same applies to discharge.

## Notes
- Spectra-only change (rebuild/redeploy). No ClaimAI change, no reprocessing.
- The applied file `newer_index_UPDATED.cshtml` already contains this (admission → `#txtHospDOA`, discharge → `#txtHospDOD`) plus the earlier age-discrepancy changes.
- These are display-format dates (via `JSONDate2`); the ClaimAI date validation normalises formats before comparing, so admission and discharge compare correctly.
