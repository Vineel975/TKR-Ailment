sudo tar -czf /tmp/nginx-ssl.tgz -C / var/SSL-Certificate etc/nginx/conf.d/claims-helixview.conf
sudo chown ubuntu:ubuntu /tmp/nginx-ssl.tgz && chmod 600 /tmp/nginx-ssl.tgz
sha256sum /tmp/nginx-ssl.tgz


scp -i "C:\Users\satyavineel.k\Downloads\AWS-Prod-Cashless-AI-KP.pem" ubuntu@10.11.2.214:/tmp/nginx-ssl.tgz "C:\Users\satyavineel.k\Desktop\nginx-ssl.tgz"

scp -i "C:\Users\satyavineel.k\Desktop\KP-AWS-PreProd-Cashless-AI.pem" "C:\Users\satyavineel.k\Desktop\nginx-ssl.tgz" ubuntu@10.11.4.180:/tmp/

apt install -y nginx

sha256sum /tmp/nginx-ssl.tgz
tar -xzpf /tmp/nginx-ssl.tgz -C /

sed -i 's/-helixview[.]fhpl[.]net/-helixview-stage.fhpl.net/g' /etc/nginx/conf.d/claims-helixview.conf
grep -n server_name /etc/nginx/conf.d/claims-helixview.conf

nginx -t && systemctl reload nginx
