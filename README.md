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

   root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# df -h /
Filesystem      Size  Used Avail Use% Mounted on
/dev/root        96G   19G   78G  20% /
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker pull ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
feature-coderefactor: Pulling from adityamiskin/claim-processing/next-convex
2d66c782e484: Already exists
e1fa61ef5c24: Already exists
a044f3bd5c10: Already exists
33ea1952272f: Already exists
b8da3bb2ce20: Already exists
e6b8ab86c56c: Already exists
cce5e5c639c5: Pull complete
c7bcca0ba5c0: Pull complete
b5706c861090: Pull complete
3cd7d58f6d42: Pull complete
12141004e33f: Pull complete
a2cf7efbc6d6: Pull complete
84b0a62f1d10: Pull complete
5a968682f857: Pull complete
a06d4bb4561e: Pull complete
239ee7219094: Pull complete
a5907b13d718: Pull complete
Digest: sha256:c8875ae9c3731edcc0b7a2e52a0e16edcefea6e92cb974dc1c3804a8deb48eb0
Status: Downloaded newer image for ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# head -3 Dockerfile.stage.coderefactor
FROM ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
RUN grep -rlE "claims-backend-helixview[.]fhpl[.]net|http://127[.]0[.]0[.]1:3210" /app/.next | xargs -r sed -i -e "s#claims-backend-helixview[.]fhpl[.]net#claims-backend-helixview-stage.fhpl.net#g" -e "s#http://127[.]0[.]0[.]1:3210#https://claims-backend-helixview-stage.fhpl.net#g"
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing#  grep -n "image:" docker-compose.yml
4:    image: ghcr.io/get-convex/convex-backend:latest
66:    image: ghcr.io/get-convex/convex-dashboard:latest
76:    image: postgres:16-alpine
100:    image: ${NEXT_IMAGE:-claim-processing-local}
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing#    docker inspect claim-processing-web-1 --format '{{.Config.Image}}'
claim-processing-stage:preauth-finals
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing#

    
