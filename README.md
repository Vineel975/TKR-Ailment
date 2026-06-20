// ── ClaimAI post-save auto-sequence (with screen cover) ────────────────────
// Add as a NEW $(document).ready(...) block. When the save-flag is set it:
//   1. covers the screen with a "Finalizing claim…" overlay,
//   2. runs Claim Actions -> Adjudication Process -> Calculate behind it,
//   3. scrolls to the bottom and fades the overlay out when done.
// The overlay is ALSO removed on any timeout/failure and by a backstop timer,
// so the user is never trapped behind it.
//
// NOTE: uses the Ace/Font-Awesome spinner (fa fa-spinner fa-spin) that the page
// already loads, so there is NO injected CSS keyframes block. (A literal "at-sign
// keyframes" in a .cshtml makes Razor parse "keyframes" as C# and throw
// "the name 'keyframes' does not exist in the current context".)
$(document).ready(function () {
  try {
    if (sessionStorage.getItem('claimAI_autoSeq') !== '1') return;
    var seqClaim = sessionStorage.getItem('claimAI_autoSeqClaimId') || '';
    var seqAt    = parseInt(sessionStorage.getItem('claimAI_autoSeqAt') || '0', 10);

    // Run once: clear the flag immediately.
    sessionStorage.removeItem('claimAI_autoSeq');
    sessionStorage.removeItem('claimAI_autoSeqClaimId');
    sessionStorage.removeItem('claimAI_autoSeqAt');

    // Ignore a stale flag (>2 min old) or a different claim now loaded.
    if (!seqAt || (Date.now() - seqAt) > 120000) return;
    var curClaim = $('#hdnClaimID').val() || '';
    if (seqClaim && curClaim && seqClaim !== curClaim) return;

    // ── Cover the screen so the user never sees the auto-clicks ──────────────
    $('<div/>', { id: 'claimAI_autoSeqOverlay' })
      .css({
        position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
        background: '#ffffff', zIndex: 2147483647,
        display: 'flex', flexDirection: 'column',
        alignItems: 'center', justifyContent: 'center',
        font: '600 16px/1.4 "Segoe UI", Arial, sans-serif', color: '#334155'
      })
      .html('<i class="fa fa-spinner fa-spin" style="font-size:38px;color:#2e7d32;margin-bottom:14px;"></i><div>Finalizing claim&hellip;</div>')
      .appendTo('body');

    var finished = false;
    function finish() {
      if (finished) return;
      finished = true;
      try { window.scrollTo(0, document.body.scrollHeight); } catch (e) {}
      $('#claimAI_autoSeqOverlay').fadeOut(250, function () { $(this).remove(); });
    }
    // Absolute backstop: never trap the user behind the overlay.
    setTimeout(finish, 60000);

    // Generic "click when the element is visible & enabled" (steps 1 & 2).
    function clickWhenReady(getEl, label, onDone, attempt) {
      attempt = attempt || 0;
      var $el;
      try { $el = getEl(); } catch (e) { $el = null; }
      if ($el && $el.length && $el.is(':visible') && !$el.is(':disabled')) {
        console.log('[ClaimAI] auto-sequence: clicking "' + label + '"');
        $el[0].click();
        if (onDone) setTimeout(onDone, 1500); else finish();
        return;
      }
      if (attempt >= 40) { // ~20s ceiling per step
        console.warn('[ClaimAI] auto-sequence: "' + label + '" not available — stopping.');
        finish();
        return;
      }
      setTimeout(function () { clickWhenReady(getEl, label, onDone, attempt + 1); }, 500);
    }

    // ── Calculate: race-proof (the intermittent failure was clicking before
    // adjudication finished loading the data BillCalculator() needs). We wait
    // for the button to be ready AND AJAX to be idle, click, then VERIFY the
    // calc actually ran — on success Spectra shows Re-Calculate / hides
    // Calculate — and retry if it didn't.
    function doCalculate() {
      var clickTries = 0, MAX_CLICKS = 4;
      function reCalcShown() {
        var $r = $('#btnReProcessBillAmt');
        return $r.length && $r.is(':visible');
      }

      function waitReadyThenClick() {
        if (reCalcShown()) { console.log('[ClaimAI] auto-sequence: calc already done.'); finish(); return; }
        var attempt = 0, MAX = 60; // ~30s
        (function poll() {
          if (reCalcShown()) { finish(); return; }
          var $btn = $('#btnBillCalculator');
          var btnReady = $btn.length && $btn.is(':visible') && !$btn.is(':disabled');
          var ajaxIdle = (typeof $.active === 'undefined') || $.active === 0;
          // Prefer AJAX-idle; if the button has been ready for ~5s, click anyway
          // (covers builds that keep a long-lived background request open).
          if (btnReady && (ajaxIdle || attempt >= 10)) {
            setTimeout(function () {
              if (reCalcShown()) { finish(); return; }
              clickTries++;
              console.log('[ClaimAI] auto-sequence: clicking "Calculate" (try ' + clickTries + ')');
              try { $btn.prop('disabled', false); } catch (e) {}
              try { $btn[0].click(); } catch (e) {}
              verifyOrRetry();
            }, 500);
            return;
          }
          if (attempt++ < MAX) { setTimeout(poll, 500); return; }
          // Never became ready: last-resort click / direct call, then reveal.
          console.warn('[ClaimAI] auto-sequence: "Calculate" never became ready — fallback.');
          if ($btn.length) { try { $btn.prop('disabled', false); $btn[0].click(); } catch (e) {} }
          else if (typeof window.BillCalculator === 'function') { try { window.BillCalculator(); } catch (e) {} }
          setTimeout(finish, 2500);
        })();
      }

      function verifyOrRetry() {
        var checks = 0, MAXC = 16; // ~4s
        (function check() {
          // Success: Re-Calculate appeared, or Calculate hid after our click.
          if (reCalcShown() || ($('#btnBillCalculator').length && !$('#btnBillCalculator').is(':visible'))) {
            console.log('[ClaimAI] auto-sequence: calculation completed.');
            setTimeout(finish, 800);
            return;
          }
          if (checks++ < MAXC) { setTimeout(check, 250); return; }
          if (clickTries < MAX_CLICKS) {
            console.warn('[ClaimAI] auto-sequence: calc not confirmed — retrying.');
            waitReadyThenClick();
          } else {
            console.warn('[ClaimAI] auto-sequence: calc unconfirmed after retries — stopping.');
            finish();
          }
        })();
      }

      waitReadyThenClick();
    }

    // Let the reloaded page finish its own initialisation first.
    setTimeout(function () {
      console.log('[ClaimAI] Post-save auto-sequence starting…');
      clickWhenReady(function () {
        return $('#divMSProceed button.dropdown-toggle');          // 1) Claim Actions
      }, 'Claim Actions', function () {
        clickWhenReady(function () {
          return $('#divMSProceed .dropdown-menu a').filter(function () {
            return $.trim($(this).text()) === 'Adjudication Process';
          }).first();                                              // 2) Adjudication Process
        }, 'Adjudication Process', function () {
          doCalculate();                                           // 3) Calculate (race-proof)
        });
      });
    }, 2500);
  } catch (e) {
    console.warn('[ClaimAI] Post-save auto-sequence error:', e);
    $('#claimAI_autoSeqOverlay').remove();
  }
});
