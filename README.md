root@ip-10-11-2-214:/home/ubuntu/claim-processing# grep -n "proxy_read_timeout\|proxy_send_timeout\|send_timeout\|client_max_body_size\|server_name\|location" /etc/nginx/conf.d/claims-helixview.conf
9:    server_name claims-helixview.fhpl.net claims-backend-helixview.fhpl.net claims-auth-helixview.fhpl.net;
17:    server_name claims-helixview.fhpl.net;
18:    client_max_body_size 50M;
27:    location / {
31:        proxy_send_timeout 180s;
32:        proxy_read_timeout 180s;
47:    server_name claims-backend-helixview.fhpl.net;
48:    client_max_body_size 50M;
57:    location / {
74:    server_name claims-auth-helixview.fhpl.net;
75:    client_max_body_size 50M;
84:    location / {
