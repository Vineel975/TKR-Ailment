CLAIM=26110544147; docker logs -t claim-processing-web-1 --since 24h 2>&1 | grep -E "autoDecision claim $CLAIM|auto-run.*$CLAIM|adjudicate.*$CLAIM|coding-shadow.*$CLAIM"


CLAIMAI_AUTO_INSURER_CONTAINS=*
CLAIMAI_AUTO_BILL_LOWEST=off


   IMG=ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
   STAGE=claim-processing-stage:preauth-finals

      docker tag $STAGE claim-processing-stage:previous

         docker pull $IMG
   docker image inspect $IMG --format 'built: {{.Created}}'

      docker build --no-cache -t $STAGE - < Dockerfile.stage.coderefactor

         P='[a-z-]*helixview[-a-z]*[.]fhpl[.]net|127[.]0[.]0[.]1:3210'
   docker run --rm --entrypoint grep $STAGE -rhoIE "$P" /app/.next | sort -u

      docker run --rm --entrypoint sh $STAGE -c '
   echo "1 automatic run (phase 5): $(ls -d /app/.next/server/app/api/auto/run >/dev/null 2>&1 && echo YES || echo NO)";
   echo "2 SBI scope code:          $(grep -rlq --include=*.js CLAIMAI_AUTO_INSURER_CONTAINS /app/.next/server && echo YES || echo NO)";
   echo "3 lens query removed:      $(grep -rlq --include=*.js "Kindly clarify the type of lens used" /app/.next && echo NO || echo YES)"'



      docker compose up -d --force-recreate web
   docker compose logs -f --tail=50 web

       docker compose ps
    docker image prune -f

    docker exec claim-processing-web-1 sh -c '
echo "1 automatic run (phase 5): $(ls -d /app/.next/server/app/api/auto/run >/dev/null 2>&1 && echo YES || echo NO)";
echo "2 SBI scope code:          $(grep -rlq --include=*.js CLAIMAI_AUTO_INSURER_CONTAINS /app/.next/server && echo YES || echo NO)";
echo "3 lens query removed:      $(grep -rlq --include=*.js "Kindly clarify the type of lens used" /app/.next && echo NO || echo YES)";
echo "   CLAIMAI_AUTO_RUN=$(printenv CLAIMAI_AUTO_RUN)  USER_REGION_ID=$(printenv CLAIMAI_AUTO_USER_REGION_ID)";
echo "   INSURER_CONTAINS=$(printenv CLAIMAI_AUTO_INSURER_CONTAINS)  BILL_LOWEST=$(printenv CLAIMAI_AUTO_BILL_LOWEST)"'





   docker tag claim-processing-stage:previous claim-processing-stage:preauth-finals
   docker compose up -d web
   docker compose logs -f --tail=50 web



curl -s -o /dev/null -w "app: %{http_code}\n" http://127.0.0.1:3000/login

curl -s -o /dev/null -w "site: %{http_code}\n" https://claims-helixview-stage.fhpl.net/login

curl -s -w "\nconvex: %{http_code}\n" http://127.0.0.1:3210/version

   docker compose logs -t --since 1h web 2>&1 | grep -E "runTariffMatching completed|Tariff matching FAILED"


AUTO APPROVAL CEILING UPTO Rs.50000/-  for Cataract, After AI Engine rules, If Amount amissible Above 50K than the case should be return to Doctor, If Admissible amount is Less than 50K than Auto approval should go.


free -h
docker stats --no-stream --format "table {{.Name}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.CPUPerc}}"
ps aux --sort=-%mem | head -12
dmesg -T | grep -i -E "out of memory|killed process" | tail -5

  docker compose up -d --force-recreate web
  docker compose logs -f --tail=30 web

    docker compose restart backend

    fallocate -l 4G /swapfile
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile
echo '/swapfile none swap sw 0 0' >> /etc/fstab
free -h


    curl --noproxy '*' -s -o /dev/null -w "%{http_code}\n" http://127.0.0.1:3000/login

    

Microsoft Windows [Version 10.0.26200.8039]
(c) Microsoft Corporation. All rights reserved.

C:\Users\satyavineel.k>

C:\Users\satyavineel.k>ssh -i "C:\Users\satyavineel.k\Desktop\KP-AWS-PreProd-Cashless-AI.pem" -p 22 ubuntu@10.11.4.180
Welcome to Ubuntu 26.04.1 LTS (GNU/Linux 7.0.0-1016-aws aarch64)

 * Documentation:  https://docs.ubuntu.com
 * Management:     https://landscape.canonical.com
 * Support:        https://ubuntu.com/pro

 System information as of Thu Nov  5 17:03:05 UTC 2026

  System load:  0.26               Temperature:           -273.1 C
  Usage of /:   14.2% of 95.82GB   Processes:             286
  Memory usage: 88%                Users logged in:       0
  Swap usage:   0%                 IPv4 address for ens5: 10.11.4.180

  => There is 1 zombie process.


Expanded Security Maintenance for Applications is not enabled.

7 updates can be applied immediately.
3 of these updates are standard security updates.
To see these additional updates run: apt list --upgradable

Enable ESM Apps to receive additional future security updates.
See https://ubuntu.com/esm or run: sudo pro status


Last login: Thu Nov  5 17:43:28 2026 from 10.10.96.34
ubuntu@AWS-PreProd-Cashless-AI:~$ cd claim-processing/
ubuntu@AWS-PreProd-Cashless-AI:~/claim-processing$ sudo su
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# free -h
docker stats --no-stream --format "table {{.Name}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.CPUPerc}}"
ps aux --sort=-%mem | head -12
dmesg -T | grep -i -E "out of memory|killed process" | tail -5
               total        used        free      shared  buff/cache   available
Mem:           3.7Gi       2.5Gi       659Mi        23Mi       777Mi       1.2Gi
Swap:          4.0Gi          0B       4.0Gi
NAME                           MEM USAGE / LIMIT     MEM %     CPU %
claim-processing-web-1         109.2MiB / 3.734GiB   2.86%     0.00%
claim-processing-backend-1     28.31MiB / 3.734GiB   0.74%     3.40%
claim-processing-dashboard-1   76.34MiB / 3.734GiB   2.00%     0.00%
claim-processing-postgres-1    36.14MiB / 3.734GiB   0.95%     3.54%
USER         PID %CPU %MEM    VSZ   RSS TTY      STAT START   TIME COMMAND
sophos-+  329801 17.8 20.4 1327048 798796 ?      SNl  17:37   1:34 /opt/sophos-spl/plugins/av/sbin/sophos_threat_detector
sophos-+    1408  0.8  2.9 1434732 115440 ?      SNl  Oct30  76:53 /opt/sophos-spl/plugins/runtimedetections/bin/runtimedetections.0
root       25038  0.5  2.8 2567512 111508 ?      Ssl  Oct30  47:38 /usr/bin/dockerd -H fd:// --containerd=/run/containerd/containerd.sock
root      333390  1.3  2.5 11586660 99204 ?      Ssl  17:44   0:01 next-server (v
root      335067  0.4  2.0 18111716 79936 ?      Ssl  17:45   0:00 ./convex-local-backend --instance-name convex-self-hosted --instance-secret d5c62829f135e8fb6f20b6f36f9904c6988a71574b79285938987d363d02a708 --port 3210 --site-proxy-port 3211 --convex-origin https://claims-backend-helixview-stage.fhpl.net --convex-site https://claims-auth-helixview-stage.fhpl.net --beacon-tag self-hosted-docker --disable-beacon --do-not-require-ssl --local-storage /convex/data/storage /convex/data/db.sqlite3
ssm-user   29395  0.0  1.8 11589512 74248 ?      Ssl  Oct30   0:06 next-server (v15
root     2422134  0.0  1.7 523408 68040 ?        Ssl  Nov04   0:07 /usr/libexec/fwupd/fwupd
root       16302  0.4  1.6 2184628 65736 ?       Ssl  Oct30  38:32 /usr/bin/containerd
root      331684  0.7  1.5 950404 61480 ?        SNl  17:43   0:01 /opt/sophos-spl/plugins/edr/bin/osqueryd
sophos-+    1419  0.2  1.5 716164 59276 ?        SNl  Oct30  24:14 /opt/sophos-spl/plugins/runtimedetections/bin/perf-sensor
root         748  0.4  1.4 776260 55372 ?        SNl  Oct30  38:14 /opt/sophos-spl/plugins/av/sbin/soapd
[Thu Nov  5 17:22:00 2026] Out of memory: Killed process 318007 (sophos_threat_d) total-vm:1541324kB, anon-rss:758816kB, file-rss:2048kB, shmem-rss:0kB, UID:992 pgtables:1820kB oom_score_adj:0
[Thu Nov  5 17:31:43 2026] Out of memory: Killed process 319961 (sophos_threat_d) total-vm:1243976kB, anon-rss:774172kB, file-rss:1488kB, shmem-rss:0kB, UID:992 pgtables:1816kB oom_score_adj:0
[Thu Nov  5 17:37:42 2026] Out of memory: Killed process 328803 (sophos_threat_d) total-vm:1186296kB, anon-rss:683588kB, file-rss:2512kB, shmem-rss:0kB, UID:992 pgtables:1636kB oom_score_adj:0
[Thu Nov  5 17:43:00 2026] Out of memory: Killed process 14553 (node) total-vm:12236456kB, anon-rss:738160kB, file-rss:8kB, shmem-rss:0kB, UID:0 pgtables:16816kB oom_score_adj:0
[Thu Nov  5 17:43:25 2026] Out of memory: Killed process 319198 (next-server (v) total-vm:12460988kB, anon-rss:951876kB, file-rss:8kB, shmem-rss:0kB, UID:0 pgtables:22692kB oom_score_adj:0
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker compose up -d --force-recreate web
WARN[0000] service "web": deploy.labels: deploy.labels are only applied to Swarm services
WARN[0000] service "web": deploy.rollback_config: deploy.rollback_config only applies to rolling updates in Swarm mode
WARN[0000] service "web": deploy.update_config: deploy.update_config only applies to rolling updates in Swarm mode
[+] up 3/3
 ✔ Container claim-processing-backend-1  Running                                                                    0.0s
 ✔ Container claim-processing-postgres-1 Running                                                                    0.0s
 ✔ Container claim-processing-web-1      Started                                                                    1.4s
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing#   docker compose logs -f --tail=30 web
web-1  | [entrypoint] Waiting for Convex backend at http://backend:3210/version ...
web-1  | [entrypoint] Deploying Convex functions (self-hosted)...
web-1  | [entrypoint] Syncing Convex deployment env...
web-1  | ✔ Successfully set OPENROUTER_API_KEY
web-1  | ✔ Successfully set MEMBER_DB_SERVER
web-1  | ✔ Successfully set MEMBER_DB_PORT
web-1  | ✔ Successfully set MEMBER_DB_DATABASE
web-1  | ✔ Successfully set MEMBER_DB_USER
web-1  | ✔ Successfully set MEMBER_DB_USERNAME
web-1  | ✔ Successfully set MEMBER_DB_PASSWORD
web-1  | ✔ Successfully set MEMBER_DB_AUTH
web-1  | - Deploying to http://backend:3210...
web-1  |
web-1  | ✔ No indexes are deleted by this push
web-1  | Uploading functions to Convex...
web-1  | Generating TypeScript bindings...
web-1  | Running TypeScript...
web-1  | Pushing code to your Convex deployment...
web-1  | Schema validation complete.
web-1  | Finalizing push...
web-1  | ✔ Deployed Convex functions to http://backend:3210
web-1  | ▲ Next.js 16.1.1
web-1  | - Local:         http://localhost:3000
web-1  | - Network:       http://0.0.0.0:3000
web-1  |
web-1  | ✓ Starting...
web-1  | ✓ Ready in 319ms
web-1  | [audit/start] Startup config: {
web-1  |   IN_DOCKER: true,
web-1  |   IS_DOCKER_HOSTNAME: true,
web-1  |   RAW_INTERNAL: 'http://backend:3210',
web-1  |   RAW_PUBLIC: 'https://claims-backend-helixview-stage.fhpl.net',
web-1  |   CHOSEN: 'http://backend:3210'
web-1  | }
^C
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker compose restart backend
[+] restart 0/1
 ⠴ Container claim-processing-backend-1 Restarting                                                                  1.5s
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing#  fallocate -l 4G /swapfile
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile
echo '/swapfile none swap sw 0 0' >> /etc/fstab
free -h
fallocate: fallocate failed: Text file busy
mkswap: error: /swapfile is mounted; will not make swapspace
swapon: /swapfile: swapon failed: Device or resource busy
               total        used        free      shared  buff/cache   available
Mem:           3.7Gi       2.5Gi       355Mi        22Mi       1.1Gi       1.2Gi
Swap:          4.0Gi       113Mi       3.9Gi
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing#  fallocate -l 4G /swapfile
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile
echo '/swapfile none swap sw 0 0' >> /etc/fstab
free -h
fallocate: fallocate failed: Text file busy
mkswap: error: /swapfile is mounted; will not make swapspace
swapon: /swapfile: swapon failed: Device or resource busy
               total        used        free      shared  buff/cache   available
Mem:           3.7Gi       2.5Gi       348Mi        22Mi       1.1Gi       1.2Gi
Swap:          4.0Gi       119Mi       3.9Gi
