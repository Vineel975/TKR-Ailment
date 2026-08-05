# Fix — Tariff missing → Refer to CRM, decoupled from claim classification  (Index.cshtml)

**File:** `Enrollment/Views/MedicalScrutiny/Index.cshtml`
**In:** the `claimAISaveComplete` handler (~line 11296)

**What changes:** today the auto "Refer To CRM" only fires when the coverage pattern is `no_limit` **and** the tariff is missing. Per your update — tariff missing now **outranks** everything and is decoupled from claim classification — this makes it fire whenever the tariff is missing, regardless of coverage pattern.

## BEFORE (around line 11296)

```javascript
                    (function () {
                        var _pattern = (event.data.coveragePattern || '').toString();
                        var _tariffOk = event.data.tariffAvailable === true;
                        if (_pattern === 'no_limit' && !_tariffOk) {
                            sessionStorage.setItem('claimAI_noTariffRefer', '1');
                            sessionStorage.setItem('claimAI_noTariffClaimId', $('#hdnClaimID').val());
                            console.log('[ClaimAI] No-limit policy with no tariff -> Refer To CRM (PMT) after reload');
                        }
                    })();
```

## AFTER

```javascript
                    (function () {
                        var _tariffOk = event.data.tariffAvailable === true;
                        // ClaimAI: tariff missing -> Refer To CRM (PMT), regardless of coverage
                        // pattern / claim classification. Tariff is the top-priority route.
                        if (!_tariffOk) {
                            sessionStorage.setItem('claimAI_noTariffRefer', '1');
                            sessionStorage.setItem('claimAI_noTariffClaimId', $('#hdnClaimID').val());
                            console.log('[ClaimAI] Tariff missing -> Refer To CRM (PMT) after reload');
                        }
                    })();
```

## What this covers vs. what it doesn't
- ✅ **Tariff missing → Refer to CRM**, decoupled from claim classification, top priority. Done here.
- ⚠️ **Heads-up:** this now fires for *every* coverage pattern (capped policies too), not just `no_limit`. That matches "whenever tariff is missing → CRM" — just confirming it's intended, since a capped policy previously fell back to its cap instead of a CRM referral.
- ❗ **A-scan missing → Raise Query** and **lens-type missing → Raise Query** are **not** in this Spectra file. This handler only ever routed tariff→CRM, missing-docs→Refer-to-Insurer, or Adjudication→Calculate — there's no auto "Raise Query" branch here, and the iframe doesn't send A-scan / lens-type missing status to Spectra. So those two routes still need to be handled where that logic actually lives (the ClaimAI side).

To do the lens-type / A-scan → Raise Query part, send me the current ClaimAI file that decides those (the `result-view.tsx` save/decision area, or wherever your query routing is), since the copy I have is from your July‑20 upload and predates this. Or, if you want it built in Spectra too, confirm the iframe can send `aScanMissing` and `lensTypeMissing` booleans in `claimAISaveComplete`, and I'll add the Raise‑Query branch + the auto-sequence step to open the query with the right rows.
