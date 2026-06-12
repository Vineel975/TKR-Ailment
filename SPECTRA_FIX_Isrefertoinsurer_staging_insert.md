# Approve error — `USP_ClaimRules_Insert expects parameter '@Isrefertoinsurer'`

## Cause
The Approve flow stages the claim via the proc `USP_ClaimRules_Insert`. That proc now declares
a `@Isrefertoinsurer` parameter (a sibling of the existing `@Isrefertocrm`) **with no default**,
so SQL Server treats it as required. The C# that builds the command —
`Enrollment/ViewModel/MedicalScrutinyViewModel.cs` → `ClaimRules_Insert(...)` — adds
`@Isrefertocrm` but never adds `@Isrefertoinsurer`, so the proc throws before inserting.

## Fix A (recommended) — add the missing parameter in the BAL
In `MedicalScrutinyViewModel.cs`, inside `ClaimRules_Insert`, right after the `@Isrefertocrm`
line (≈ line 461), add the `@Isrefertoinsurer` parameter, mirroring the same pattern:

### FIND
```csharp
                vDBHelper.mAddParameter("@Isrefertocrm", SqlDbType.VarChar, ParameterDirection.Input, Isrefertocrm);
                vDBHelper.mAddParameter("@SkipAudit", SqlDbType.Bit, ParameterDirection.Input, SkipAudit);
```

### REPLACE WITH
```csharp
                vDBHelper.mAddParameter("@Isrefertocrm", SqlDbType.VarChar, ParameterDirection.Input, Isrefertocrm);
                vDBHelper.mAddParameter("@Isrefertoinsurer", SqlDbType.VarChar, ParameterDirection.Input, "0");
                vDBHelper.mAddParameter("@SkipAudit", SqlDbType.Bit, ParameterDirection.Input, SkipAudit);
```

Notes:
- `"0"` = "not referred to insurer", which is the correct value for a normal approve. The real
  refer-to-insurer workflow is handled separately in the controller (the `is_refer_to_insurer`
  / `ReferToInsurer` logic), not in this staging insert.
- Match the proc's declared type. If your proc declares `@Isrefertoinsurer` as **BIT** rather
  than VARCHAR, use:
  ```csharp
  vDBHelper.mAddParameter("@Isrefertoinsurer", SqlDbType.Bit, ParameterDirection.Input, false);
  ```
  (Passing VarChar `"0"` to a BIT param also converts fine, but matching the type is cleaner.)
- Rebuild in Visual Studio and recycle the app pool.

## Fix B (instant, no rebuild) — give the proc parameter a default
If you'd rather not redeploy right now, make the parameter optional in the proc so it no longer
has to be supplied. In SSMS, `ALTER PROCEDURE USP_ClaimRules_Insert` and change its declaration:

```sql
@Isrefertoinsurer VARCHAR(10) = '0'   -- or: @Isrefertoinsurer BIT = 0 — match the proc's actual type
```

(Use whatever type the proc already declares; just append `= '0'` / `= 0`.) This unblocks Approve
immediately. You can still add Fix A later so the value is passed explicitly.

## Which to use
Both end up writing `0` for now, so the result is the same. Fix A keeps the BAL ↔ proc contract
explicit (the proc stays strict); Fix B is the fastest if you can't rebuild this minute. If the
`@Isrefertoinsurer` column is meant to store the *actual* refer-to-insurer status on the staging
row, that's a larger change (thread a real value from the controller through the BAL signature) —
tell me and I'll wire it end to end.

## Caveat on line numbers
The BAL snapshot I have is from the older spectra zip; your current `MedicalScrutinyViewModel.cs`
may sit at a slightly different line, but the `@Isrefertocrm` mAddParameter line is the anchor —
add the new line immediately after it.
