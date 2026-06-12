# Spectra — TKR benefit cap in `GetCodingProcedureEligibleLimit` (FINAL)

File: `MedicalScrutinyController.cs`. Confirmed from 4 real TKR claims + full Remarks.

**Cap rule (from the policy Remarks):**
- "f) Knee Replacement **Unilateral** - Rs.2,50,000/-"  → unilateral knee
- "g) Knee replacement **Bilateral** - Rs.4,50,000/-"   → bilateral knee

Both cap rows carry the **same** `TPAProcedureID = 597,599,598`, so the procedure list
can't tell them apart — the only distinguisher is the Remarks word **"Unilateral" / "Bilateral"**
(mutually exclusive: "unilateral" has no "b", so it never matches `%bilateral%`). Selection =
match the claim's laterality (from the coded procedure) to that word.

Laterality from the coded procedure:
- **bilateral**  = 599 (primary) / 1342 (re-do)            → Rs.4,50,000
- **unilateral** = 597 (R) / 598 (L) / 1343 (re-do uni)    → Rs.2,50,000

`isDayCare = 0` for TKR is already in your deployed method — no change there.

Full updated method is in `GetCodingProcedureEligibleLimit_TKR_full.cs` (paste-replace the
whole method — safest). The two edits are below for reference. Balance verified
(braces 43/43, parens 167/167, brackets 53/53, identical to the original).

---

## EDIT 1 — coded BPSIConditions fallback: pick the cap by laterality (the key fix)

This is the path that runs for coded TKR claims. Today its `ORDER BY … ASC` returns the
smaller cap (Rs.2,50,000) for everything. Add a laterality CASE before the ASC tie-break.

### FIND
```sql
                          AND EXISTS (SELECT 1 FROM fn_Split(bsc.TPAProcedureID, ',')
                                      WHERE LTRIM(RTRIM(Stringvalue)) = @ProcedureID)
                          ORDER BY COALESCE(bsc.ClaimLimit, bsc.IndividualLimit, bsc.FamilyLimit, bsc.PolicyLimit) ASC", conn))
```

### REPLACE WITH
```sql
                          AND EXISTS (SELECT 1 FROM fn_Split(bsc.TPAProcedureID, ',')
                                      WHERE LTRIM(RTRIM(Stringvalue)) = @ProcedureID)
                          ORDER BY
                              -- TKR: both unilateral & bilateral knee cap rows carry the same
                              -- TPAProcedureID list, so the EXISTS above matches both. Pick the
                              -- row whose Remarks laterality matches the coded procedure:
                              --   bilateral knee  = 599 (primary) / 1342 (re-do) -> bilateral  (Rs.4,50,000)
                              --   unilateral knee = 597 / 598 / 1343             -> unilateral (Rs.2,50,000)
                              -- Inert for non-TKR procedures (they fall to ELSE 1 and keep ASC).
                              CASE
                                  WHEN @ProcedureID IN ('599','1342')       AND LOWER(bsc.Remarks) LIKE '%bilateral%'  THEN 0
                                  WHEN @ProcedureID IN ('597','598','1343') AND LOWER(bsc.Remarks) LIKE '%unilateral%' THEN 0
                                  ELSE 1
                              END,
                              COALESCE(bsc.ClaimLimit, bsc.IndividualLimit, bsc.FamilyLimit, bsc.PolicyLimit) ASC", conn))
```

`@ProcedureID` is already a parameter (passed as a string), so no new parameter is needed.

---

## EDIT 2 — uncoded fallback: match the TKR cap by procedure id (robust)

Your deployed uncoded branch matches on Remarks text. That happens to work on this policy,
but is fragile across policies. Switch it to match the knee procedure ids — same approach as
the coded path. Uncoded can't know laterality, so it correctly defaults to unilateral
(Rs.2,50,000) via the ASC tie-break; the coded path refines it once coding is done.

### FIND
```sql
                                  WHEN @ClaimType = 'tkr'       AND par.Name = 'Ailment Conditions' AND (
                                           LOWER(bsc.Remarks) LIKE '%knee replacement%'
                                        OR LOWER(bsc.Remarks) LIKE '%total knee%'
                                        OR LOWER(bsc.Remarks) LIKE '%knee arthroplasty%'
                                        OR LOWER(bsc.Remarks) LIKE '%tkr%'
                                        OR LOWER(bsc.Remarks) LIKE '%joint replacement%') THEN 0
```

### REPLACE WITH
```sql
                                  WHEN @ClaimType = 'tkr'       AND par.Name = 'Ailment Conditions' AND EXISTS (
                                           SELECT 1 FROM fn_Split(bsc.TPAProcedureID, ',')
                                           WHERE LTRIM(RTRIM(Stringvalue)) IN ('597','598','599','1342','1343')) THEN 0
```

---

## Expected results (verified by trace)

| Claim                         | Coded proc | Laterality  | Benefit cap |
|-------------------------------|-----------:|-------------|------------:|
| claim2 (26050970022)          | 597 (R)    | unilateral  | 2,50,000    |
| claim3 (26051590764)          | 598 (L)    | unilateral  | 2,50,000    |
| claim4 (26051592214)          | 597 (latest)| unilateral | 2,50,000    |
| (bilateral, primary)          | 599        | bilateral   | 4,50,000    |
| (re-do bilateral)             | 1342       | bilateral   | 4,50,000    |
| (re-do unilateral)            | 1343       | unilateral  | 2,50,000    |
| claim1 (26050868073)          | 1150 / none| n/a — no knee cap row | full SI |

Non-TKR claims (cataract/maternity/other) are unaffected — their coded procedure isn't in the
id lists and their cap rows don't carry "unilateral/bilateral", so the CASE falls to `ELSE 1`.

## Deploy
Apply both edits (or paste the full method), build in Visual Studio, recycle the app pool.
No ClaimAI change.
