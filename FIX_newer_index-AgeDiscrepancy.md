# Age discrepancy (> 5 yrs) → Refer To CRM — changes for `newer_index.cshtml`

Tailored to **your** uploaded file. The already-applied file is **`newer_index_UPDATED.cshtml`** — you can use that directly, or apply the 4 edits below to your own copy. Line numbers are from your original `newer_index.cshtml`.

Pairs with the ClaimAI `result-view.tsx` that sends `ageDiscrepancy: { claimAge, policyAge }` in `claimAISaveComplete`.

**Priority (only one window opens):** Query (missing docs) → tariff CRM → age CRM — enforced at flag-set time (Edit 1).

> Note: your file's tariff-refer condition is the decoupled `if (!_tariffOk)` (no `no_limit` gate), so the edits below match that.

---

## Edit 1 — the no-tariff block → priority chain  (your lines ~11310-11318)

### BEFORE
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

### AFTER
```javascript
                    (function () {
                        // Post-save CRM refer - PRIORITY: Query (missing docs) > tariff CRM > age CRM.
                        // The Query flow is handled by the Refer-to-Insurer block above (it sets
                        // claimAI_autoRefer). Here we set AT MOST ONE CRM-refer flag, and ONLY when
                        // there is no query - so exactly one post-save window opens.
                        var _isQuery = event.data.referToInsurer === true;
                        var _tariffOk = event.data.tariffAvailable === true;
                        var _tariffRefer = !_tariffOk;
                        var _age = event.data.ageDiscrepancy;

                        if (_isQuery) {
                            // Query Process takes priority - do not set any CRM-refer flag.
                        } else if (_tariffRefer) {
                            sessionStorage.setItem('claimAI_noTariffRefer', '1');
                            sessionStorage.setItem('claimAI_noTariffClaimId', $('#hdnClaimID').val());
                            console.log('[ClaimAI] Tariff missing -> Refer To CRM (PMT) after reload');
                        } else if (_age && _age.claimAge != null && _age.policyAge != null) {
                            sessionStorage.setItem('claimAI_ageRefer', '1');
                            sessionStorage.setItem('claimAI_ageReferClaimId', $('#hdnClaimID').val());
                            sessionStorage.setItem('claimAI_ageReferClaimAge', String(_age.claimAge));
                            sessionStorage.setItem('claimAI_ageReferPolicyAge', String(_age.policyAge));
                            console.log('[ClaimAI] Age discrepancy (' + _age.claimAge + ' vs ' + _age.policyAge + ') -> Refer To CRM after reload');
                        }
                    })();
```

---

## Edit 2 — add `referToCrmAgeDiscrepancy` after `referToCrmNoTariff`  (your line ~8667)

Insert this new function between the closing `}` of `referToCrmNoTariff()` and the `// Let the reloaded page finish its own initialisation first.` comment.

### BEFORE
```javascript
                        })();
                    }

                    // Let the reloaded page finish its own initialisation first.
```

### AFTER
```javascript
                        })();
                    }

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

                    // Let the reloaded page finish its own initialisation first.
```

---

## Edit 3 — read the age-refer flag in the auto-sequence  (after your `_noTariffRefer`, ~line 8680)

### BEFORE
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

## Edit 4 — Claim Actions decision, add the age branch  (your lines ~8684-8688)

### BEFORE
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
                                // Tariff missing/item-not-found - Refer To CRM (PMT).
                                referToCrmNoTariff();
                                return;
                            }
                            if (_ageRefer) {
                                // Age differs by > 5 yrs - Refer To CRM ("CRM Clarification").
                                referToCrmAgeDiscrepancy(_ageRefer.claimAge, _ageRefer.policyAge);
                                return;
                            }
```

---

## Priority resolution
| Case | Flag set (Edit 1) | Window |
|---|---|---|
| Query (± tariff ± age) | `autoRefer` | Query Process |
| Tariff (no query, ± age) | `noTariffRefer` | Refer To CRM (PMT) |
| Age only | `ageRefer` | Refer To CRM (CRM Clarification) |

## Notes
- Reason: `setSelectByText($reason, 'CRM Clarification')` (regex fallback `/crm\s*clarification/i`) — change the text if your dropdown option is worded differently.
- Remarks: `Kindly clarify age discrepancy. <claimAge> as per Hospital records and <policyAge> as per Policy.`
- Spectra rebuild/redeploy, paired with the ClaimAI `result-view.tsx` that sends `ageDiscrepancy`. No reprocessing.
- In the applied file (`newer_index_UPDATED.cshtml`), these land at: Edit 1 ~11370, Edit 2 ~8669, Edit 3 ~8719, Edit 4 ~8745.
