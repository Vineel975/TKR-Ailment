docker run --rm --entrypoint sh claim-processing-stage:preauth-finals -c "grep -rhoE 'https?://[a-z0-9.-]*(helixview|127[.]0[.]0[.]1)[a-z0-9.:-]*' /app/.next | sort | uniq -c"
