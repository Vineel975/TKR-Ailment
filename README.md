cp app/api/audit/start/route.ts /home/ubuntu/route.ts.bak-$(date +%F)
mv /home/ubuntu/route.ts app/api/audit/start/route.ts
grep -n "toInternalUploadUrl" app/api/audit/start/route.ts
docker compose exec web printenv CONVEX_SELF_HOSTED_URL
docker compose up -d --build web
