# Fix — Date of Discharge reading the wrong Spectra field  (newer_index.cshtml)

## Bug
In the patient-info validation, `spectraFields.dischargeDate` was read from `GetClaimFieldsForValidation`'s `fields.dischargeDate`, which returns the **wrong** value. Admission date is fine because it uses the reliable `basicData[0].dateofadmission`.

## Correct source
The Spectra form field **`#txtHospDOD`** holds the real discharge date — it is populated from `data[0].DateofDischarge` (see, in your file, `$("#txtHospDOD").val(JSONDate2(data[0].DateofDischarge));`), exactly parallel to `#txtHospDOA` for admission. So we read the discharge date from `#txtHospDOD` and stop using the wrong source.

Two small edits, both in the `spectraFields` collection block (~line 9180-9197 in your file).

---

## Edit A — read discharge from `#txtHospDOD` (right after the admission block)

### BEFORE
```javascript
            // Admission date — basicData[0].dateofadmission confirmed column name
            if (typeof basicData !== 'undefined' && basicData[0] && basicData[0].dateofadmission) {
                spectraFields.admissionDate = basicData[0].dateofadmission.toString().trim();
            }
```

### AFTER
```javascript
            // Admission date — basicData[0].dateofadmission confirmed column name
            if (typeof basicData !== 'undefined' && basicData[0] && basicData[0].dateofadmission) {
                spectraFields.admissionDate = basicData[0].dateofadmission.toString().trim();
            }

            // Discharge date — read from the Spectra form field #txtHospDOD, which is
            // populated from data[0].DateofDischarge (the correct Spectra value). The
            // GetClaimFieldsForValidation 'dischargeDate' source below was returning the
            // wrong value, so the correct one is set here.
            (function () {
                var _dod = getInputVal('txtHospDOD');
                if (_dod) spectraFields.dischargeDate = _dod.toString().trim();
            })();
```

---

## Edit B — stop using the wrong `fields.dischargeDate` (in the `GetClaimFieldsForValidation` block)

### BEFORE
```javascript
                    if (fields.dischargeDate && !spectraFields.dischargeDate)
                                             spectraFields.dischargeDate = fields.dischargeDate;
```

### AFTER
```javascript
                    // dischargeDate is sourced from #txtHospDOD above (data[0].DateofDischarge);
                    // fields.dischargeDate from GetClaimFieldsForValidation was wrong, so it is
                    // no longer used.
```

---

## Result
- `spectraFields.dischargeDate` now comes from `#txtHospDOD` = `data[0].DateofDischarge` — the same claim data that fills the form, so it matches what the adjudicator sees.
- The wrong `GetClaimFieldsForValidation.dischargeDate` value is no longer used.
- The discharge-date validation (AI claim-document date vs Spectra date) now compares against the correct Spectra value.
- Admission-date handling is untouched (it was already correct).

## Notes
- Spectra-only change (rebuild/redeploy). No ClaimAI change, no reprocessing.
- The applied file `newer_index_UPDATED.cshtml` contains this fix **and** the earlier age-discrepancy Refer-To-CRM changes.
- If your date validation is strict about format, note `#txtHospDOD` is in the display format (via `JSONDate2`); the ClaimAI validation normalises dates, and admission already uses a different raw format successfully — so a value now flows through correctly. If you ever see a format mismatch specifically, tell me and I'll normalise it to match `admissionDate`.
