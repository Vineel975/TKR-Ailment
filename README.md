   openssl x509 -in /tmp/intermediate.pem -noout -issuer
   openssl x509 -in /tmp/intermediate.pem -noout -text | grep "CA Issuers"



      curl -s -o /tmp/inter2.crt "<CA Issuers URL from step 1>"
   openssl x509 -inform DER -in /tmp/inter2.crt -out /tmp/inter2.pem 2>/dev/null \
     || openssl x509 -in /tmp/inter2.crt -out /tmp/inter2.pem

        cat /tmp/intermediate.pem /tmp/inter2.pem > /tmp/chain.pem
   openssl verify -untrusted /tmp/chain.pem /var/SSL-Certificate/fullchain.pem

        cat /tmp/inter2.pem >> /var/SSL-Certificate/fullchain.pem
