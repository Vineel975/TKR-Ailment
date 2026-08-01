# GIPSA PPN → SOC Two-Pass Tariff Extraction — Change Document

## 1. What this feature does

For a specific class of claims, the tariff extraction now tries the **PPN** tariff
file first and, only if that file has no exact match, falls back to the
**non-PPN / SOC** tariff file.

**Trigger — all four must hold:**
1. Insurer is a **Public (PSU)** insurer.
2. The selected tariff zip is a **GIPSA** zip.
3. That GIPSA zip contains **both** a PPN file and a non-PPN/SOC file.
4. The lens named in the claim document is **not monofocal** (i.e. multifocal /
   toric / premium).

**Behaviour when triggered:**
1. **PPN pass** — look for a row matching the claim's **exact procedure + exact
   lens type**. If found → use it; the Tariff Extraction card shows the **PPN
   file name**.
2. **SOC pass** (only if the PPN had no exact combination) — look for the
   **exact-or-nearest** procedure+lens combination in the SOC file. If found →
   use it; the card shows the **SOC file name**.
3. **Neither file yields a usable row** → **empty tariff** ("no matching tariff
   row").

When the trigger does **not** hold, behaviour is exactly as before: one file,
single-pass extraction.

Design decisions confirmed with the product owner:
- "Combination found in PPN" = **exact procedure AND exact lens type** both match.
- If PPN misses and SOC also has nothing usable → **empty tariff** (not a nearest
  PPN fallback).

**Both submit paths covered.** The two-pass works for BOTH the **browser** submit
path (`StartClaimAuditProxy`) and the **staging worker** path
(`SubmitClaimToClaimAI`). Both fetch the SOC file server-side via
`GetTariffDocument(claimId, slNo, wantSoc: true)` and add it to the multipart body.

**Fallback-file naming.** The non-PPN fallback file is recognised when its filename
contains **"soc"** OR **"non-ppn" / "nonppn" / "non ppn"**. The PPN file is a GIPSA
file whose name contains none of those. (Both `GipsaZipHasBothPpnAndSoc` and
`PickBestSocTariffFile` use this widened match.)

---

## 2. Architecture / data flow

```
Spectra (MedicalScrutinyController.cs)
  GetTariffDocument(claimId, slNo)            -> picks PPN (primary) file
  GetTariffDocument(claimId, slNo, wantSoc=true) -> picks SOC file (guarded)
        │  (SOC returned only if PSU + GIPSA zip has BOTH ppn and soc)
        ▼
  SubmitClaimToClaimAI(... socName, socB64)
  BuildMultipartBody(... socTariffBill part)
        │  multipart POST
        ▼
ClaimAI  POST /api/audit/start
  reads  tariffBill + socTariffBill
  uploads both to Convex storage
  createJobAndProcess({ tariffStorageId, socTariffStorageId, ... })
  runTariffMatching({ tariffStorageId, socTariffStorageId,
                      tariffFileName, socTariffFileName })
        ▼
Convex  resolveTariffForResult(...)
  runTwoPass = (socTariffPdfBuffer present) AND (claim lens is multifocal)
    Pass 1  extractClaimTariffFromPdf(PPN, "PPN (exact-combination only)")
              -> exactCombinationFound?
    Pass 2  extractClaimTariffFromPdf(SOC, "SOC (exact or nearest)")  [only if needed]
  resolvedTariffFileName = PPN name or SOC name (whichever produced the rows)
```

The gating conditions that need zip contents + insurer (PSU, GIPSA-zip,
both-files-present) are decided in **Spectra** (it has the zip and insurer). The
**non-monofocal-lens** condition is enforced again in **ClaimAI** as a safety net,
so the second pass never runs for a monofocal claim even if both files arrive.

---

## 3. Files changed

### A. Spectra (C#) — `MedicalScrutinyController.cs`

1. **`GipsaZipHasBothPpnAndSoc(candidates)`** *(new helper)* — returns true when the
   candidate list contains both a GIPSA-not-SOC file (PPN) and a GIPSA-SOC file.

2. **`PickBestSocTariffFile(candidates)`** *(new helper)* — picks the GIPSA-SOC file
   (filename contains "gipsa" and "soc"), latest-modified first, converted to PDF
   via the existing `EnsurePdf`. Mirrors the P3 tier of `PickBestTariffFile`.

3. **`GetTariffDocument(claimId, slNo, bool wantSoc = false)`** *(param added)* — when
   `wantSoc` is true, both the local-zip branch and the S3/prod branch return the
   **SOC** file instead of the AI-selected file, but **only** for PSU claims whose
   GIPSA zip contains both a PPN and a SOC file. Otherwise they return an explicit
   "no SOC" payload (`fileName = null, base64Content = null`) so the caller sends
   only the PPN. The default (`wantSoc = false`) path is byte-for-byte unchanged.

4. **Staging submit flow** — after fetching the primary tariff, it now also calls
   `GetTariffDocument(claimId, slNo, true)` to fetch the SOC file (best-effort;
   empty unless the trigger holds), and passes `socName` / `socB64` onward.

5. **`SubmitClaimToClaimAI(...)`** — two optional params `socName`, `socB64`. Decodes
   the SOC bytes, adds `tariffFileName` and `socTariffFileName` to `spectraFields`,
   and passes the SOC file to `BuildMultipartBody`.

6. **`BuildMultipartBody(...)`** — two optional params `socTarFileName`,
   `socTarBytes`. When present, writes a second file part named **`socTariffBill`**
   into the multipart body.

*Verification:* C# cannot be compiled in this environment; the edits were verified
by brace balance (0-balanced, unchanged) and paren-delta (identical to the
original), and every anchor matched exactly once.

### B. ClaimAI — Next.js route: `app/api/audit/start/route.ts`
- Reads the new `socTariffBill` file part.
- Uploads it to Convex storage → `socTariffStorageId`.
- Passes `socTariffStorageId`, `tariffFileName`, `socTariffFileName` into
  `createJobAndProcess` and into the `runTariffMatching` action call.

### C. ClaimAI — Convex schema: `convex/schema.ts`
- `jobFiles.fileType` union gains `"tariffSoc"` so the SOC file can be stored.

### D. ClaimAI — Convex mutation: `convex/jobMutations.ts`
- `createJobAndProcess` gains `socTariffStorageId` + `socTariffFileName` args and
  stores the SOC file as its own `jobFiles` row (`fileType: "tariffSoc"`).

### E. ClaimAI — Convex processing: `convex/processPdf.ts`
- `extractClaimTariffFromPdf(...)` gains an optional `passLabel` param (injects a
  `TARIFF PASS: ...` line into the claim context) and now returns
  `exactCombinationFound`.
- `resolveTariffForResult(...)` gains `uploadedSocTariffStorageId`, `tariffFileName`,
  `socTariffFileName`; fetches the SOC buffer; runs the **two-pass orchestration**
  (PPN exact-only → SOC exact-or-nearest → else empty); sums cost/usage across both
  passes; sets `resolvedTariffFileName` to the file the rows came from.
- `TariffMatchResult` gains `resolvedTariffFileName`; `applyTariffMatch` writes it to
  `tariffFileName` so the chosen file surfaces in the UI card.
- The `runTariffMatching` action + the internal-processing action gain the SOC args
  and pass them through to `resolveTariffForResult`.

### F. ClaimAI — Schema/prompt: `src/models.ts`, `src/prompts.ts`
- `tariffCalculationSchema` gains `exactCombinationFound` (boolean, nullable).
- Prompt: instructions for `exactCombinationFound`, and the two pass modes —
  `PPN (exact-combination only)` (exact match or empty) vs `SOC (exact or nearest)`.

---

## 4. Deploy order

1. **ClaimAI** — Convex deploy (schema + processPdf + jobMutations + models +
   prompts) and the Next.js app (audit/start route). Safe to deploy first: with no
   SOC file arriving, every path is a single-pass no-op.
2. **Spectra** — rebuild/deploy `MedicalScrutinyController.cs`.
3. **Reprocess** the affected claims (extraction-side change).

Because ClaimAI treats an absent SOC file as "single-pass as before", deploying
ClaimAI ahead of Spectra changes nothing for existing claims until Spectra starts
sending the second file.

---

## 5. Notes / caveats

- **Cost:** for the specific claims that reach Pass 2 (PPN missed the exact combo),
  tariff extraction runs twice → ~2× tariff-extraction cost for those claims only.
- **PPN/SOC detection** is filename-based ("gipsa" + "soc"), matching the existing
  `PickBestTariffFile` tier logic — no new heuristic introduced.
- The **non-monofocal** gate is enforced in ClaimAI, so even if Spectra sends both
  files for a monofocal claim, the two-pass won't run.
- If Spectra's SOC endpoint returns "no SOC" (not PSU, or the zip lacks both files),
  ClaimAI never receives a SOC file and runs exactly as today.

## 6. Are all other claims unaffected?

Yes. Every non-qualifying claim behaves exactly as before, guaranteed at three
independent layers:

1. **Spectra never sends a SOC file** unless `wantSoc && isPsu &&
   GipsaZipHasBothPpnAndSoc(...)`. The normal tariff fetch (`wantSoc = false`) is
   byte-for-byte unchanged.
2. **ClaimAI only runs the two-pass** when `socTariffPdfBuffer` is present AND the
   claim lens is multifocal (`runTwoPass = !!socTariffPdfBuffer && claimLensCat ===
   "multifocal"`). Otherwise it calls the original single-pass `extractClaimTariffFromPdf`
   with the original signature.
3. **The prompt is unchanged for single-pass claims** — the `TARIFF PASS:` line is
   only injected when a `passLabel` is supplied (two-pass only). `exactCombinationFound`
   is read and returned but only *acted on* inside the two-pass branch.

So: non-PSU claims, non-GIPSA zips, GIPSA zips with only one file, and monofocal
claims all follow the exact original code path.
