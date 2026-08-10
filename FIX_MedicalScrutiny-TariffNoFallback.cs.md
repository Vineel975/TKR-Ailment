# Fix — AI's deliberate "no tariff" (tier=none) must not fall back to a wrong-insurer file  (MedicalScrutinyController.cs)

**File:** `Enrollment/Controllers/MedicalScrutinyController.cs`
**In:** the AI tariff-selection method (~line 9776, right after reading the AI response)

## Bug
When the AI returns an **empty** `selectedFile` with `tier=none` (a *deliberate* "no applicable tariff for this insurer"), the C# treats it the same as an AI failure and runs `PickBestTariffFile`, which then grabs a file restricted to a **different** insurer (e.g. an "only for ACKO" file for an SBI claim).

The AI's empty is only reached when there is genuinely no applicable file. It must be **respected**, not overridden.

> Note: paired with the ClaimAI `prompts.ts` change, the AI now selects a generic (unrestricted) in-window file when one exists — so it returns empty only when there truly is no applicable tariff. This C# fix ensures that empty is honoured.

## Edit — respect `tier=none`

### BEFORE (~line 9776)
```csharp
                            if (!string.IsNullOrWhiteSpace(selectedFile))
                            {
                                var match = candidates.Find(c => c.Item1 == selectedFile);
                                if (match != null)
                                {
                                    byte[] converted = EnsurePdf(match.Item1, match.Item3);
                                    if (converted != null) return System.Tuple.Create(match.Item1, converted);
                                }
                            }
```

### AFTER
```csharp
                            if (!string.IsNullOrWhiteSpace(selectedFile))
                            {
                                var match = candidates.Find(c => c.Item1 == selectedFile);
                                if (match != null)
                                {
                                    byte[] converted = EnsurePdf(match.Item1, match.Item3);
                                    if (converted != null) return System.Tuple.Create(match.Item1, converted);
                                }
                            }
                            else
                            {
                                // AI DELIBERATELY returned no file (tier=none): there is no tariff
                                // applicable to THIS insurer. Respect it — do NOT fall back to the
                                // rule-based picker, which would grab a file restricted to a
                                // different insurer (e.g. an "only for ACKO" file for an SBI claim).
                                string _tier = result?.priorityTier?.ToString();
                                if (string.Equals(_tier, "none", System.StringComparison.OrdinalIgnoreCase))
                                {
                                    _lastTariffSelectionLog += " -> AI deliberately returned NO tariff (tier=none); NOT falling back.";
                                    TariffLog("[Tariff] AI returned no applicable tariff (tier=none) — not falling back.");
                                    return null; // no tariff -> your existing 'tariff not available' path
                                }
                            }
```

## Result
- AI selects a file → used (unchanged).
- AI returns `tier=none` (no applicable tariff) → **return null** (tariff not available), which flows into your existing no-tariff → Refer-To-CRM path. No wrong-insurer file is ever substituted.
- AI errors / times out / non-success → still falls back to `PickBestTariffFile` (unchanged), so a real AI outage is still covered.

## Related (recommended, separate)
`PickBestTariffFile` itself does not exclude insurer-restricted files. It's now only reached on a genuine AI failure, but for full safety it should also skip files whose name says "only for <a different insurer>" (mirroring the AI's INSURER-RESTRICTED rule). If you want, share that method and I'll add the same exclusion + generic-file preference so even the offline fallback can't pick a wrong-insurer tariff.

## Notes
- Spectra change only — rebuild/redeploy. Pair with the updated ClaimAI `prompts.ts` (redeploy ClaimAI, reprocess the claim).
- After both: the ASG/SBI claim selects `6405-Asg Hospital Pvt Ltd Muzaffarpur BH ( 03-Apr-2018 ).pdf` — the only generic, in-window file — instead of an "only for ACKO" file.
