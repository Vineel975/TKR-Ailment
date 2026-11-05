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



root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# CLAIM=26110544142; docker logs -t claim-processing-web-1 --since 24h 2>&1 | grep -E "autoDecision claim $CLAIM|auto-run.*$CLAIM|adjudicate.*$CLAIM|coding-shadow.*$CLAIM"
2026-11-05T10:31:55.809862563Z [staging/result] autoDecision claim 26110544142 -> refer_crm | amount 24000 | basis tariff | Patient age differs from the policy by more than 5 years.
2026-11-05T10:32:00.690225427Z [auto-run] on claim 26110544142/1: doctor (crm), saved - saved; claim 26110544142/1: crm - not done: Spectra did not complete the action - see results.; Adjudication_Actions_Insert: Procedure or function 'USP_ClaimReason_Insert' expects parameter '@GroundofRepudiation', which was not supplied.; open stages now 5  [reason CRM Clarification; pre-fill on the claim]
