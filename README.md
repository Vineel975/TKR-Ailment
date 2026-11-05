CLAIM=26110544142; docker logs -t claim-processing-web-1 --since 24h 2>&1 | grep -E "autoDecision claim $CLAIM|auto-run.*$CLAIM|adjudicate.*$CLAIM|coding-shadow.*$CLAIM"


CLAIMAI_AUTO_INSURER_CONTAINS=*
CLAIMAI_AUTO_BILL_LOWEST=off


   IMG=ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
   STAGE=claim-processing-stage:preauth-finals

      docker tag $STAGE claim-processing-stage:previous

         docker pull $IMG
   docker image inspect $IMG --format 'built: {{.Created}}'

      docker build --no-cache -t $STAGE - < Dockerfile.stage.coderefactor

         P='[a-z-]*helixview[-a-z]*[.]fhpl[.]net|127[.]0[.]0[.]1:3210'
   docker run --rm --entrypoint grep $STAGE -rhoIE "$P" /app/.next | sort -u

      docker run --rm --entrypoint sh $STAGE -c '
   echo "1 automatic run (phase 5): $(ls -d /app/.next/server/app/api/auto/run >/dev/null 2>&1 && echo YES || echo NO)";
   echo "2 SBI scope code:          $(grep -rlq --include=*.js CLAIMAI_AUTO_INSURER_CONTAINS /app/.next/server && echo YES || echo NO)";
   echo "3 lens query removed:      $(grep -rlq --include=*.js "Kindly clarify the type of lens used" /app/.next && echo NO || echo YES)"'



      docker compose up -d --force-recreate web
   docker compose logs -f --tail=50 web

       docker compose ps
    docker image prune -f

    docker exec claim-processing-web-1 sh -c '
echo "1 automatic run (phase 5): $(ls -d /app/.next/server/app/api/auto/run >/dev/null 2>&1 && echo YES || echo NO)";
echo "2 SBI scope code:          $(grep -rlq --include=*.js CLAIMAI_AUTO_INSURER_CONTAINS /app/.next/server && echo YES || echo NO)";
echo "3 lens query removed:      $(grep -rlq --include=*.js "Kindly clarify the type of lens used" /app/.next && echo NO || echo YES)";
echo "   CLAIMAI_AUTO_RUN=$(printenv CLAIMAI_AUTO_RUN)  USER_REGION_ID=$(printenv CLAIMAI_AUTO_USER_REGION_ID)";
echo "   INSURER_CONTAINS=$(printenv CLAIMAI_AUTO_INSURER_CONTAINS)  BILL_LOWEST=$(printenv CLAIMAI_AUTO_BILL_LOWEST)"'


   docker tag claim-processing-stage:previous claim-processing-stage:preauth-finals
   docker compose up -d web
   docker compose logs -f --tail=50 web



curl -s -o /dev/null -w "app: %{http_code}\n" http://127.0.0.1:3000/login

curl -s -o /dev/null -w "site: %{http_code}\n" https://claims-helixview-stage.fhpl.net/login

curl -s -w "\nconvex: %{http_code}\n" http://127.0.0.1:3210/version

   docker compose logs -t --since 1h web 2>&1 | grep -E "runTariffMatching completed|Tariff matching FAILED"


<img width="452" height="366" alt="image" src="https://github.com/user-attachments/assets/ae42bed9-7fc8-41ae-a589-891469f84731" />
web-1  | [audit/start] autoSavePayloads built: {"approvedAccommodation":{"type":"setApprovedAccommodation","facilityId":null,"claimType":"cataract","defaultFacilityId":"195"},"clinical":{"type":"setClinicalDetails","diagnosis":"Cataract","lineOfTreatment":"MICS c lens implant in left eye","presentingComplaint":"A 50-year-old male patient presents with Cataract in the left eye, admitted for surgical management via MICS with lens implantation.","processingRemarks":"Cataract covered up to Rs.60,000/- per eye. Lens covered up to Rs.30,000/-.","doctorNotes":"Covered for active Surgical management of MICS c lens implant in left eye\nNo copay is applicable.\nRs. 60,000 has been approved.\nRestricted to ailment limit as per policy |FT|","hospTreatmentKeyword":"surgical","procedureHint":"cataract mics c lens implant in left eye","eligibleAmount":60000,"packageAmount":71700},"billing":{"type":"setBillingDetails","hospitalBillAmount":71700,"tariffAmount":0,"totalAmountApproved":60000,"approvalBasis":"benefit","ailmentSublimitAmount":60000},"icd":{"type":"setIcdSlots","procedureHint":"cataract mics c lens implant in left eye","eligibleAmount":60000,"packageAmount":71700,"lastIcdCode":"H25.9","resolvedTpaProcedureName":"MICS -Unifocal (Lens Included)-Left"}}
web-1  | [audit/start] autoApproval: {"eligible":false,"minAmount":60000,"approvalBasis":"benefit","approvedAmount":60000,"actionType":"approve","threshold":50000,"reason":"basis 'benefit' pre-selected; min(60000) >= threshold(50000), doctor reviews.","billingState":{"approvalBasis":"benefit","manualBillAmount":"","ailmentSublimitAmount":60000,"autoApproved":false}}
web-1  | [audit/start] pre-seeded billingState for claim 26110544145 basis: benefit
web-1  | [audit/start] Triggering runTariffMatching for jobId: jn7a52genxhnds3tzn7d231tq98hmtgb tariffStorageId: kg26pfff1s9v2k1fvjkhc39hhh8hn4nz
web-1  | [CONVEX A(processPdf:runTariffMatching)] [LOG] '[ai-usage] {"op":"tariff-extraction","model":"google/gemini-3-flash-preview","claimId":"26110544145","cache":"none","ms":15313,"in":62290,"out":565,"reasoning":0,"cachedIn":0,"ok":true}'
web-1  | [CONVEX A(processPdf:runTariffMatching)] [LOG] '[ai-usage] {"op":"benefit-amount","model":"google/gemini-3-flash-preview","claimId":"26110544145","cache":"none","ms":1709,"in":1838,"out":123,"reasoning":0,"cachedIn":0,"ok":true}'
web-1  | [audit/start] runTariffMatching completed
web-1  | [staging/result] autoDecision claim 26110544145 -> query | amount 15000 | basis tariff | Mandatory documents missing: Kindly submit A-scan/Biometry Report.
web-1  | [auto-run] on claim 26110544145/1: done (query), saved - save: Hospital Estimate 71700, approved 60000 (tariff), procedure 503 MICS -Unifocal (Lens Included)-Left, ICD H25.9; claim 26110544145/1: query - DONE; Save_QueryPendingDetails:  ;  ; Epreauth status updated; open stages now 7  [1 query row(s); pre-fill on the claim]

