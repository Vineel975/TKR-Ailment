docker run --rm --entrypoint sh ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals -c "grep -rhoE 'claims-backend-helixview[-a-z]*[.]fhpl[.]net' /app/.next | sort -u"
