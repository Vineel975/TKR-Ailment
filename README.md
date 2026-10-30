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



mkdir -p /root/anon-docker
for r in ghcr.io/get-convex/convex-backend@sha256:ed68a48786ab5d83aaddf22b86d9d06b146a530957951dd77f541c216b762224 ghcr.io/get-convex/convex-dashboard@sha256:4dd0dd03a4f9d83403c0e895db70dfe868e99b311465a0a05ed0bd6fdcfe923f; do DOCKER_CONFIG=/root/anon-docker docker pull "$r" && docker tag "$r" "${r%@*}:latest"; done

docker pull postgres:16-alpine

cd /home/ubuntu/claim-processing
docker compose up -d postgres backend dashboard


root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# mkdir -p /root/anon-docker
for r in ghcr.io/get-convex/convex-backend@sha256:ed68a48786ab5d83aaddf22b86d9d06b146a530957951dd77f541c216b762224 ghcr.io/get-convex/convex-dashboard@sha256:4dd0dd03a4f9d83403c0e895db70dfe868e99b311465a0a05ed0bd6fdcfe923f; do DOCKER_CONFIG=/root/anon-docker docker pull "$r" && docker tag "$r" "${r%@*}:latest"; done
Error response from daemon: Get "https://ghcr.io/v2/get-convex/convex-backend/manifests/sha256:ed68a48786ab5d83aaddf22b86d9d06b146a530957951dd77f541c216b762224": denied
Error response from daemon: Get "https://ghcr.io/v2/get-convex/convex-dashboard/manifests/sha256:4dd0dd03a4f9d83403c0e895db70dfe868e99b311465a0a05ed0bd6fdcfe923f": denied








