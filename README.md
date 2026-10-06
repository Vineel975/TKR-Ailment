   openssl x509 -inform DER -in /tmp/intermediate.crt -out /tmp/intermediate.pem 2>/dev/null \
     || openssl x509 -in /tmp/intermediate.crt -out /tmp/intermediate.pem

      cat /tmp/intermediate.pem >> /var/SSL-Certificate/fullchain.pem
