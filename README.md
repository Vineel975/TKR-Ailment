   openssl verify -untrusted /tmp/intermediate.pem /var/SSL-Certificate/fullchain.pem

      nginx -t && systemctl reload nginx
