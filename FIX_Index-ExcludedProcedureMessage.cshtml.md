# Add — Excluded-procedure message shown the same compact way as "unsupported claim"  (Index.cshtml)

**File:** `Enrollment/Views/MedicalScrutiny/Index.cshtml`
**In:** the ClaimAI `window.addEventListener('message', …)` handler (~line 10823)

## Goal
When ClaimAI detects an excluded procedure (Phakic / Myopia / Squint / Lasik / Contact Lens / IPCL), show the message in the **exact same compact layout** as the existing "AI Summary is currently available only for Cataract, Maternity and TKR claims…" message — by reusing the existing `showAiError(...)` function. `showAiError` hides the iframe (`#ifrClaimAI`) and shows the message box (`#divAiError`), so there is **no white space** below.

The ClaimAI side (`result-view.tsx`) now posts:
```js
{ source: "claimai", type: "excludedProcedure", message: "This claim involves <X>, which is excluded from the AI engine. Please process this claim manually." }
```

## Edit — handle the message BEFORE the read-only guard

It must run in **all** stages (it's informational, not a write), so place it right after the `source === 'claimai'` check and **before** the `_claimAI_isAdjudicationStage()` read-only guard.

### BEFORE (around line 10823)

```javascript
            window.addEventListener('message', function (event) {
                if (!event.data || event.data.source !== 'claimai') return;
                // Read-only stages (ClaimStageID !== 5): ignore ALL writes from the ClaimAI
                // iframe. The iframe is already read-only; this is the authoritative parent
                // guard so nothing can mutate the form or trigger a save outside Adjudication.
                if (!_claimAI_isAdjudicationStage()) {
```

### AFTER

```javascript
            window.addEventListener('message', function (event) {
                if (!event.data || event.data.source !== 'claimai') return;

                // Excluded procedure (Phakic / Myopia / Squint / Lasik / Contact Lens / IPCL)
                // detected by the AI — show it the SAME compact way as an unsupported claim
                // type. Informational only, so handle it in ALL stages (before the read-only
                // guard below). showAiError hides the iframe and shows #divAiError, so there
                // is no white space.
                if (event.data.type === 'excludedProcedure') {
                    showAiError(event.data.message || 'This claim is excluded from the AI engine. Please process it manually.');
                    return;
                }

                // Read-only stages (ClaimStageID !== 5): ignore ALL writes from the ClaimAI
                // iframe. The iframe is already read-only; this is the authoritative parent
                // guard so nothing can mutate the form or trigger a save outside Adjudication.
                if (!_claimAI_isAdjudicationStage()) {
```

## Result
- The excluded-procedure message now renders in the **same** `#divAiError` box (cream background, warning triangle, no iframe) as the "not supported" message — identical layout, no white space.
- Reuses `showAiError` verbatim, so if you ever restyle that box, both messages stay consistent.
- Works in every claim stage (it's placed before the read-only guard).

## Notes / deploy
- This is a **Spectra** change (rebuild + redeploy Spectra). Pair it with the updated ClaimAI `result-view.tsx`, which posts the `excludedProcedure` message.
- The ClaimAI iframe still renders its own compact message internally as a **fallback** — but once this Spectra handler is in place, `showAiError` hides the iframe, so only the Spectra message box shows.
- The claim is processed by the AI first (that's how the excluded procedure is detected), so the user may briefly see the iframe before it collapses to the message — same as any post-processing state.
