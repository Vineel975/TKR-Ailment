# Fix — MedicalScrutiny.js  (issue 2)

**File:** `Enrollment/Scripts/jss/MedicalScrutiny.js`
**Function:** `Enable_Buttons(_stageID)`

This lets the post‑refresh auto‑sequence open Claim Actions (Query Process / Refer to CRM / Calculate). It reads the `window._claimAI_autoSeqActive` flag that `Index.cshtml` sets when the auto‑sequence starts, and skips the three accommodation/billing gates during that sequence.

> Apply together with `FIX_Index.cshtml.md`.
> **Issue 4 (Back to Dashboard) is already working — leave that edit as‑is; nothing to re‑apply there.**
> This replaces my earlier issue‑2 attempt (the `_aprvFacSelected` guard), which didn't work because the dropdown isn't populated at validation time.

---

## Edit — guard the three blocking validations with the auto‑sequence flag

### BEFORE (inside `function Enable_Buttons(_stageID)`, around line 11772)

```javascript
        if (basicData[0].ServiceTypeID == 1 && basicData[0].IsAprvFacilitychanged != 1 && ($('#hdnClaimStageID').val() == 5 || $('#hdnClaimStageID').val() == 38) &&
            (basicData[0].RequestTypeID == 1 || basicData[0].RequestTypeID == 2 || basicData[0].RequestTypeID == 3)) {
            _valid = false;
            if (basicData[0].RequestTypeID == 1)
                _valmsgs.push('Please select approved accommodation OR save hospitalization details again');
            else
                _valmsgs.push('Please select approved accommodation');
        }

        if ($("#hdnIsFacilityChanged").val() == "true") {
            _valid = false;
            _valmsgs.push('There could be an impact on the bill related amounts as the accommodation change. Please recheck and save the bill details.')
        }

        if (MakeZerofromUndefinedorEmpty(basicData[0].BillingCorrection) != 2 && basicData[0].ServiceTypeID != 2) {
            _valid = false;
            _valmsgs.push('The change in Billing details shall have an impact on total eligible amount of the claim. Request you to ensure the same eligible amount reflects in Coding section. Please modify Coding details.');
        }
```

### AFTER

```javascript
        // ClaimAI: during the post-save auto-sequence the accommodation/billing values are
        // already saved to the DB, but the in-memory flags aren't always set at this instant.
        // Skip these three gates while the auto-sequence is driving so Claim Actions can open.
        if (!window._claimAI_autoSeqActive && basicData[0].ServiceTypeID == 1 && basicData[0].IsAprvFacilitychanged != 1 && ($('#hdnClaimStageID').val() == 5 || $('#hdnClaimStageID').val() == 38) &&
            (basicData[0].RequestTypeID == 1 || basicData[0].RequestTypeID == 2 || basicData[0].RequestTypeID == 3)) {
            _valid = false;
            if (basicData[0].RequestTypeID == 1)
                _valmsgs.push('Please select approved accommodation OR save hospitalization details again');
            else
                _valmsgs.push('Please select approved accommodation');
        }

        if (!window._claimAI_autoSeqActive && $("#hdnIsFacilityChanged").val() == "true") {
            _valid = false;
            _valmsgs.push('There could be an impact on the bill related amounts as the accommodation change. Please recheck and save the bill details.')
        }

        if (!window._claimAI_autoSeqActive && MakeZerofromUndefinedorEmpty(basicData[0].BillingCorrection) != 2 && basicData[0].ServiceTypeID != 2) {
            _valid = false;
            _valmsgs.push('The change in Billing details shall have an impact on total eligible amount of the claim. Request you to ensure the same eligible amount reflects in Coding section. Please modify Coding details.');
        }
```

---

### Notes
- Manual claim processing is unaffected — `window._claimAI_autoSeqActive` is only `true` while the auto‑sequence runs (and auto‑clears after 90 s).
- If, after this, the sequence gets *past* accommodation but stops at Calculate on a different message, send me that message — it'll be one more gate in the same function to guard the same way.
