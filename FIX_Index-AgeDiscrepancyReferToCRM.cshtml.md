# Add — Age discrepancy (> 5 yrs) → Refer To CRM ("CRM Clarification")  (Index.cshtml)

**File:** `Enrollment/Views/MedicalScrutiny/Index.cshtml`

## Goal
When the patient age on the claim document differs from the Spectra/policy age by **more than 5 years**, the post-save auto-sequence should open **Refer To CRM** (exactly like the tariff-missing flow), but with:
- **Reason:** `CRM Clarification` (selected from the `#ddlReferToCRM` dropdown)
- **Remarks:** `Kindly clarify age discrepancy. <claim age> as per Hospital records and <policy age> as per Policy.`

**Priority:** if BOTH tariff-missing/tariff-item-missing AND age discrepancy apply, **tariff wins** — the tariff Refer-To-CRM (with its own details) is used, not the age one.

The ClaimAI side (`result-view.tsx`) now sends in `claimAISaveComplete`:
```js
ageDiscrepancy: { claimAge: <AI age>, policyAge: <Spectra age> }   // only when |diff| > 5, else null
```

There are **4 edits**, all near the existing tariff-refer code.

---

## Edit 1 — set the age-refer flag in the `claimAISaveComplete` handler

Right after the existing no-tariff block (~line 11300).

### BEFORE
```javascript
                    (function () {
                        var _tariffOk = event.data.tariffAvailable === true;
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
                        var _tariffOk = event.data.tariffAvailable === true;
                        if (!_tariffOk) {
                            sessionStorage.setItem('claimAI_noTariffRefer', '1');
                            sessionStorage.setItem('claimAI_noTariffClaimId', $('#hdnClaimID').val());
                            console.log('[ClaimAI] Tariff missing -> Refer To CRM (PMT) after reload');
                        }
                        // Age discrepancy (> 5 yrs) -> Refer To CRM ("CRM Clarification").
                        // Stored regardless of tariff; the auto-sequence gives tariff priority.
                        var _age = event.data.ageDiscrepancy;
                        if (_age && _age.claimAge != null && _age.policyAge != null) {
                            sessionStorage.setItem('claimAI_ageRefer', '1');
                            sessionStorage.setItem('claimAI_ageReferClaimId', $('#hdnClaimID').val());
                            sessionStorage.setItem('claimAI_ageReferClaimAge', String(_age.claimAge));
                            sessionStorage.setItem('claimAI_ageReferPolicyAge', String(_age.policyAge));
                            console.log('[ClaimAI] Age discrepancy (' + _age.claimAge + ' vs ' + _age.policyAge + ') -> Refer To CRM after reload');
                        }
                    })();
```

---

## Edit 2 — add the `referToCrmAgeDiscrepancy` function (next to `referToCrmNoTariff`)

Paste this immediately AFTER the closing `}` of `function referToCrmNoTariff() { … }` (~line 8663).

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
                                    // Match on the words if option text is decorated.
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

## Edit 4 — use it in the Claim Actions decision (tariff FIRST, then age)

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
                                // Tariff missing/item-not-found takes PRIORITY over age discrepancy.
                                referToCrmNoTariff();
                                return;
                            }
                            if (_ageRefer) {
                                // No tariff issue, but the age differs by > 5 yrs -> CRM Clarification.
                                referToCrmAgeDiscrepancy(_ageRefer.claimAge, _ageRefer.policyAge);
                                return;
                            }
```

---

## Notes
- **Priority is guaranteed:** the auto-sequence checks `_noTariffRefer` before `_ageRefer`, so when both apply, the tariff Refer-To-CRM (with tariff remarks) wins and the age path is skipped entirely.
- Reason lookup uses `setSelectByText($reason, 'CRM Clarification')` with a regex fallback — adjust the text if your dropdown option is worded differently (e.g. "CRM - Clarification").
- Remarks read exactly: `Kindly clarify age discrepancy. <claimAge> as per Hospital records and <policyAge> as per Policy.`
- This is a **Spectra** change (rebuild + redeploy) paired with the updated ClaimAI `result-view.tsx` (which sends `ageDiscrepancy`). Reprocessing isn't required — the age comes from data already extracted; just redeploy both.
