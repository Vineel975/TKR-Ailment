# Fix — ClaimsCommonUtils.js

**File:** `Enrollment/Scripts/jss/ClaimsCommonUtils.js`
**Fixes issue 1:** the *"There could be impact on the bill related amounts as the estimation days is changed…"* pop‑up that appears when you press **Save** in the iframe and delays the auto‑refresh.

## Why this happens
This is an informational `alert()` in the hospitalization‑save validation. It fires whenever estimated days differ from the saved value **and** a bill amount is present. In the ClaimAI flow the AI save re‑saves the bill details itself, so this manual cross‑check pop‑up only interrupts the save / auto‑refresh — it never blocks anything meaningful. This matches the pattern already used elsewhere in this file (e.g. *"ClaimAI: Approved accommodation validation removed — handled by AI save flow"*).

## BEFORE (around line 1569)

```javascript
        var oldEstimatedDays = $("#hdnEstimatedDays").val();

        if (parseInt($("#hdnEstimatedDays").val()) != parseInt($('#txtExtimatedDays').val())) {
            if ($('#txtTotalServicesBillAmount').val() != '') {
                alert('There could be impact on the bill related amounts as the estimation days is changed. Please cross check and save the bill details.');
            }
        }
```

## AFTER

```javascript
        var oldEstimatedDays = $("#hdnEstimatedDays").val();

        // ClaimAI: estimation-days impact alert suppressed — the AI save flow re-saves the
        // bill details automatically, so this manual cross-check pop-up must not interrupt
        // the iframe save / auto-refresh sequence.
        //if (parseInt($("#hdnEstimatedDays").val()) != parseInt($('#txtExtimatedDays').val())) {
        //    if ($('#txtTotalServicesBillAmount').val() != '') {
        //        alert('There could be impact on the bill related amounts as the estimation days is changed. Please cross check and save the bill details.');
        //    }
        //}
```

## Notes
- This alert does **not** set `flag = false` — it never blocked the save; it only forced an OK click, which is what delayed your auto‑refresh. Removing it is safe.
- If you ever want it back for **manual** claims only, wrap it in a guard instead of commenting it out (e.g. only alert when the ClaimAI auto‑sequence is not driving).
