# Spectra — discount popup not opening in the ClaimAI iframe

**One change: move two `<script>` tags from the bottom of `_ClaimAIFrameLayout.cshtml`
into its `<head>`.** No change to `ProviderDetailsFrame.cshtml` or the partial.

---

## Correction to my earlier diagnosis

I previously said jQuery UI was missing from the frame. It is not — the layout loads
both `/Content/css/jquery-ui.min.css` and `/Scripts/js/jquery-ui.min.js`. The problem
is not that they are absent; it is **when** they load.

---

## Root cause: the partial's inline script runs before jQuery exists

`_ClaimAIFrameLayout.cshtml` renders the page body first and loads every script
afterwards:

```cshtml
<body class="claimai-frame">
    @RenderBody()                                    <-- partial + its inline <script> runs HERE

    <script src="/Scripts/js/jquery.min.js"></script>     <-- jQuery arrives only now
    <script src="/Scripts/js/jquery-ui.min.js"></script>
```

`_ClaimProviderDetails.cshtml` has one large inline `<script>` block (lines 649-1592)
that starts executing the moment the browser reaches it — while `$` is still
undefined. Its first top-level jQuery statement is line 682:

```javascript
      $("#txt_nonnetwork_hd_add_state").change(function (e) {     // line 682
```

That throws `ReferenceError: $ is not defined`, which **aborts every remaining
top-level statement in the block** — including the dialog registration at line 1233:

```javascript
    $(function () {
        $("#dvServiceDisc").dialog({ autoOpen: false, width: 1200, ... });   // never runs
    });
```

Function *declarations* are hoisted, so `LoadProviderDetails`, `FillServiceDiscount`
and friends still exist — which is exactly why the provider data loads normally and
the AJAX fires with the correct URL. Only the top-level statements are lost.

The two symptoms in the screenshot follow directly:

- `#dvServiceDisc` was never converted into a dialog, so it stays in the document
  flow → the Service / Package Discount tabs render **inline**;
- `FillServiceDiscount()` calls `.dialog("open")` on an element jQuery UI never
  initialised, which throws *"cannot call methods on dialog prior to
  initialization"* on its first line — **before** the grid-fill code → the panel
  underneath is **empty**.

The main claim page does not have this problem because `_Layout.cshtml` loads jQuery
in the head, ahead of the body content.

---

## The change

In `Views/Shared/_ClaimAIFrameLayout.cshtml`:

### FIND (in `<head>`, the last stylesheet line)

```cshtml
    <link rel="stylesheet" href="/Scripts/DataTable.css" />
```

### REPLACE

```cshtml
    <link rel="stylesheet" href="/Scripts/DataTable.css" />

    @* jQuery + jQuery UI MUST load before RenderBody(). The partials rendered in
       this layout (_MemberView, _ClaimProviderDetails) carry large inline script
       blocks whose TOP-LEVEL statements use $ as soon as the browser reaches them.
       With these two at the bottom of the body, the first such statement threw
       "$ is not defined", which aborted the rest of the block - including the
       $("#dvServiceDisc").dialog({...}) registration, so the Service / Package
       Discount popup could never open and its tabs rendered inline instead.
       _Layout.cshtml loads jQuery in the head for the same reason. *@
    <script src="/Scripts/js/jquery.min.js"></script>
    <script src="/Scripts/js/jquery-ui.min.js"></script>
```

### FIND (at the bottom of `<body>`)

```cshtml
    <!-- Core scripts + the util files that define the bind/retrieve functions -->
    <script src="/Scripts/js/jquery.min.js"></script>
    <script src="/Scripts/js/jquery-ui.min.js"></script>
    <script src="/Scripts/js/ace-extra.min.js"></script>
```

### REPLACE

```cshtml
    @* Core scripts + the util files that define the bind/retrieve functions.
       jQuery and jQuery UI moved to the head - see the comment there. *@
    <script src="/Scripts/js/ace-extra.min.js"></script>
```

Everything else stays exactly where it is.

---

## Why this is enough

Once the dialog registration runs, `FillServiceDiscount()` finds an initialised
dialog, `.dialog("open")` succeeds and execution continues into the grid fill. No
change is needed in `ProviderDetailsFrame.cshtml` or in the partial.

It also repairs the same class of breakage in **Member View**, whose partial has the
same inline-script shape — any top-level jQuery in `_MemberView.cshtml` has been
failing silently for the same reason.

---

## Optional: size the dialog to the iframe

The partial hard-codes `width: 1200, height: 500`, which is wider than the AI Summary
panel, so the popup will be clipped horizontally. To fit it without touching the
shared partial, add this to the `@section scripts` block of
`ProviderDetailsFrame.cshtml`:

```javascript
        // The partial sizes this dialog for the full-width claim page (1200x500).
        // Shrink it to the iframe so it isn't clipped. Runs after the partial's own
        // $(function(){...}) has created the dialog.
        $(function () {
            var $d = $('#dvServiceDisc');
            if ($d.length && $d.hasClass('ui-dialog-content')) {
                $d.dialog('option', {
                    width:  Math.max(320, Math.min(1200, $(window).width()  - 40)),
                    height: Math.max(260, Math.min(500,  $(window).height() - 80))
                });
            }
        });
```

---

## Two things noticed while reading, not changed here

1. **Duplicate hidden-field ids.** `ProviderDetailsFrame.cshtml` declares
   `hdnProviderID` and `hdnMOUID`, and `_ClaimProviderDetails.cshtml` declares them
   again inside the discount anchor (lines 279-280). `$('#hdnMOUID')` returns only
   the first match — the frame's — which is why the MOU is correct today. It will
   break quietly if the markup order ever changes.

2. **`$('#ProviderDetailsDiv').addClass('collapsed')` at line 892 is top-level**, so
   it has not been running in the frame either. After this fix it WILL run and
   collapse the section on load; the frame's `LoadProviderDetails(...)` then removes
   the class on success (line 914), so the panel opens as before. If the retrieve
   call ever fails the section will now stay collapsed rather than showing empty.

---

## Verify

Open AI Summary → Provider Details and check the **iframe** console first: the
`$ is not defined` error at page load should be gone. Then click "Current Service
Discount & Package Discount" — the popup opens with the Service Discount grid filled
and "MOU ID: 358272" top-right, matching the Spectra page.

---

## Two encoding / Razor notes for this file

**1. Use Razor comments, not HTML comments, around anything with `@`.**
My first draft wrapped the new script tags in `<!-- ... @RenderBody() ... -->`.
Razor does NOT treat an HTML comment as inert: it still transitions on `@`, so that
`@RenderBody()` would have executed inside the head, rendering the body there and
then throwing *"RenderBody cannot be called more than once"* on the real call below.
The blocks above now use `@* ... *@`, which the Razor parser strips before the markup
is ever emitted. (If you ever need `@` literally in markup, escape it as `@@`.)

**2. The file already contains a corrupt character.**
Line 4 of the existing comment holds `U+FFFD` (bytes `EF BF BD`), the Unicode
replacement character:

```
so the widget renders cleanly inside ClaimAI's right panel <U+FFFD> it only pulls in
```

That was an em dash saved in Windows-1252 and later re-read as UTF-8. It is inside a
Razor comment, so it has no runtime effect at all - Visual Studio is only warning that
saving will make the substitution permanent. Two minutes to clean up properly:

1. Replace that character with a plain ASCII hyphen `-`.
2. File > Save As > click the arrow beside Save > **Save with Encoding...** >
   *Unicode (UTF-8 with signature) - Codepage 65001*.

Everything I have added above is ASCII-only, so it will not reintroduce the problem.
