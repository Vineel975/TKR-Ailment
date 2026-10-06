root@ip-10-11-2-214:/home/ubuntu/claim-processing# grep -c "BEGIN CERTIFICATE" /var/SSL-Certificate/fullchain.pem
nginx -t && systemctl reload nginx
echo | openssl s_client -connect claims-helixview.fhpl.net:443 -servername claims-helixview.fhpl.net 2>/dev/null | grep -E "^ *[0-9] s:"
2
nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
nginx: configuration file /etc/nginx/nginx.conf test is successful
 0 s:CN=*.fhpl.net
