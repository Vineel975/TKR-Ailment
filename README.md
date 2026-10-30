docker compose up -d postgres backend dashboard

until curl -fsS http://127.0.0.1:3310/version >/dev/null 2>&1; do sleep 2; done; echo "backend ready"

KEY=$(docker compose exec -T backend ./generate_admin_key.sh | grep '^convex-self-hosted|')
echo "CONVEX_SELF_HOSTED_ADMIN_KEY=$KEY" >> .env
grep -c '^CONVEX_SELF_HOSTED_ADMIN_KEY=convex-self-hosted|' .env

docker compose up -d web

docker compose logs -f --tail=50 web


docker compose ps


uname -m
sudo docker image inspect ghcr.io/get-convex/convex-backend:latest ghcr.io/get-convex/convex-dashboard:latest --format '{{index .RepoDigests 0}}'

for r in PASTE_BACKEND_LINE PASTE_DASHBOARD_LINE; do docker pull "$r" && docker tag "$r" "${r%@*}:latest"; done

docker pull postgres:16-alpine

docker images --format '{{.Repository}}:{{.Tag}}'


cd /home/ubuntu/claim-processing
docker compose up -d postgres backend dashboard




