cd /home/ubuntu/claim-processing
mkdir -p certs
cat /tmp/intermediate.pem /tmp/inter2.pem > certs/godaddy-chain.pem
grep -c "BEGIN CERTIFICATE" certs/godaddy-chain.pem
