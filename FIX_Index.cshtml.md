# Fixes — Index.cshtml  (issues 1 & 2)

**File:** `Enrollment/Views/MedicalScrutiny/Index.cshtml`

These two edits *set* a flag that tells the shared validation functions "the ClaimAI flow is driving — don't block." The matching *reads* of those flags are in the other two files (`ClaimsCommonUtils.js` and `MedicalScrutiny.js`). **Apply all three files together** — this one alone does nothing.

> This supersedes my earlier FIX‑1 / FIX‑2 attempt. The earlier ones patched single validation lines; these fix the actual save‑and‑sequence flow.

---

## Edit A — Issue 1: don't block / pop‑up on the iframe save

When the doctor saves, the flow clicks the real **Save Hospitalization** button. We raise a flag right before that click so the save skips the manual‑entry validations (the data is already persisted by the `…ForClaimAI` actions).

### BEFORE (around line 11426)

```javascript
                        console.log('[ClaimAI] Clicking Save Hospitalization Details');
                        $('#btnHospDetailsSave').show().click().hide();
```

### AFTER

```javascript
                        console.log('[ClaimAI] Clicking Save Hospitalization Details');
                        window._claimAI_bypassSaveValidation = true;                 // ClaimAI: suppress manual-entry validations/pop-ups for this save
                        $('#btnHospDetailsSave').show().click().hide();
                        setTimeout(function () { window._claimAI_bypassSaveValidation = false; }, 5000);  // auto-clear so manual saves still validate
```

---

## Edit B — Issue 2: let the post‑refresh auto‑sequence open Claim Actions

After the reload the auto‑sequence runs. We flag that it's active so `Enable_Buttons` skips the accommodation/billing gates and the Claim Actions dropdown (Query Process / Refer to CRM / Calculate) actually opens.

Place the flag‑set **after** the sequence's own staleness/claim guards pass, right before the overlay is created — so it only turns on when the sequence is genuinely about to run.

### BEFORE (around line 8444)

```javascript
                    // Ignore a stale flag (>2 min old) or a different claim now loaded.
                    if (!seqAt || (Date.now() - seqAt) > 120000) return;
                    var curClaim = $('#hdnClaimID').val() || '';
                    if (seqClaim && curClaim && seqClaim !== curClaim) return;
```

### AFTER

```javascript
                    // Ignore a stale flag (>2 min old) or a different claim now loaded.
                    if (!seqAt || (Date.now() - seqAt) > 120000) return;
                    var curClaim = $('#hdnClaimID').val() || '';
                    if (seqClaim && curClaim && seqClaim !== curClaim) return;

                    window._claimAI_autoSeqActive = true;                                             // ClaimAI: sequence confirmed & starting — let Enable_Buttons pass
                    setTimeout(function () { window._claimAI_autoSeqActive = false; }, 90000);         // safety auto-clear (sequence has a 60s backstop)
```

---

### After applying
Apply the other two files, then rebuild. On save you should see no pop‑up and the normal auto‑refresh; after refresh, Claim Actions should open with the Query Process / Refer to CRM / Calculate screen, as before.
