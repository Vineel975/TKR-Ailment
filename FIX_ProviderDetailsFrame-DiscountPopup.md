# Spectra — "Current Service Discount & Package Discount" popup not opening in the ClaimAI iframe

The frame is missing **jQuery UI**. One file changes: `ProviderDetailsFrame.cshtml`.

---

## Root cause

The popup is a jQuery UI dialog. `_ClaimProviderDetails.cshtml` turns the section
into one on DOM ready:

```javascript
    $(function () {
        $("#dvServiceDisc").dialog({
            autoOpen: false, width: 1200, height: 500, modal: true, ...
        });
    });
```

and the link's handler opens it:

```javascript
    function FillServiceDiscount() {
        $("#dvServiceDisc").dialog("open");      // <-- line 1281
        $('#tbServiceDisc tbody').empty();
        ...                                       // grid fill happens AFTER this line
```

`_ClaimAIFrameLayout.cshtml` does not load jQuery UI (the partial itself only pulls
`CommonUtils.js` and `ContactUtils.js`), so inside the frame `$.fn.dialog` is
undefined. Two things follow, and both match the screenshot exactly:

1. The init never converts `#dvServiceDisc` into a dialog, so it stays in the normal
   document flow — which is why the "Service Discount / Package Discount" tabs appear
   **inline** on the page instead of inside a popup.
2. `FillServiceDiscount()` throws on its very first line, **before** it fills the
   grids — which is why the panel underneath the tabs is empty.

The AJAX itself is fine. `GetProviderServicePackageDisc` accepts the extra parameter
(`bool isFrmArchived = false` is in its signature), the URL carries the right
ClaimID / ProviderID / MouId, and `FillProviderServicePkgDisc()` completes; the
failure is only in `FillDiscGrid()` → `FillServiceDiscount()`.

---

## The change

Replace the whole of `Views/MedicalScrutiny/ProviderDetailsFrame.cshtml` with:

```cshtml
@{
    Layout = "~/Views/Shared/_ClaimAIFrameLayout.cshtml";
    ViewBag.Title = "Provider Details";
    var claimID = Convert.ToInt64(ViewBag.ClaimID ?? 0);
    var providerID = Convert.ToInt64(ViewBag.ProviderID ?? 0);
    var memberPolicyID = Convert.ToInt64(ViewBag.MemberPolicyID ?? 0);
}

@*
    Standalone frameable Provider Details for the ClaimAI iframe.
    Place at: Views/MedicalScrutiny/ProviderDetailsFrame.cshtml

    The _ClaimProviderDetails partial defines LoadProviderDetails(...) which calls
    /MedicalScrutiny/ProviderDetails_Retrieve. We render the partial, seed the
    hidden fields, and fire LoadProviderDetails on load (same as the + button).
*@

@* #dvServiceDisc is a jQuery UI dialog. Until it is initialised it sits in the
   normal document flow, so hide it here — otherwise the Service/Package Discount
   tabs render inline on the page (they did, before this fix). jQuery UI sets an
   INLINE display on open, which beats this rule, so the popup still shows. *@
<style>
    #dvServiceDisc { display: none; }
</style>

@* Hidden fields the _ClaimProviderDetails partial reads from. *@
<input type="hidden" id="hdnClaimID" value="@claimID" />
<input type="hidden" id="hdnClaimSlNo" value="0" />
<input type="hidden" id="hdnProviderID" value="@providerID" />
<input type="hidden" id="hdnMemberPolicyID" value="@memberPolicyID" />
<input type="hidden" id="hdnMOUID" value="0" />
<input type="hidden" id="hdnIsFrmArchived" value="false" />

@Html.Partial("_ClaimProviderDetails")

@section scripts {
    <script>
        // ── jQuery UI, on demand ────────────────────────────────────────────────
        // The frame layout does not load jQuery UI, so $.fn.dialog is undefined and
        // the "Current Service Discount & Package Discount" popup could not open.
        // Load it here, then initialise the dialog the partial declared.
        // If NONE of the paths resolve we install a minimal shim so the section
        // still opens inline and the grids still fill — degraded, but not broken.
        (function () {
            var UI_CSS = [
                '/Content/css/jquery-ui.min.css',
                '/Content/css/jquery-ui.css',
                '/Content/css/jquery-ui-1.10.3.full.min.css'
            ];
            var UI_JS = [
                '/Scripts/jquery-ui.min.js',
                '/Scripts/js/jquery-ui.min.js',
                '/Content/js/jquery-ui.min.js',
                '/Scripts/jquery-ui-1.10.3.full.min.js'
            ];

            function tryCss(list, i) {
                if (i >= list.length) return;
                var l = document.createElement('link');
                l.rel = 'stylesheet';
                l.href = list[i];
                l.onerror = function () { tryCss(list, i + 1); };
                document.head.appendChild(l);
            }

            function tryJs(list, i, done) {
                if (i >= list.length) { done(false); return; }
                var s = document.createElement('script');
                s.src = list[i];
                s.onload = function () {
                    console.log('[ClaimAI] jQuery UI loaded from ' + list[i]);
                    done(true);
                };
                s.onerror = function () { tryJs(list, i + 1, done); };
                document.body.appendChild(s);
            }

            function initDialog() {
                var $d = $('#dvServiceDisc');
                if (!$d.length) return;

                // Fit the iframe rather than the 1200x500 the main page uses — the
                // AI Summary panel is narrower, and an oversized dialog gets clipped.
                var w = Math.max(320, Math.min(1200, $(window).width() - 40));
                var h = Math.max(260, Math.min(500, $(window).height() - 80));

                if ($d.hasClass('ui-dialog-content')) {
                    $d.dialog('option', { width: w, height: h });   // already initialised
                    return;
                }
                $d.dialog({
                    autoOpen: false,
                    width: w,
                    height: h,
                    title: "",
                    dialogClass: "no-close",
                    closeOnEscape: true,
                    draggable: true,
                    modal: true,
                    buttons: [{
                        html: "<button class='btn btn-xs btn-grey'>Close</button>",
                        click: function () { $(this).dialog("close"); }
                    }]
                });
            }

            function installShim() {
                console.warn('[ClaimAI] jQuery UI not found — Service/Package Discount will open inline, not as a popup.');
                if (typeof $.fn.dialog === 'function') return;
                $.fn.dialog = function (opt) {
                    if (opt === 'open') {
                        this.show();
                        try { this[0].scrollIntoView({ behavior: 'smooth', block: 'start' }); } catch (e) { }
                    } else if (opt === 'close') {
                        this.hide();
                    } else if (opt !== 'option') {
                        this.hide();          // init: stay hidden until opened
                    }
                    return this;
                };
            }

            $(function () {
                if (typeof $.fn.dialog === 'function') { initDialog(); return; }
                tryCss(UI_CSS, 0);
                tryJs(UI_JS, 0, function (ok) {
                    if (ok) { initDialog(); } else { installShim(); }
                });
            });
        })();

        $(function () {
            try {
                // Same call the "+" toggle on Provider Details makes.
                LoadProviderDetails(@claimID, @providerID, @memberPolicyID, null, false);
            } catch (e) { console.error('[ClaimAI] LoadProviderDetails failed', e); }
        });
    </script>
}
```

---

## Better: fix it once in the layout

The candidate-path list above exists only because I could not see
`_ClaimAIFrameLayout.cshtml`. If that layout gets jQuery UI directly:

```html
    <link href="~/Content/css/jquery-ui.min.css" rel="stylesheet" />
    <script src="~/Scripts/jquery-ui.min.js"></script>
```

(matching whatever `_Layout.cshtml` already uses — the main claim page needs jQuery UI
for `.datepicker()`, `.dialog()` and `.spinner()`, so the correct path is already
referenced there) then the loader block collapses to just `initDialog()`, and any
OTHER frame you add later gets dialogs, datepickers and spinners for free. Send me
that layout and I'll write the trimmed version.

---

## Two things noticed while reading, not fixed here

1. **Duplicate hidden-field ids.** The frame declares `hdnProviderID` and `hdnMOUID`,
   and `_ClaimProviderDetails.cshtml` declares them AGAIN inside the discount anchor
   (lines 279-280). `$('#hdnMOUID')` returns only the first match — the frame's —
   which is why the MOU works today. It will break quietly the day the markup order
   changes. Worth removing the frame's two lines and letting the partial own them,
   or vice-versa.

2. **`hdnClaimSlNo` is hard-coded to `0`** in the frame. Provider Details does not
   appear to use it, but anything else in the partial that does will read 0 rather
   than the claim's real extension number.

---

## Verify

Open AI Summary → Provider Details, click "Current Service Discount & Package
Discount". Expected in the iframe console:

```
[ClaimAI] jQuery UI loaded from /Scripts/jquery-ui.min.js
```

and the popup opens with the Service Discount grid populated and the "MOU ID: 358272"
label top-right — the same as the Spectra page. If you instead see the
`jQuery UI not found` warning, none of the candidate paths were right: check the
`<script src>` for jquery-ui in `_Layout.cshtml` and put that exact path first in
`UI_JS`.
