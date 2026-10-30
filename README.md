docker compose up -d postgres backend dashboard

until curl -fsS http://127.0.0.1:3310/version >/dev/null 2>&1; do sleep 2; done; echo "backend ready"

KEY=$(docker compose exec -T backend ./generate_admin_key.sh | grep '^convex-self-hosted|')
echo "CONVEX_SELF_HOSTED_ADMIN_KEY=$KEY" >> .env
grep -c '^CONVEX_SELF_HOSTED_ADMIN_KEY=convex-self-hosted|' .env

docker compose up -d web

docker compose logs -f --tail=50 web

docker compose ps
