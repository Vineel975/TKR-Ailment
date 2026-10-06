docker compose logs web | grep -m1 "Startup config"
docker compose exec web printenv DOCKER_ENV IN_DOCKER CONVEX_SELF_HOSTED_URL
