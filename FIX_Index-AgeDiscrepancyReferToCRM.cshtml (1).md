# Add — Age discrepancy (> 5 yrs) → Refer To CRM ("CRM Clarification"), with Query > tariff > age priority  (Index.cshtml)

**File:** `Enrollment/Views/MedicalScrutiny/Index.cshtml`

## Goal
When the patient age on the claim document differs from the Spectra/policy age by **more than 5 years**, the post-save flow should open **Refer To CRM** (like the tariff-missing flow), with:
- **Reason:** `CRM Clarification` (from the `#ddlReferToCRM` dropdown)
- **Remarks:** `Kindly clarify age discrepancy. <claim age> as per Hospital records and <policy age> as per Policy.`

**Priority (only ONE window opens):** `Query Process (missing docs) → (else) tariff CRM → (else) age CRM`.
Enforced at **flag-set time** in the `claimAISaveComplete` handler, so at most one CRM-refer flag is ever set — no dependence on block timing.

The ClaimAI side (`result-view.tsx`) sends in `claimAISaveComplete`:
```js
ageDiscrepancy: { claimAge: <AI age>, policyAge: <Spectra age> }   // only when |diff| > 5, else null
```

There are **4 edits**.

---

## Edit 1 — REPLACE the existing no-tariff block with a single priority chain

The Query (Refer-to-Insurer) block above it — `if (event.data.referToInsurer === true) { … claimAI_autoRefer … }` — stays **unchanged**. Only the no-tariff block below it changes.

### BEFORE (~line 11296)
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

### AFTER
```javascript
                    (function () {
                        // Post-save CRM refer — PRIORITY: Query (missing docs) > tariff CRM > age CRM.
                        // The Query flow is handled by the Refer-to-Insurer block above (it sets
                        // claimAI_autoRefer). Here we set AT MOST ONE CRM-refer flag, and ONLY when
                        // there is no query — so exactly one post-save window opens.
                        var _isQuery = event.data.referToInsurer === true;
                        var _pattern = (event.data.coveragePattern || '').toString();
                        var _tariffOk = event.data.tariffAvailable === true;
                        var _tariffRefer = (_pattern === 'no_limit' && !_tariffOk);
                        var _age = event.data.ageDiscrepancy;

                        if (_isQuery) {
                            // Query Process takes priority — do not set any CRM-refer flag.
                        } else if (_tariffRefer) {
                            sessionStorage.setItem('claimAI_noTariffRefer', '1');
                            sessionStorage.setItem('claimAI_noTariffClaimId', $('#hdnClaimID').val());
                            console.log('[ClaimAI] No-limit policy with no tariff -> Refer To CRM (PMT) after reload');
                        } else if (_age && _age.claimAge != null && _age.policyAge != null) {
                            sessionStorage.setItem('claimAI_ageRefer', '1');
                            sessionStorage.setItem('claimAI_ageReferClaimId', $('#hdnClaimID').val());
                            sessionStorage.setItem('claimAI_ageReferClaimAge', String(_age.claimAge));
                            sessionStorage.setItem('claimAI_ageReferPolicyAge', String(_age.policyAge));
                            console.log('[ClaimAI] Age discrepancy (' + _age.claimAge + ' vs ' + _age.policyAge + ') -> Refer To CRM after reload');
                        }
                    })();
```

Set-time result: **Query** → only `claimAI_autoRefer`. **Tariff** (no query) → only `claimAI_noTariffRefer`. **Age** (no query, no tariff issue) → only `claimAI_ageRefer`. Never two at once.

---

## Edit 2 — add the `referToCrmAgeDiscrepancy` function (next to `referToCrmNoTariff`)

Paste immediately AFTER the closing `}` of `function referToCrmNoTariff() { … }` (~line 8663).

```javascript
                    function referToCrmAgeDiscrepancy(claimAge, policyAge) {
                        function finish() {
                            $('#claimAI_autoSeqOverlay').fadeOut(250, function () { $(this).remove(); });
                        }
                        try {
                            if (typeof GetReferToCRM === 'function') {
                                GetReferToCRM($('#hdnClaimID').val(), $('#hdnClaimSlNo').val());
                            }
                        } catch (e) { console.warn('[ClaimAI] GetReferToCRM failed', e); }
                        var _link = document.getElementById('lnkReferCRM');
                        if (_link) { try { _link.click(); } catch (e) { } }

                        var tries = 0;
                        (function fillWhenReady() {
                            var $reason = $('#ddlReferToCRM');
                            var $remarks = $('#taReferto_CRMRemarks');
                            if ($reason.length && $reason.find('option').length > 1 && $remarks.length) {
                                var picked = setSelectByText($reason, 'CRM Clarification');
                                if (picked === null) {
                                    $reason.find('option').each(function () {
                                        if (/crm\s*clarification/i.test($.trim($(this).text()))) {
                                            $reason.val($(this).val()).trigger('change');
                                            picked = $(this).val();
                                            return false;
                                        }
                                    });
                                }
                                $remarks.val('Kindly clarify age discrepancy. ' + claimAge + ' as per Hospital records and ' + policyAge + ' as per Policy.').trigger('change');
                                console.log('[ClaimAI] Refer To CRM pre-filled for age discrepancy (reason=' + picked + '). Doctor to click Submit.');
                                finish();
                                return;
                            }
                            if (tries++ < 40) { setTimeout(fillWhenReady, 300); return; }
                            console.warn('[ClaimAI] Refer To CRM popup not ready - skipping auto-fill.');
                            finish();
                        })();
                    }
```

---

## Edit 3 — read the age-refer flag in the auto-sequence (next to `_noTariffRefer`)

### BEFORE (~line 8669)
```javascript
                        var _noTariffRefer = (function () {
                            try {
                                var f = sessionStorage.getItem('claimAI_noTariffRefer');
                                var c = sessionStorage.getItem('claimAI_noTariffClaimId');
                                sessionStorage.removeItem('claimAI_noTariffRefer');
                                sessionStorage.removeItem('claimAI_noTariffClaimId');
                                return f === '1' && c === ($('#hdnClaimID').val() || '').toString();
                            } catch (e) { return false; }
                        })();
```

### AFTER
```javascript
                        var _noTariffRefer = (function () {
                            try {
                                var f = sessionStorage.getItem('claimAI_noTariffRefer');
                                var c = sessionStorage.getItem('claimAI_noTariffClaimId');
                                sessionStorage.removeItem('claimAI_noTariffRefer');
                                sessionStorage.removeItem('claimAI_noTariffClaimId');
                                return f === '1' && c === ($('#hdnClaimID').val() || '').toString();
                            } catch (e) { return false; }
                        })();
                        var _ageRefer = (function () {
                            try {
                                var f = sessionStorage.getItem('claimAI_ageRefer');
                                var c = sessionStorage.getItem('claimAI_ageReferClaimId');
                                var ca = sessionStorage.getItem('claimAI_ageReferClaimAge');
                                var pa = sessionStorage.getItem('claimAI_ageReferPolicyAge');
                                sessionStorage.removeItem('claimAI_ageRefer');
                                sessionStorage.removeItem('claimAI_ageReferClaimId');
                                sessionStorage.removeItem('claimAI_ageReferClaimAge');
                                sessionStorage.removeItem('claimAI_ageReferPolicyAge');
                                if (f === '1' && c === ($('#hdnClaimID').val() || '').toString()) {
                                    return { claimAge: ca, policyAge: pa };
                                }
                                return null;
                            } catch (e) { return null; }
                        })();
```

---

## Edit 4 — use it in the Claim Actions decision (tariff first, then age)

Because Edit 1 guarantees only one flag is ever set, this simple chain is sufficient — no query check needed here (when a query is pending, neither flag is set; this falls through to the normal path while the separate Query Process block opens the query window).

### BEFORE (~line 8681)
```javascript
                            if (_noTariffRefer) {
                                // Replaces Adjudication -> Calculate entirely.
                                referToCrmNoTariff();
                                return;
                            }
```

### AFTER
```javascript
                            if (_noTariffRefer) {
                                // Tariff missing/item-not-found — Refer To CRM (PMT).
                                referToCrmNoTariff();
                                return;
                            }
                            if (_ageRefer) {
                                // Age differs by > 5 yrs — Refer To CRM ("CRM Clarification").
                                referToCrmAgeDiscrepancy(_ageRefer.claimAge, _ageRefer.policyAge);
                                return;
                            }
```

---

## Priority — how it resolves
| Case | Flag set (Edit 1) | Window that opens |
|---|---|---|
| Query only | `autoRefer` | Query Process |
| Query + tariff | `autoRefer` | Query Process |
| Query + age | `autoRefer` | Query Process |
| Query + tariff + age | `autoRefer` | Query Process |
| Tariff only | `noTariffRefer` | Refer To CRM (PMT) |
| Tariff + age | `noTariffRefer` | Refer To CRM (PMT) |
| Age only | `ageRefer` | Refer To CRM (CRM Clarification) |

So: **Query → (else) tariff CRM → (else) age CRM** — exactly one window, every time.

## Notes / deploy
- Reason lookup uses `setSelectByText($reason, 'CRM Clarification')` with a regex fallback — adjust the text if your dropdown option is worded differently.
- Remarks read exactly: `Kindly clarify age discrepancy. <claimAge> as per Hospital records and <policyAge> as per Policy.`
- **Spectra** change (rebuild + redeploy) paired with the updated ClaimAI `result-view.tsx` (which sends `ageDiscrepancy`). No reprocessing needed.
