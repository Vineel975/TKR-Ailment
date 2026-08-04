# Fixes — MedicalScrutiny.js

**File:** `Enrollment/Scripts/jss/MedicalScrutiny.js`
Two independent fixes in this file: **issue 2** (auto‑sequence blocked by *"Please select approved accommodation"*) and **issue 4** (*"Go to dashboard"* empty pop‑up, can't go back).

---

## Fix 2 — "Please select approved accommodation" blocks the auto‑sequence

**Symptom:** After the auto‑refresh, *"Finalizing claim…"* shows, but Claim Actions don't open — you get the warning *"Please select approved accommodation OR save hospitalization details again."*

**Why:** This validation lives in `Enable_Buttons(_stageID)`. It fires when `IsAprvFacilitychanged != 1`. In the ClaimAI flow the approved accommodation **is** set (`#ddlApprovedFacility`), but the server‑side `IsAprvFacilitychanged` flag isn't always `1` in that path, so the gate trips and the auto‑sequence stops. The fix skips the gate **only when an approved accommodation is actually selected** — so it still protects the genuine "nothing selected" case.

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
```

### AFTER

```javascript
        // ClaimAI: skip the "select approved accommodation" gate when an approved
        // accommodation is already selected. The AI save flow sets #ddlApprovedFacility and
        // re-saves hospitalization, but the server-side IsAprvFacilitychanged flag is not
        // always 1 in that path — so gate on the dropdown actually being filled instead.
        var _aprvFacSelected = $('#ddlApprovedFacility').val() != null && $('#ddlApprovedFacility').val() != '' && $('#ddlApprovedFacility').val() != '0';
        if (!_aprvFacSelected && basicData[0].ServiceTypeID == 1 && basicData[0].IsAprvFacilitychanged != 1 && ($('#hdnClaimStageID').val() == 5 || $('#hdnClaimStageID').val() == 38) &&
            (basicData[0].RequestTypeID == 1 || basicData[0].RequestTypeID == 2 || basicData[0].RequestTypeID == 3)) {
            _valid = false;
            if (basicData[0].RequestTypeID == 1)
                _valmsgs.push('Please select approved accommodation OR save hospitalization details again');
            else
                _valmsgs.push('Please select approved accommodation');
        }
```

**Note:** two sibling validations sit right below this one in the same function — *"…impact on the bill related amounts as the accommodation change"* (`#hdnIsFacilityChanged == "true"`) and the billing‑correction gate (`BillingCorrection != 2`). Your AI save flow already sets `hdnIsFacilityChanged = false` and `BillingCorrection = 2`, so those should pass once the billing/coding save succeeds. If, after this fix, the sequence still stops on one of those two, tell me and I'll guard them the same way.

---

## Fix 4 — "Go to dashboard" empty pop‑up, can't go back

**Symptom:** Clicking **Back to Dashboard** shows an empty *"localhost says"* pop‑up and doesn't navigate.

**Why:** `backtodashboard(navurl)` calls `/MedicalScrutiny/claimProcessUnlockedbyUser` to release the claim lock, then navigates **only if** `result.Success` is true. In this build the unlock is returning `Success = false` with an empty `Message`, so you get an empty `alert()` and stay stuck. The unlock is best‑effort for navigation purposes, so we navigate regardless (and log the reason instead of blocking).

### BEFORE (inside `function backtodashboard(navurl)`, around line 23181)

```javascript
                success: function (result) {
                    console.log(result)
                    if (result.Success) {
                        window.location.href = navurl;
                    } else {
                        alert(result.Message);
                    }

                },
```

### AFTER

```javascript
                success: function (result) {
                    console.log(result)
                    if (result.Success) {
                        window.location.href = navurl;
                    } else {
                        // ClaimAI: unlock is best-effort for navigation. Do not trap the user
                        // on the page when it returns false / empty message — log and navigate.
                        if (result.Message) { console.warn('claimProcessUnlockedbyUser:', result.Message); }
                        window.location.href = navurl;
                    }

                },
```

### Important caveat on Fix 4
This makes the button navigate, but it treats the failed unlock as non‑fatal. If the claim lock **genuinely matters** (Redis `MvcApplication.MemoryDB` claim‑lock), navigating away without a successful unlock could leave the claim locked for the next open. The empty `Message` strongly suggests the unlock endpoint itself is misbehaving in this merged build — the proper root‑cause fix is on the server side (`claimProcessUnlockedbyUser` in `MedicalScrutinyController`), which we touched during the merge. This JS change unblocks you now; if you want, I can look at that controller method next so the unlock actually succeeds rather than being bypassed.
