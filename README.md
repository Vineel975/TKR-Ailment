cp app/api/audit/start/route.ts /home/ubuntu/route.ts.bak-$(date +%F)
mv /home/ubuntu/route.ts app/api/audit/start/route.ts
grep -n "toInternalUploadUrl" app/api/audit/start/route.ts
docker compose exec web printenv CONVEX_SELF_HOSTED_URL
docker compose up -d --build web


root@ip-10-11-2-214:/home/ubuntu/claim-processing# cp app/api/audit/start/route.ts /home/ubuntu/route.ts.bak-$(date +%F)
cp: cannot stat 'app/api/audit/start/route.ts': No such file or directory


ls
grep -nE "^\s*(image|build):" docker-compose.yml
find / -path /proc -prune -o -path "*/app/api/audit/start/route.ts" -print 2>/dev/null | grep -v node_modules | head
