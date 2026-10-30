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


<img width="1110" height="303" alt="image" src="https://github.com/user-attachments/assets/03cfea6d-3ac2-46e4-8de8-62add250ac5d" />

<img width="1267" height="623" alt="image" src="https://github.com/user-attachments/assets/0bded6a7-6c4a-4f45-a131-9fbd48cce932" />

