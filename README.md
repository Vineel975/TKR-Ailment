   scp -i "C:\Users\satyavineel.k\Downloads\AWS-Prod-Cashless-AI-KP.pem" ubuntu@10.11.2.214:/tmp/claim-processing.tgz "C:\Users\satyavineel.k\Desktop\claim-processing.tgz"

      certutil -hashfile "C:\Users\satyavineel.k\Desktop\claim-processing.tgz" SHA256

         scp -i "C:\Users\satyavineel.k\Desktop\KP-AWS-PreProd-Cashless-AI.pem" "C:\Users\satyavineel.k\Desktop\claim-processing.tgz" ubuntu@10.11.4.180:/tmp/

   sha256sum /tmp/claim-processing.tgz        # must be the same 71dcc504...8ccc
   tar -xzpf /tmp/claim-processing.tgz -C /home/ubuntu
   rm /home/ubuntu/claim-processing/.env.sw?
   ls -la /home/ubuntu/claim-processing


<img width="1110" height="303" alt="image" src="https://github.com/user-attachments/assets/03cfea6d-3ac2-46e4-8de8-62add250ac5d" />

<img width="1267" height="623" alt="image" src="https://github.com/user-attachments/assets/0bded6a7-6c4a-4f45-a131-9fbd48cce932" />

