root@ip-10-11-2-214:/home/ubuntu/claim-processing# openssl verify -untrusted /tmp/intermediate.pem /var/SSL-Certificate/fullchain.pem
C=US, O=GoDaddy.com, CN=GoDaddy TLS Intermediate CA DV - R1v1
error 20 at 1 depth lookup: unable to get local issuer certificate
error /var/SSL-Certificate/fullchain.pem: verification failed
