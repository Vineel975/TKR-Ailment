cd /home/ubuntu/claim-processing
docker compose exec web node -e "fetch('https://openrouter.ai/api/v1/models').then(r=>console.log('openrouter', r.status)).catch(e=>console.log('openrouter FAILED', e.cause?.code || e.message))"


grep -q '^DISABLE_BEACON=' .env || echo 'DISABLE_BEACON=true' >> .env
grep '^DISABLE_BEACON=' .env

docker compose up -d backend
docker compose exec backend printenv DISABLE_BEACON
