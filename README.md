sudo grep -E 'ghcr|credsStore|credHelpers' /root/.docker/config.json

sudo cp /root/.docker/config.json /tmp/docker-config.json && sudo chown ubuntu:ubuntu /tmp/docker-config.json && chmod 600 /tmp/docker-config.json

scp -i "C:\Users\satyavineel.k\Downloads\AWS-Prod-Cashless-AI-KP.pem" ubuntu@10.11.2.214:/tmp/docker-config.json "C:\Users\satyavineel.k\Desktop\docker-config.json"

scp -i "C:\Users\satyavineel.k\Desktop\KP-AWS-PreProd-Cashless-AI.pem" "C:\Users\satyavineel.k\Desktop\docker-config.json" ubuntu@10.11.4.180:/tmp/

mkdir -p /root/.docker && install -m 600 -o root -g root /tmp/docker-config.json /root/.docker/config.json && rm /tmp/docker-config.json

Prod: rm /tmp/docker-config.json
Laptop: del "C:\Users\satyavineel.k\Desktop\docker-config.json"

docker pull ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals
