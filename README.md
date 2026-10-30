sudo docker compose ls -a
sudo ss -ltnp | grep -E ':(80|443) '
sudo docker ps --format '{{.Names}}  {{.Image}}  {{.Ports}}'
for d in $(sudo grep -hE '^(APP_DOMAIN|CONVEX_DOMAIN|CONVEX_SITE_DOMAIN)=' ~/claim-processing/.env | cut -d= -f2 | tr -d '"'); do getent hosts $d || echo "$d -> not found"; done
getent hosts claims-helixview-stage.fhpl.net claims-backend-helixview-stage.fhpl.net claims-auth-helixview-stage.fhpl.net || echo "stage names not found"
diff ~/claim-processing/docker-compose.yml.bak-2026-10-06 ~/claim-processing/docker-compose.yml
sudo ls -la ~/claim-processing/certs
grep -hE 'test.sh|ai-ana|network create|nginx|caddy' ~/.bash_history; sudo grep -hE 'test.sh|ai-ana|network create|nginx|caddy' /root/.bash_history
