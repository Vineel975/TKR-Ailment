# Add — Claim-open / return-to-dashboard tracking (real review time)  (Index.cshtml)

Your metrics already log `SAVE_CLICK` and `FIELD_CHANGE`. To get a **real review duration** (Opened → Returned) instead of a guess, log two more events into the same `ClaimAI_EventLog`. No controller change is needed — `LogClaimAIEvent` already accepts any `eventType` and only increments the save count for `SAVE_CLICK`.

The Claims Activity API reads these:
- **`CLAIM_OPENED`** → the claim's *Opened At*.
- **`RETURN_TO_DASHBOARD`** → the claim's *Returned At*. (If absent, the API falls back to the **last `SAVE_CLICK`**, so review time still works — this event just makes it exact.)

## Edit 1 — a reusable logger (place once, near your other ClaimAI helpers)

```javascript
// Fire-and-forget ClaimAI metric logger (reuses /MedicalScrutiny/LogClaimAIEvent).
function _claimAI_logMetric(eventType, fieldName, aiVal, userVal) {
    try {
        $.ajax({
            url: '/MedicalScrutiny/LogClaimAIEvent',
            type: 'POST',
            data: {
                claimId:   $('#hdnClaimID').val() || '',
                slNo:      $('#hdnClaimSlNo').val() || '1',
                eventType: eventType,
                fieldName: fieldName || null,
                aiValue:   aiVal || '',
                userValue: userVal || '',
                claimType: window._claimAI_claimType || 'other'
            },
            error: function () { /* non-critical — silent */ }
        });
    } catch (e) { /* silent */ }
}
```

## Edit 2 — log `CLAIM_OPENED` when the AI view loads (~line 8417)

### BEFORE
```javascript
            $('#ifrClaimAI').on('load', function() {
                setTimeout(claimAI_sendFacilityOptions, 800);
            });
```

### AFTER
```javascript
            $('#ifrClaimAI').on('load', function() {
                setTimeout(claimAI_sendFacilityOptions, 800);
                // METRICS: record when the reviewer opened this claim's AI view — once
                // per claim/session (the iframe can reload after save).
                (function () {
                    var _cid = ($('#hdnClaimID').val() || '') + ':' + ($('#hdnClaimSlNo').val() || '1');
                    var _k = 'claimAI_openedLogged_' + _cid;
                    if (_cid !== ':1' && !sessionStorage.getItem(_k)) {
                        sessionStorage.setItem(_k, '1');
                        _claimAI_logMetric('CLAIM_OPENED');
                    }
                })();
            });
```

## Edit 3 — log `RETURN_TO_DASHBOARD` when the reviewer leaves for the dashboard

Pick whichever fits your UI:

**A. You have a "Go to Dashboard" / back button** — hook its click:
```javascript
$(document).on('click', '#btnGoToDashboard, .js-go-to-dashboard', function () {
    _claimAI_logMetric('RETURN_TO_DASHBOARD');
});
```
(Replace the selector with your actual dashboard button/link id or class.)

**B. Robust fallback — fire on page leave** (uses `sendBeacon` so it survives navigation):
```javascript
window.addEventListener('pagehide', function () {
    try {
        var params = new URLSearchParams({
            claimId:   $('#hdnClaimID').val() || '',
            slNo:      $('#hdnClaimSlNo').val() || '1',
            eventType: 'RETURN_TO_DASHBOARD',
            claimType: window._claimAI_claimType || 'other'
        });
        navigator.sendBeacon('/MedicalScrutiny/LogClaimAIEvent', params);
    } catch (e) { /* silent */ }
});
```

If you skip Edit 3 entirely, the API uses the last `SAVE_CLICK` as the return time — review duration is still populated, just measured to the final save rather than the exact dashboard click.

## Notes
- Spectra change only (rebuild/redeploy). No controller change.
- `window._claimAI_claimType` is the same variable your existing metrics logging uses; keep it consistent.
- The two new event types flow straight into `ClaimAI_EventLog` and are picked up by `GET /api/claims-activity`.
