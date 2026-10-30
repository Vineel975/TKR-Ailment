docker compose up -d postgres backend dashboard

until curl -fsS http://127.0.0.1:3310/version >/dev/null 2>&1; do sleep 2; done; echo "backend ready"

KEY=$(docker compose exec -T backend ./generate_admin_key.sh | grep '^convex-self-hosted|')
echo "CONVEX_SELF_HOSTED_ADMIN_KEY=$KEY" >> .env
grep -c '^CONVEX_SELF_HOSTED_ADMIN_KEY=convex-self-hosted|' .env

docker compose up -d web

docker compose logs -f --tail=50 web

docker compose ps


root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker compose up -d postgres backend dashboard
WARN[0000] service "web": deploy.labels: deploy.labels are only applied to Swarm services
WARN[0000] service "web": deploy.rollback_config: deploy.rollback_config only applies to rolling updates in Swarm mode
WARN[0000] service "web": deploy.update_config: deploy.update_config only applies to rolling updates in Swarm mode
[+] up 3/3
 ! Image postgres:16-alpine                         Interrupted     1.0s
 ✘ Image ghcr.io/get-convex/convex-backend:latest   Error manif...  1.0s
 ! Image ghcr.io/get-convex/convex-dashboard:latest Interrupted     1.0s
Error response from daemon: manifest unknown
Error response from daemon: No such image: ghcr.io/get-convex/convex-dashboard:latest
Error response from daemon: No such image: postgres:16-alpine
