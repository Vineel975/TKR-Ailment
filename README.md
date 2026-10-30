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


sudo docker save --platform linux/arm64 -o /tmp/convex-images.tar ghcr.io/get-convex/convex-backend:latest ghcr.io/get-convex/convex-dashboard:latest
sudo chown ubuntu:ubuntu /tmp/convex-images.tar && gzip /tmp/convex-images.tar
ls -lh /tmp/convex-images.tar.gz


scp -i "C:\Users\satyavineel.k\Downloads\AWS-Prod-Cashless-AI-KP.pem" ubuntu@10.11.2.214:/tmp/convex-images.tar.gz "C:\Users\satyavineel.k\Desktop\convex-images.tar.gz"

scp -i "C:\Users\satyavineel.k\Desktop\KP-AWS-PreProd-Cashless-AI.pem" "C:\Users\satyavineel.k\Desktop\convex-images.tar.gz" ubuntu@10.11.4.180:/tmp/

docker load -i /tmp/convex-images.tar.gz
docker images --format '{{.Repository}}:{{.Tag}}' | grep get-convex









