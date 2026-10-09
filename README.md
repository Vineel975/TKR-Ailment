cp /etc/nginx/conf.d/claims-helixview.conf /root/claims-helixview.conf.bak-$(date +%F)

sed -i '31s/proxy_send_timeout 180s;/proxy_send_timeout 660s;/; 32s/proxy_read_timeout 180s;/proxy_read_timeout 660s;/' /etc/nginx/conf.d/claims-helixview.conf
sed -i '32a\        send_timeout 660s;' /etc/nginx/conf.d/claims-helixview.conf

sed -i '18s/client_max_body_size 50M;/client_max_body_size 100M;/' /etc/nginx/conf.d/claims-helixview.conf

sed -n '15,36p' /etc/nginx/conf.d/claims-helixview.conf

nginx -t && systemctl reload nginx

   
