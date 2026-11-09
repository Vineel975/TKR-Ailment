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

     CLAIM=26110644150
   docker logs claim-processing-web-1 2>&1 | grep "$CLAIM" | grep -E "flow=|final approvals|autoDecision|auto-run"

   CLAIM=26110644150
   docker logs claim-processing-web-1 2>&1 | grep "$CLAIM" | grep -E "flow=|FINAL|final approvals|autoDecision|auto-run" | tail -20

        root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing#  df -h /
Filesystem      Size  Used Avail Use% Mounted on
/dev/root        96G   20G   76G  21% /
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# IMG=ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing#    STAGE=claim-processing-stage:preauth-finals
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker tag $STAGE claim-processing-stage:previous
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker pull $IMG
feature-coderefactor: Pulling from adityamiskin/claim-processing/next-convex
2d66c782e484: Already exists
e1fa61ef5c24: Already exists
a044f3bd5c10: Already exists
33ea1952272f: Already exists
b8da3bb2ce20: Already exists
e6b8ab86c56c: Already exists
defadd81c7b1: Pull complete
8e56f62ce8d4: Pull complete
742375da732b: Pull complete
83dd94a93b02: Pull complete
7cec3a65f1d7: Pull complete
50431493345b: Pull complete
43e213ba6aa6: Pull complete
bf8aacbeca0a: Pull complete
5f52159909da: Pull complete
bb55c68c3dac: Pull complete
d93d6b9c211f: Pull complete
5aa141b8c65c: Pull complete
Digest: sha256:ede25eeda1ef984638d9d1b611b61d6304bf6ce8e926b85f1b7b25c575510514
Status: Downloaded newer image for ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker build -t $STAGE - < Dockerfile.stage.coderefactor
[+] Building 1.5s (6/6) FINISHED                         docker:default
 => [internal] load build definition from Dockerfile               0.0s
 => => transferring dockerfile: 398B                               0.0s
 => [internal] load metadata for ghcr.io/adityamiskin/claim-proce  0.0s
 => [internal] load .dockerignore                                  0.0s
 => => transferring context: 2B                                    0.0s
 => [1/2] FROM ghcr.io/adityamiskin/claim-processing/next-convex:  0.2s
 => [2/2] RUN grep -rlE "claims-backend-helixview[.]fhpl[.]net|ht  0.6s
 => exporting to image                                             0.1s
 => => exporting layers                                            0.1s
 => => writing image sha256:20b4884c6b72acac3d3766c797ece68c2532d  0.0s
 => => naming to docker.io/library/claim-processing-stage:preauth  0.0s
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# P='[a-z-]*helixview[-a-z]*[.]fhpl[.]net|127[.]0[.]0[.]1:3210'
   docker run --rm --entrypoint grep $STAGE -rhoIE "$P" /app/.next | sort -u
   docker run --rm --entrypoint cat $STAGE /app/BUILD_COMMIT
claims-backend-helixview-stage.fhpl.net
claims-helixview-stage.fhpl.net
claims-helixview.fhpl.net
15a206a21d3c49265712b17361716058cd01397f
