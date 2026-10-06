root@ip-10-11-2-214:/home/ubuntu/claim-processing# grep -Rn "ssl_certificate" /etc/nginx/
/etc/nginx/conf.d/claims-helixview.conf:20:    ssl_certificate /var/SSL-Certificate/fullchain.pem;
/etc/nginx/conf.d/claims-helixview.conf:21:    ssl_certificate_key /var/SSL-Certificate/privkey.pem;
/etc/nginx/conf.d/claims-helixview.conf:50:    ssl_certificate /var/SSL-Certificate/fullchain.pem;
/etc/nginx/conf.d/claims-helixview.conf:51:    ssl_certificate_key /var/SSL-Certificate/privkey.pem;
/etc/nginx/conf.d/claims-helixview.conf:77:    ssl_certificate /var/SSL-Certificate/fullchain.pem;
/etc/nginx/conf.d/claims-helixview.conf:78:    ssl_certificate_key /var/SSL-Certificate/privkey.pem;
/etc/nginx/snippets/snakeoil.conf:4:ssl_certificate /etc/ssl/certs/ssl-cert-snakeoil.pem;
/etc/nginx/snippets/snakeoil.conf:5:ssl_certificate_key /etc/ssl/private/ssl-cert-snakeoil.key;
root@ip-10-11-2-214:/home/ubuntu/claim-processing# ^C
