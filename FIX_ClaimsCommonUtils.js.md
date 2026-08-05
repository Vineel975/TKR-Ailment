# Fix — ClaimsCommonUtils.js  (issue 1)

**File:** `Enrollment/Scripts/jss/ClaimsCommonUtils.js`
**Function:** `Save_HospitalizationDetails(...)`

This makes the manual save (triggered by the ClaimAI flow) go through without the pop‑up and without being blocked by the manual‑entry validations. It reads the `window._claimAI_bypassSaveValidation` flag that `Index.cshtml` sets right before the save click.

> Apply together with `FIX_Index.cshtml.md`. Three small edits in this one function.

---

## Edit 1 — suppress the "estimation days changed" pop‑up in the ClaimAI flow

### BEFORE (around line 1571)

```javascript
        if (parseInt($("#hdnEstimatedDays").val()) != parseInt($('#txtExtimatedDays').val())) {
            if ($('#txtTotalServicesBillAmount').val() != '') {
                alert('There could be impact on the bill related amounts as the estimation days is changed. Please cross check and save the bill details.');
            }
        }
```

### AFTER

```javascript
        if (window._claimAI_bypassSaveValidation !== true && parseInt($("#hdnEstimatedDays").val()) != parseInt($('#txtExtimatedDays').val())) {
            if ($('#txtTotalServicesBillAmount').val() != '') {
                alert('There could be impact on the bill related amounts as the estimation days is changed. Please cross check and save the bill details.');
            }
        }
```

---

## Edit 2 — force the save through (skip the `flag = false` blocks) in the ClaimAI flow

The 16 manual‑entry validations set `flag = false`; the save only runs `if (flag == true)`. In the ClaimAI flow the data is already persisted and every field is populated, so we force `flag = true` right at the gate.

### BEFORE (line 1663)

```javascript
        if (flag == true) {
```

### AFTER

```javascript
        // ClaimAI: the AI save flow already persisted hospitalization/clinical/billing/coding via
        // the *ForClaimAI actions and populated every field — force the save through so the
        // manual-entry validations above don't block it (manual saves are unaffected).
        if (window._claimAI_bypassSaveValidation === true) { flag = true; $('#divErrorMessage').html(''); }
        if (flag == true) {
```

---

## Edit 3 — let the inner required‑field check pass in the ClaimAI flow

Inside the `if (flag == true)` block there's a second gate, `HospitalizationDetails_Validate(...)`, before the actual save AJAX. Let it pass in the ClaimAI flow too.

### BEFORE (line 1671)

```javascript
            if (HospitalizationDetails_Validate(_basicData[0].ServiceTypeID)) {
```

### AFTER

```javascript
            if (window._claimAI_bypassSaveValidation === true || HospitalizationDetails_Validate(_basicData[0].ServiceTypeID)) {
```

---

### Notes
- Manual (non‑ClaimAI) saves are unchanged — the flag is only `true` for ~5 seconds around the ClaimAI save click, and defaults to `false`/undefined otherwise.
- There are a few *hard* `return false` checks earlier in the function (dates of admission/discharge, discharge type). The ClaimAI flow populates those fields, so they should pass. If a save still silently doesn't go through after this, tell me the `[ClaimAI]` console output and I'll guard those too.
