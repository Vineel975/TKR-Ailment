   cp /var/SSL-Certificate/fullchain.pem /var/SSL-Certificate/fullchain.pem.bak-$(date +%F)

      curl -s -o /tmp/intermediate.crt "<CA Issuers URL>"

      
