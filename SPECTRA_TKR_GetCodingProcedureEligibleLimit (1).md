# Spectra — TKR changes in `GetCodingProcedureEligibleLimit`

File: `MedicalScrutinyController.cs`. Two edits. **Edit A is confirmed — apply now.**
**Edit B is a draft that needs the TKR `BPSIConditions` data to finalize** (run the
diagnostic at the bottom and send it back).

No other Spectra change is needed for TKR: claim-type identification now flows from
ClaimAI's keyword classifier (`/api/classify-claim-type`) through `GetStagingClaimType`
and `GetClaimType`, so once ClaimAI knows `tkr`, both staging and on-demand do too.

---

## EDIT A — `isDayCare` (TKR is inpatient, not day-care). CONFIRMED.

Around line 8875.

### FIND
```csharp
                    // Maternity = inpatient (isDayCare=0). Cataract = daycare (isDayCare=1)
                    byte isDayCare = (claimType == "maternity") ? (byte)0 : (byte)1;
```

### REPLACE WITH
```csharp
                    // Maternity & TKR = inpatient (isDayCare=0). Cataract = daycare (isDayCare=1)
                    byte isDayCare = (claimType == "maternity" || claimType == "tkr") ? (byte)0 : (byte)1;
```

`claimType` arrives lowercased from the classifier (same as the existing `"maternity"`
check relies on), so `"tkr"` matches.

---

## EDIT B — benefit-cap disease-aware CASE (uncoded fallback)

CONFIRMED from 4 real TKR claims: the TKR cap row is identified by its **`TPAProcedureID`**
containing a knee-replacement procedure (597/598/599, or re-do 1342/1343). The `Remarks` are
generic ("CAPPING LIMIT OF 8 LISTED AILMENTS…") with no knee/TKR text — so match by
procedure id, NOT by remarks.

This adds a `tkr` branch to the `ORDER BY CASE` you already have in the **uncoded** benefit
fallback (the block with `ailmentLimitFb`).

### FIND (current cataract/maternity CASE)
```sql
                                  ORDER BY
                                      CASE
                                          WHEN @ClaimType = 'maternity' AND par.Name = 'Maternity'          AND c.Name = 'Maternity'                 THEN 0
                                          WHEN @ClaimType = 'cataract'  AND par.Name = 'Ailment Conditions' AND LOWER(bsc.Remarks) LIKE '%cataract%' THEN 0
                                          ELSE 1
                                      END,
```

### REPLACE WITH (adds the tkr branch — matches by knee procedure id)
```sql
                                  ORDER BY
                                      CASE
                                          WHEN @ClaimType = 'maternity' AND par.Name = 'Maternity'          AND c.Name = 'Maternity'                 THEN 0
                                          WHEN @ClaimType = 'cataract'  AND par.Name = 'Ailment Conditions' AND LOWER(bsc.Remarks) LIKE '%cataract%' THEN 0
                                          WHEN @ClaimType = 'tkr'       AND par.Name = 'Ailment Conditions' AND (
                                                   ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,597,%'
                                                OR ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,598,%'
                                                OR ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,599,%'
                                                OR ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,1342,%'
                                                OR ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,1343,%') THEN 0
                                          ELSE 1
                                      END,
```

No `@ClaimType` parameter change is needed — it already exists from the cataract fix.

### STILL OPEN — ₹2,50,000 vs ₹4,50,000 selection (blocks final sign-off)
Each covered policy has TWO knee cap rows (₹2,50,000 and ₹4,50,000). The `ORDER BY … ASC`
tie-break above (and the existing **coded** path) currently pick the **smaller, ₹2,50,000**,
for every TKR claim. If the two values are single-knee vs both-knees, bilateral claims must
pick ₹4,50,000 — which needs a laterality rule in BOTH the coded path and this fallback
(597/598/1343 → smaller; 599/1342 → larger). Confirm the rule (and send the full untruncated
Remarks) and I'll add it. Policies with no knee cap row (e.g. claim 1) correctly fall through
to full SI — no branch needed there.

---

## Diagnostic — fetch the TKR benefit-cap data (run on a TKR policy)

Same shape as the cataract one. Replace `@ClaimID` with a TKR claim id.

```sql
DECLARE @ClaimID BIGINT = 0;  -- <<< a TKR claim id
DECLARE @MemberPolicyID INT =
    (SELECT TOP 1 MemberPolicyID FROM Claims WITH(NOLOCK) WHERE ID = @ClaimID);
SELECT @ClaimID AS ClaimID, @MemberPolicyID AS MemberPolicyID;

-- Is it coded, and with which knee procedure?
SELECT TOP 10 ID, TPAProcedureID, TPALevel1, TPALevel2, TPALevel3, ISNULL(Deleted,0) AS Deleted
FROM ClaimsCoding WITH(NOLOCK) WHERE ClaimID = @ClaimID ORDER BY ID DESC;

-- Any cap row whose TPAProcedureID lists a knee-replacement procedure (597/598/599/1342/1343),
-- or whose Remarks mention knee replacement / TKR / joint replacement.
SELECT
    bsc.BPSIID, bsc.ID AS BPSIConditionID,
    par.Name AS ParentName, c.Name AS ConditionName,
    bsc.isCovered, ISNULL(bsc.Deleted,0) AS Deleted,
    bsc.ClaimLimit, bsc.IndividualLimit, bsc.FamilyLimit, bsc.PolicyLimit, bsc.CorporateLimit,
    COALESCE(bsc.ClaimLimit, bsc.IndividualLimit, bsc.FamilyLimit, bsc.PolicyLimit) AS LimitAmt,
    bsc.TPAProcedureID, bsc.Remarks
FROM BPSIConditions bsc WITH(NOLOCK)
LEFT JOIN Mst_BPConditions c   WITH(NOLOCK) ON c.ID   = bsc.BPConditionID
LEFT JOIN Mst_BPConditions par WITH(NOLOCK) ON par.ID = c.ParentID
WHERE bsc.BPSIID IN (SELECT BPSIID FROM MemberSI WITH(NOLOCK)
                     WHERE MemberPolicyID = @MemberPolicyID AND ISNULL(Deleted,0)=0)
  AND ISNULL(bsc.Deleted,0) = 0
  AND (
        LOWER(bsc.Remarks) LIKE '%knee%'
     OR LOWER(bsc.Remarks) LIKE '%tkr%'
     OR LOWER(bsc.Remarks) LIKE '%arthroplasty%'
     OR LOWER(bsc.Remarks) LIKE '%joint replacement%'
     OR ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,597,%'
     OR ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,598,%'
     OR ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,599,%'
     OR ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,1342,%'
     OR ','+REPLACE(bsc.TPAProcedureID,' ','')+',' LIKE '%,1343,%'
  );
```

## Deploy
Apply Edit A (and Edit B once confirmed), build in Visual Studio, recycle the app pool.
