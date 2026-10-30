ls -la ~/claim-processing     # -la also lists hidden files such as .env
ls -la ~                      # see whether there are other app folders next to it
sudo tar -czf /tmp/claim-processing.tgz -C /home/ubuntu claim-processing
sudo chown ubuntu:ubuntu /tmp/claim-processing.tgz && chmod 600 /tmp/claim-processing.tgz
sha256sum /tmp/claim-processing.tgz      # note this value


cd %USERPROFILE%\Desktop
scp -i "PATH\TO\YOUR-PROD-KEY.pem" ubuntu@10.11.2.214:/tmp/claim-processing.tgz .
scp -i "C:\Users\satyavineel.k\Desktop\KP-AWS-PreProd-Cashless-AI.pem" claim-processing.tgz ubuntu@10.11.4.180:/tmp/


sha256sum /tmp/claim-processing.tgz      # must match the value from step 1
tar -xzpf /tmp/claim-processing.tgz -C /home/ubuntu
ls -la /home/ubuntu/claim-processing     # should look the same as on prod
