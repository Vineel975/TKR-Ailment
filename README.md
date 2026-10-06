openssl x509 -in /var/SSL-Certificate/fullchain.pem -noout -subject -issuer
openssl x509 -in /var/SSL-Certificate/fullchain.pem -noout -text | grep "CA Issuers"
