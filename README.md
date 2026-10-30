cd /home/ubuntu/claim-processing
sed -i '/^CONVEX_SELF_HOSTED_ADMIN_KEY=/d' .env
grep -E '^(WEB_PORT|CONVEX_BACKEND_PORT|CONVEX_SITE_PORT)=' .env

docker pull ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals

docker run --rm --entrypoint sh ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals -c "grep -rhoE 'claims-backend-helixview[-a-z]*[.]fhpl[.]net' /app/.next | sort -u"

