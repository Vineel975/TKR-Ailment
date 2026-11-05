docker inspect claim-processing-web-1 --format 'image={{.Config.Image}}  id={{.Image}}  started={{.State.StartedAt}}'

docker image inspect claim-processing-stage:preauth-finals --format 'stage copy:   id={{.Id}}  created={{.Created}}'
docker image inspect ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals --format 'GitHub image: created={{.Created}}'

grep -n "FROM" Dockerfile.stage
grep -n "^NEXT_IMAGE" .env

