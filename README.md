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

    root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# grep -n "^NEXT_IMAGE" .env
21:NEXT_IMAGE="claim-processing-stage:preauth-finals"
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker build -t claim-processing-stage:preauth-finals - < Dockerfile.stage.coderefactor
[+] Building 2.0s (6/6) FINISHED                                                                         docker:default
 => [internal] load build definition from Dockerfile                                                               0.0s
 => => transferring dockerfile: 398B                                                                               0.0s
 => [internal] load metadata for ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor            0.0s
 => [internal] load .dockerignore                                                                                  0.0s
 => => transferring context: 2B                                                                                    0.0s
 => [1/2] FROM ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor                              0.2s
 => [2/2] RUN grep -rlE "claims-backend-helixview[.]fhpl[.]net|http://127[.]0[.]0[.]1:3210" /app/.next | xargs -r  1.3s
 => exporting to image                                                                                             0.3s
 => => exporting layers                                                                                            0.2s
 => => writing image sha256:f004c0f5140fb04278523b190069538a1cb81cce169b8a16257a68608727e210                       0.0s
 => => naming to docker.io/library/claim-processing-stage:preauth-finals                                           0.0s
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker compose up -d web
WARN[0000] service "web": deploy.labels: deploy.labels are only applied to Swarm services
WARN[0000] service "web": deploy.rollback_config: deploy.rollback_config only applies to rolling updates in Swarm mode
WARN[0000] service "web": deploy.update_config: deploy.update_config only applies to rolling updates in Swarm mode
[+] up 3/3
 ✔ Container claim-processing-backend-1  Running                                                                    0.0s
 ✔ Container claim-processing-postgres-1 Running                                                                    0.0s
 ✔ Container claim-processing-web-1      Started                                                                    1.8s
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker compose ps web
   docker exec claim-processing-web-1 printenv CLAIMAI_PREAUTH_FINALS
   docker exec claim-processing-web-1 sh -c 'grep -rl "CLAIMAI_PREAUTH_FINALS" /app/.next/server | wc -l'
   docker exec claim-processing-web-1 sh -c 'grep -rlE "claims-backend-helixview-stage[.]fhpl[.]net" /app/.next | wc -l'
   docker exec claim-processing-web-1 sh -c 'grep -rlE "claims-backend-helixview[.]fhpl[.]net" /app/.next | wc -l'
   docker exec claim-processing-web-1 node -e "fetch('http://localhost:3000/api/auto/run',{method:'POST'}).then(r=>console.log(r.status))"
NAME                     IMAGE                                   COMMAND                  SERVICE   CREATED          STATUS                    PORTS
claim-processing-web-1   claim-processing-stage:preauth-finals   "/bin/sh /entrypoint…"   web       49 seconds ago   Up 47 seconds (healthy)   127.0.0.1:3100->3000/tcp
on
1
24
0
401

   

    
