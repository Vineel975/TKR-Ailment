sudo du -xh --max-depth=1 /var/lib | sort -h | tail

docker image prune -a -f      # unused images — usually the biggest win by far
docker builder prune -a -f    # build cache
docker container prune -f     # stopped containers
