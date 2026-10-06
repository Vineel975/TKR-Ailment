root@ip-10-11-2-214:/home/ubuntu/claim-processing# cat /tmp/intermediate.pem /tmp/inter2.pem > /tmp/chain.pem
openssl verify -untrusted /tmp/chain.pem /var/SSL-Certificate/fullchain.pem
C=US, O=GoDaddy.com, CN=GoDaddy TLS Root CA - R1
error 19 at 2 depth lookup: self-signed certificate in certificate chain
error /var/SSL-Certificate/fullchain.pem: verification failed
