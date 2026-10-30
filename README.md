ps -eo comm= | sort -u | grep -iE 'mdatp|wdavdaemon|falcon|ds_agent|qualys|wazuh|ossec|sentinel|tanium|osquery|lacework|datadog|cbagent'

cat /etc/docker/daemon.json 2>/dev/null || echo "no file"

echo '{ "features": { "containerd-snapshotter": false } }' > /etc/docker/daemon.json
systemctl restart docker
docker info --format '{{.Driver}}'

docker network ls --filter name=edge

docker pull ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals

docker build -t claim-processing-stage:preauth-finals - < /home/ubuntu/claim-processing/Dockerfile.stage

