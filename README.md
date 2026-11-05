   IMG=ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
   STAGE=claim-processing-stage:preauth-finals

      docker tag $STAGE claim-processing-stage:previous

         docker pull $IMG

            sed 's#next-convex:preauth-finals#next-convex:feature-coderefactor#g' Dockerfile.stage > Dockerfile.stage.coderefactor
   grep -n "FROM" Dockerfile.stage.coderefactor

      docker build -t $STAGE - < Dockerfile.stage.coderefactor

         P='[a-z-]*helixview[-a-z]*[.]fhpl[.]net|127[.]0[.]0[.]1:3210'
   docker run --rm --entrypoint grep $STAGE -rhoIE "$P" /app/.next | sort -u

       docker compose up -d web
    docker compose logs -f --tail=50 web

        docker exec claim-processing-web-1 sh -c 'ls /app/.next/server/app/api/auto /app/.next/server/app/api/checks'
    docker exec claim-processing-web-1 node -e "fetch('http://localhost:3000/api/auto/run',{method:'POST'}).then(r=>console.log(r.status))"

    
