sudo cat /etc/nginx/conf.d/claims-helixview.conf

ls -la /etc/nginx/conf.d/ /etc/nginx/sites-enabled/

for c in $(sudo awk '$1=="ssl_certificate"{gsub(";","",$2); print $2}' /etc/nginx/conf.d/claims-helixview.conf | sort -u); do echo "== $c"; sudo openssl x509 -in "$c" -noout -subject -ext subjectAltName -enddate; done

docker network create edge

grep -E '^(APP_DOMAIN|CONVEX_DOMAIN|CONVEX_SITE_DOMAIN|NEXT_PUBLIC_CONVEX_URL|NEXT_PUBLIC_CONVEX_SITE_URL|CONVEX_CLOUD_ORIGIN|CONVEX_SITE_ORIGIN|CONVEX_URL_PUBLIC|NEXT_IMAGE|SPECTRA_BASE_URL|CLAIMAI_ALLOWED_ORIGINS|MEMBER_DB_SERVER|MEMBER_DB_DATABASE|MEMBER_DB_USER|MEMBER_DB_USERNAME)=' /home/ubuntu/claim-processing/.env
