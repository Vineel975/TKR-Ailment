printf 'FROM ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals\nRUN grep -rlE "claims-backend-helixview[.]fhpl[.]net|http://127[.]0[.]0[.]1:3210" /app/.next | xargs -r sed -i -e "s#claims-backend-helixview[.]fhpl[.]net#claims-backend-helixview-stage.fhpl.net#g" -e "s#http://127[.]0[.]0[.]1:3210#https://claims-backend-helixview-stage.fhpl.net#g"\n' > /home/ubuntu/claim-processing/Dockerfile.stage


docker build -t claim-processing-stage:preauth-finals - < /home/ubuntu/claim-processing/Dockerfile.stage

docker run --rm --entrypoint sh claim-processing-stage:preauth-finals -c "grep -rhoE 'https?://[a-z0-9.-]*(helixview|127[.]0[.]0[.]1)[a-z0-9.:-]*' /app/.next | sort | uniq -c"
