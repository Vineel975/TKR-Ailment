
CLAIM=26110544147; docker logs -t claim-processing-web-1 --since 24h 2>&1 | grep -E "autoDecision claim $CLAIM|auto-run.*$CLAIM|adjudicate.*$CLAIM|coding-shadow.*$CLAIM"


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


AUTO APPROVAL CEILING UPTO Rs.50000/-  for Cataract, After AI Engine rules, If Amount amissible Above 50K than the case should be return to Doctor, If Admissible amount is Less than 50K than Auto approval should go.


free -h
docker stats --no-stream --format "table {{.Name}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.CPUPerc}}"
ps aux --sort=-%mem | head -12
dmesg -T | grep -i -E "out of memory|killed process" | tail -5

  docker compose up -d --force-recreate web
  docker compose logs -f --tail=30 web

    docker compose restart backend

     CLAIM=26112144190
   docker logs claim-processing-web-1 2>&1 | grep "$CLAIM" | grep -E "flow=|final approvals|autoDecision|auto-run"


      cp .env .env.bak-$(date +%Y%m%d-%H%M)

         grep '^RULES_GITHUB_BASE_BRANCH=' .env
   grep -c '^RULES_AGENT_ENABLED=' .env
   grep -c '^RULES_ASSIST_ENABLED=' .env
   docker compose exec web sh -c 'test -n "$OPENROUTER_API_KEY" && echo key-set || echo key-missing'

      grep -E '^RULES_GITHUB_BASE_BRANCH=|^RULES_AGENT_ENABLED=|^RULES_ASSIST_ENABLED=' .env

   CLAIM=26111044157
   docker logs claim-processing-web-1 2>&1 | grep "$CLAIM" | grep -E "flow=|FINAL|final approvals|autoDecision|auto-run" | tail -20

    cd /home/ubuntu/claim-processing
   sed -i 's#^RULES_GITHUB_BASE_BRANCH=.*#RULES_GITHUB_BASE_BRANCH=feature/coderefactor#' .env
   grep '^RULES_GITHUB_BASE_BRANCH=' .env<img width="1841" height="710" alt="image" src="https://github.com/user-attachments/assets/1aa14d07-0729-4280-8811-f03af0a450dc" />

   docker compose up -d --force-recreate web

/* Automatic-run status for chosen claims. Read-only.
   Put the claim IDs below, one per row: (ClaimID, SlNo). SlNo is usually 1. */
DECLARE @Claims TABLE (ClaimID bigint, SlNo int);
INSERT INTO @Claims (ClaimID, SlNo) VALUES
    (26112444211, 1),
    (26112144187, 1);
    -- add more: , (ClaimID, SlNo)

SELECT c.ClaimID, c.SlNo,
       e.CreatedAt                                  AS RunAt,
       ISNULL(e.AIValue, '(no AUTO_RUN row)')       AS Outcome,     -- done / doctor / skipped / error / dry
       e.FieldName                                  AS Action,
       e.UserValue                                  AS Message,     -- why, or the error
       r.ProcessingStatus                           AS StagingStatus,
       JSON_VALUE(r.J, '$.autoDecision.action')     AS DecidedAction,
       JSON_VALUE(r.J, '$.autoDecision.amount')     AS DecidedAmount,
       JSON_VALUE(r.J, '$.autoDecision.basis')      AS DecidedBasis,
       JSON_VALUE(r.J, '$.autoDecision.reasons[0]') AS DecisionReason
FROM @Claims c
LEFT JOIN dbo.ClaimAI_EventLog e WITH (NOLOCK)
       ON e.ClaimID = c.ClaimID AND ISNULL(e.SlNo, 1) = c.SlNo AND e.EventType = 'AUTO_RUN'
OUTER APPLY (SELECT TOP 1 x.ProcessingStatus,
                    CASE WHEN ISJSON(x.AnalysisJson) = 1 THEN x.AnalysisJson END AS J
             FROM dbo.ClaimAI_Results x WITH (NOLOCK)
             WHERE x.ClaimID = c.ClaimID AND ISNULL(x.SlNo, 1) = c.SlNo
             ORDER BY x.ID DESC) r
ORDER BY c.ClaimID, c.SlNo, e.CreatedAt DESC;


SELECT c.ID AS ClaimID, p.PolicyNo, mc.Name AS CorporateName
FROM dbo.Claims c WITH (NOLOCK)
LEFT JOIN dbo.MemberPolicy mp WITH (NOLOCK) ON mp.ID = c.MemberPolicyID
LEFT JOIN dbo.Policy p WITH (NOLOCK) ON p.ID = mp.PolicyID
LEFT JOIN dbo.Mst_Corporate mc WITH (NOLOCK) ON mc.ID = mp.CorpID
WHERE c.ID IN (26112744386, 26112544286, <an SBI corporate claim ID>)
  AND ISNULL(c.Deleted, 0) = 0;



