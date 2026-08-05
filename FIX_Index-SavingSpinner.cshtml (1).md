# Add — Saving spinner during the iframe‑save delay  (Index.cshtml)

**File:** `Enrollment/Views/MedicalScrutiny/Index.cshtml`

**Goal:** the moment the user clicks **Save inside the ClaimAI iframe**, show a full‑screen spinner (same look as the "Finalizing claim…" overlay) so the screen never looks frozen during the save + delayed auto‑refresh. When the page auto‑refreshes, the spinner is gone (the page reloaded) and your existing "Finalizing claim…" overlay takes over — everything after the refresh stays exactly as it is now.

This is a **single addition** to the save handler. It's independent of the other fixes.

## How it behaves
- Spinner appears immediately when the save sequence starts.
- The "estimation days changed" browser pop‑up (if it appears) sits **on top** of the spinner — browser‑native dialogs always do — so the user can still click **OK**. After OK, they see the spinner, not a frozen page.
- The spinner is removed automatically when the page reloads. A 30‑second safety timer also removes it if, for any reason, the reload doesn't fire.

## BEFORE (around line 11387)

```javascript
                    // Step 0: Set approved accommodation based on claim type
                    var _isCataract = window._claimAI_claimType === 'cataract';
```

## AFTER

```javascript
                    // ClaimAI: full-screen spinner shown the moment Save is clicked, so the user
                    // isn't staring at a frozen page during the save + delayed auto-refresh. It
                    // stays until the page reloads (removed automatically); after the reload the
                    // "Finalizing claim…" overlay takes over. Same look as that overlay.
                    if (!document.getElementById('claimAI_savingOverlay')) {
                        $('<div/>', { id: 'claimAI_savingOverlay' })
                            .css({
                                position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
                                background: '#ffffff', zIndex: 99998,   // just below the bottom-right toasts (99999) so they show on top of the spinner
                                display: 'flex', flexDirection: 'column',
                                alignItems: 'center', justifyContent: 'center',
                                font: '600 16px/1.4 "Segoe UI", Arial, sans-serif', color: '#334155'
                            })
                            .html('<i class="fa fa-spinner fa-spin" style="font-size:38px;color:#2e7d32;margin-bottom:14px;"></i><div>Saving claim details&hellip;</div>')
                            .appendTo('body');
                        // Safety: never trap the user if the reload doesn't fire for some reason.
                        setTimeout(function () { $('#claimAI_savingOverlay').remove(); }, 30000);
                    }

                    // Step 0: Set approved accommodation based on claim type
                    var _isCataract = window._claimAI_claimType === 'cataract';
```

## Notes
- Uses the same markup, colours, font and `fa fa-spinner fa-spin` icon as your "Finalizing claim…" overlay, so the two feel like one continuous experience across the refresh — the message is just "Saving claim details…" instead of "Finalizing claim…".
- The spinner sits at `z-index: 99998`, **just below** the bottom-right toasts (which are all at `99999`). So the white spinner covers the page/iframe, but your "✓ Coding details saved", "✓ Bill details saved", "✓ Hospitalization details saved", etc. toasts still appear **on top of** the spinner as they fire. (The estimation-days browser pop-up is a native dialog, so it's always on top regardless.)
- No CSS `@keyframes` added (the spin comes from Font Awesome's `fa-spin`), so there's nothing new for Razor to parse.
- If you'd prefer a different label (e.g. "Please wait…" or "Processing…"), change the text inside the `<div>…</div>` in the `.html(...)` line.
