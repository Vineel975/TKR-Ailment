# Spectra `Index.cshtml` — send DOA / DOD to the ClaimAI iframe

One function is replaced. Nothing else in the file changes.

---

## Why this is needed

`SubmitAndLoadIframe` collects the dates into `spectraFields` **once, at page load**:

```javascript
if (typeof basicData !== 'undefined' && basicData[0] && basicData[0].dateofadmission) {
    spectraFields.admissionDate = basicData[0].dateofadmission.toString().trim();
}
(function () {
    var _dod = getInputVal('txtHospDOD');
    if (_dod) spectraFields.dischargeDate = _dod.toString().trim();
})();
```

`#txtHospDOD` is filled by `Fill_HospitalizationDetails`, which runs **asynchronously**
when the Hospitalization Details pane loads. At the moment `SubmitAndLoadIframe`
reads it, that box is normally still empty — so `dischargeDate` never makes it into
the payload, and even when it does it is a one-shot snapshot that never updates.
Admission does not have this problem because it is read from `basicData`, which is
already in the page.

Accommodation avoids the timing issue entirely: it is pushed to the iframe by
`postMessage` **after** the pane is populated. This change puts the two dates on
that same channel.

---

## Step 1 — locate the function

In `Index.cshtml`, search for:

```
claimAI_sendFacilityOptions
```

The first hit is the function definition, inside the first `$(document).ready(...)`,
just below `GetInsurerRejectionMaster(...)`.

---

## Step 2 — FIND this exact block

```javascript
            // Send facilityOptions + availedId to ClaimAI iframe after it loads
            function claimAI_sendFacilityOptions() {
                var iframe = document.getElementById('ifrClaimAI');
                if (!iframe || !iframe.contentWindow) return;
                var options = [];
                $('#ddlApprovedFacility option').each(function() {
                    var v = $(this).val(), t = $(this).text().trim();
                    if (v && t && t !== 'Select') options.push({ id: v.toString(), text: t });
                });
                if (options.length > 0) {
                    iframe.contentWindow.postMessage(
                        { source: 'spectra', type: 'setFacilityOptions',
                          options: options,
                          availedId: ($('#ddlReceivedAccomodation').val() || '').toString() },
                        '*'
                    );
                    console.log('[ClaimAI] Sent facilityOptions:', options.length, 'options, availedId:', $('#ddlReceivedAccomodation').val());
                }
            }
```

## Step 3 — REPLACE it with

```javascript
            // Send facilityOptions + availedId to ClaimAI iframe after it loads
            function claimAI_sendFacilityOptions() {
                var iframe = document.getElementById('ifrClaimAI');
                if (!iframe || !iframe.contentWindow) return;
                _claimAI_lastHospDates = '';   // fresh iframe — allow the dates to be sent again
                var options = [];
                $('#ddlApprovedFacility option').each(function() {
                    var v = $(this).val(), t = $(this).text().trim();
                    if (v && t && t !== 'Select') options.push({ id: v.toString(), text: t });
                });
                if (options.length > 0) {
                    iframe.contentWindow.postMessage(
                        { source: 'spectra', type: 'setFacilityOptions',
                          options: options,
                          availedId: ($('#ddlReceivedAccomodation').val() || '').toString() },
                        '*'
                    );
                    console.log('[ClaimAI] Sent facilityOptions:', options.length, 'options, availedId:', $('#ddlReceivedAccomodation').val());
                }
                claimAI_sendHospDates(0);
            }

            // ── DOA / DOD -> ClaimAI iframe ────────────────────────────────────────
            // Same pane, same channel as the accommodation values above. Sent on its
            // own message so it does not depend on the facility list being ready, and
            // polled for 20s because Fill_HospitalizationDetails populates #txtHospDOA
            // and #txtHospDOD asynchronously — which is why reading them once inside
            // SubmitAndLoadIframe produced a blank / stale discharge date. Each send is
            // de-duplicated, so the iframe only hears about real changes.
            var _claimAI_lastHospDates = '';
            function claimAI_sendHospDates(attempt) {
                attempt = attempt || 0;
                var iframe = document.getElementById('ifrClaimAI');
                if (iframe && iframe.contentWindow) {
                    var doa = ($('#txtHospDOA').val() || '').toString().trim();
                    var dod = ($('#txtHospDOD').val() || '').toString().trim();
                    var key = doa + '|' + dod;
                    if ((doa || dod) && key !== _claimAI_lastHospDates) {
                        _claimAI_lastHospDates = key;
                        iframe.contentWindow.postMessage(
                            { source: 'spectra', type: 'setHospDates',
                              admissionDate: doa, dischargeDate: dod },
                            '*'
                        );
                        console.log('[ClaimAI] Sent hospitalization dates — DOA:', doa || '(blank)', '| DOD:', dod || '(blank)');
                    }
                }
                if (attempt < 20) {
                    setTimeout(function () { claimAI_sendHospDates(attempt + 1); }, 1000);
                }
            }

            // Adjudicator edits a date after load -> push it straight through.
            // attempt=20 sends once without starting another poll.
            $(document).off('change.claimAIDates blur.claimAIDates', '#txtHospDOA, #txtHospDOD')
                       .on('change.claimAIDates blur.claimAIDates', '#txtHospDOA, #txtHospDOD', function () {
                           claimAI_sendHospDates(20);
                       });
```

That is the entire Spectra change.

---

## Notes

- `#txtHospDOA` / `#txtHospDOD` are the Hospitalization Details date boxes — the same
  ones the datepickers in this file bind to and `Fill_HospitalizationDetails` writes.
  Their format is `dd-M-yy` (e.g. `05-Nov-2025`); ClaimAI normalises it.
- `SubmitAndLoadIframe`'s existing `spectraFields.admissionDate` /
  `spectraFields.dischargeDate` lines are left alone. ClaimAI still reads them as a
  secondary source, so nothing regresses if this postMessage ever fails to fire.
- The `$('#ifrClaimAI').on('load', ...)` handler is registered twice in this file
  (a duplicate block). Harmless — both call `claimAI_sendFacilityOptions`, and the
  date send de-duplicates.
- `_setAllIframeSrcs` (global scope) also calls `claimAI_sendFacilityOptions`, which
  is defined inside `$(document).ready` and is not visible there. That path only runs
  under the staging flow, which is currently disabled — worth fixing separately if
  staging is ever re-enabled.

---

## Verify

Open a claim, then the **iframe** console (right-click inside the ClaimAI panel →
Inspect). Expect, in order:

1. `[ClaimAI] Sent hospitalization dates — DOA: … | DOD: …`  (Spectra page console)
2. `[ClaimAI] Hospitalization dates received — DOA: … | DOD: …`  (iframe console)
3. `[ClaimAI] Spectra DOA: … | Spectra DOD: …`  (iframe console)

Then hover the Discharge Date label in Patient Info: a green tick means the document
value and the Spectra DOD agree; an amber triangle shows both values side by side.
