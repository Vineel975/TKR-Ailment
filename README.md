ubuntu@ip-10-11-2-214:~/claim-processing$ sudo docker compose ls -a
NAME                STATUS              CONFIG FILES
claim-processing    running(4)          /home/ubuntu/claim-processing/docker-compose.yml
ubuntu@ip-10-11-2-214:~/claim-processing$ sudo ss -ltnp | grep -E ':(80|443) '
LISTEN 0      511          0.0.0.0:443       0.0.0.0:*    users:(("nginx",pid=2067906,fd=7),("nginx",pid=2067905,fd=7),("nginx",pid=2067904,fd=7),("nginx",pid=2067903,fd=7),("nginx",pid=2067901,fd=7))
LISTEN 0      511          0.0.0.0:80        0.0.0.0:*    users:(("nginx",pid=2067906,fd=5),("nginx",pid=2067905,fd=5),("nginx",pid=2067904,fd=5),("nginx",pid=2067903,fd=5),("nginx",pid=2067901,fd=5))
LISTEN 0      511             [::]:443          [::]:*    users:(("nginx",pid=2067906,fd=8),("nginx",pid=2067905,fd=8),("nginx",pid=2067904,fd=8),("nginx",pid=2067903,fd=8),("nginx",pid=2067901,fd=8))
LISTEN 0      511             [::]:80           [::]:*    users:(("nginx",pid=2067906,fd=6),("nginx",pid=2067905,fd=6),("nginx",pid=2067904,fd=6),("nginx",pid=2067903,fd=6),("nginx",pid=2067901,fd=6))
ubuntu@ip-10-11-2-214:~/claim-processing$ sudo docker ps --format '{{.Names}}  {{.Image}}  {{.Ports}}'
claim-processing-web-1  ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor  127.0.0.1:3100->3000/tcp
claim-processing-backend-1  ghcr.io/get-convex/convex-backend:latest  127.0.0.1:3310->3210/tcp, 127.0.0.1:3311->3211/tcp
claim-processing-dashboard-1  ghcr.io/get-convex/convex-dashboard:latest  127.0.0.1:6792->6791/tcp
claim-processing-postgres-1  postgres:16-alpine  127.0.0.1:6544->5432/tcp
ubuntu@ip-10-11-2-214:~/claim-processing$ for d in $(sudo grep -hE '^(APP_DOMAIN|CONVEX_DOMAIN|CONVEX_SITE_DOMAIN)=' ~/claim-processing/.env | cut -d= -f2 | tr -d '"'); do getent hosts $d || echo "$d -> not found"; done
10.11.2.214     claims-helixview.fhpl.net
10.11.2.214     claims-backend-helixview.fhpl.net
10.11.2.214     claims-auth-helixview.fhpl.net
ubuntu@ip-10-11-2-214:~/claim-processing$ getent hosts claims-helixview-stage.fhpl.net claims-backend-helixview-stage.fhpl.net claims-auth-helixview-stage.fhpl.net || echo "stage names not found"
10.11.4.180     AWS-PreProd-Cashless-AI.fhpl.in claims-helixview-stage.fhpl.net
10.11.4.180     AWS-PreProd-Cashless-AI.fhpl.in claims-backend-helixview-stage.fhpl.net
10.11.4.180     AWS-PreProd-Cashless-AI.fhpl.in claims-auth-helixview-stage.fhpl.net
ubuntu@ip-10-11-2-214:~/claim-processing$ diff ~/claim-processing/docker-compose.yml.bak-2026-10-06 ~/claim-processing/docker-compose.yml
sudo ls -la ~/claim-processing/certs
16a17
>       - ./certs/godaddy-chain.pem:/etc/ssl/extra/godaddy-chain.pem:ro
52a54
>       - NODE_EXTRA_CA_CERTS=/etc/ssl/extra/godaddy-chain.pem
total 16
drwxr-xr-x 2 root   root   4096 Oct  6 09:49 .
drwxr-xr-x 4 ubuntu ubuntu 4096 Oct 26 19:10 ..
-rw-r--r-- 1 root   root   4163 Oct  6 09:49 godaddy-chain.pem
ubuntu@ip-10-11-2-214:~/claim-processing$ sudo ls -la ~/claim-processing/certs
total 16
drwxr-xr-x 2 root   root   4096 Oct  6 09:49 .
drwxr-xr-x 4 ubuntu ubuntu 4096 Oct 26 19:10 ..
-rw-r--r-- 1 root   root   4163 Oct  6 09:49 godaddy-chain.pem
ubuntu@ip-10-11-2-214:~/claim-processing$ grep -hE 'test.sh|ai-ana|network create|nginx|caddy' ~/.bash_history; sudo grep -hE 'test.sh|ai-ana|network create|nginx|caddy' /root/.bash_history
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxybash scripts/test.sh --proxybash scripts/test.sh --proxybash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxybash scripts/test.sh --proxybash scripts/test.sh --proxybash scripts/test.sh --proxybash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy\
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
sudo find /etc/nginx/conf.d/claims-helixview.conf
cat /etc/nginx/conf.d/claims-helixview.conf
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
cat test.sh
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
bash scripts/test.sh --proxy
