root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker network create edge
f01f851696b605bbfeb1d1c20767066102442714478fb5b43bd414b37b51ff3a
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# grep -E '^(APP_DOMAIN|CONVEX_DOMAIN|CONVEX_SITE_DOMAIN|NEXT_PUBLIC_CONVEX_URL|NEXT_PUBLIC_CONVEX_SITE_URL|CONVEX_CLOUD_ORIGIN|CONVEX_SITE_ORIGIN|CONVEX_URL_PUBLIC|NEXT_IMAGE|SPECTRA_BASE_URL|CLAIMAI_ALLOWED_ORIGINS|MEMBER_DB_SERVER|MEMBER_DB_DATABASE|MEMBER_DB_USER|MEMBER_DB_USERNAME)=' /home/ubuntu/claim-processing/.env
NEXT_PUBLIC_CONVEX_URL="https://claims-backend-helixview-stage.fhpl.net"
NEXT_PUBLIC_CONVEX_SITE_URL="https://claims-auth-helixview-stage.fhpl.net"
SPECTRA_BASE_URL="https://spectra-ai.fhpl.net"
CONVEX_CLOUD_ORIGIN="https://claims-backend-helixview-stage.fhpl.net"
CONVEX_SITE_ORIGIN="https://claims-auth-helixview-stage.fhpl.net"
NEXT_IMAGE="ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals"
MEMBER_DB_SERVER=AWS-Prod-ReportingSRV02.fhpl.in
MEMBER_DB_DATABASE=McarePlus_AI
MEMBER_DB_USER=ZAPSIGHTREPORTINGUSER
MEMBER_DB_USERNAME=ZAPSIGHTREPORTINGUSER
APP_DOMAIN="claims-helixview-stage.fhpl.net"
CONVEX_DOMAIN="claims-backend-helixview-stage.fhpl.net"
CONVEX_SITE_DOMAIN="claims-auth-helixview-stage.fhpl.net"
CLAIMAI_ALLOWED_ORIGINS=https://claims-helixview-stage.fhpl.net
root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing#


ubuntu@ip-10-11-2-214:~/claim-processing$ sudo cat /etc/nginx/conf.d/claims-helixview.conf
map $http_upgrade $connection_upgrade {
    default upgrade;
    '' close;
}

server {
    listen 80;
    listen [::]:80;
    server_name claims-helixview.fhpl.net claims-backend-helixview.fhpl.net claims-auth-helixview.fhpl.net;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl;
    listen [::]:443 ssl;
    http2 on;
    server_name claims-helixview.fhpl.net;
    client_max_body_size 100M;

    ssl_certificate /var/SSL-Certificate/fullchain.pem;
    ssl_certificate_key /var/SSL-Certificate/privkey.pem;
    ssl_session_timeout 1d;
    ssl_session_cache shared:SSL:10m;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_prefer_server_ciphers off;

    location / {
        proxy_pass http://127.0.0.1:3100;
        proxy_http_version 1.1;
        proxy_connect_timeout 180s;
        proxy_send_timeout 660s;
        proxy_read_timeout 660s;
        send_timeout 660s;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Forwarded-Proto https;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}

server {
    listen 443 ssl;
    listen [::]:443 ssl;
    http2 on;
    server_name claims-backend-helixview.fhpl.net;
    client_max_body_size 50M;

    ssl_certificate /var/SSL-Certificate/fullchain.pem;
    ssl_certificate_key /var/SSL-Certificate/privkey.pem;
    ssl_session_timeout 1d;
    ssl_session_cache shared:SSL:10m;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_prefer_server_ciphers off;

    location / {
        proxy_pass http://127.0.0.1:3310;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Forwarded-Proto https;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}

server {
    listen 443 ssl;
    listen [::]:443 ssl;
    http2 on;
    server_name claims-auth-helixview.fhpl.net;
    client_max_body_size 50M;

    ssl_certificate /var/SSL-Certificate/fullchain.pem;
    ssl_certificate_key /var/SSL-Certificate/privkey.pem;
    ssl_session_timeout 1d;
    ssl_session_cache shared:SSL:10m;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_prefer_server_ciphers off;

    location / {
        proxy_pass http://127.0.0.1:3311;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Forwarded-Proto https;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}
ubuntu@ip-10-11-2-214:~/claim-processing$ ls -la /etc/nginx/conf.d/ /etc/nginx/sites-enabled/
/etc/nginx/conf.d/:
total 12
drwxr-xr-x 2 root root 4096 Oct  9 06:17 .
drwxr-xr-x 8 root root 4096 Oct 27 06:34 ..
-rw-r--r-- 1 root root 2943 Oct  9 06:17 claims-helixview.conf

/etc/nginx/sites-enabled/:
total 8
drwxr-xr-x 2 root root 4096 May 15 10:43 .
drwxr-xr-x 8 root root 4096 Oct 27 06:34 ..
lrwxrwxrwx 1 root root   34 May 15 10:43 default -> /etc/nginx/sites-available/default
ubuntu@ip-10-11-2-214:~/claim-processing$ for c in $(sudo awk '$1=="ssl_certificate"{gsub(";","",$2); print $2}' /etc/nginx/conf.d/claims-helixview.conf | sort -u); do echo "== $c"; sudo openssl x509 -in "$c" -noout -subject -ext subjectAltName -enddate; done
== /var/SSL-Certificate/fullchain.pem
subject=CN=*.fhpl.net
X509v3 Subject Alternative Name:
    DNS:*.fhpl.net, DNS:fhpl.net
notAfter=Mar 19 10:53:59 2027 GMT
ubuntu@ip-10-11-2-214:~/claim-processing$
