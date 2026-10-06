   grep -c "BEGIN CERTIFICATE" /var/SSL-Certificate/fullchain.pem      # expect 2
   nginx -t && systemctl reload nginx
   echo | openssl s_client -connect claims-helixview.fhpl.net:443 -servername claims-helixview.fhpl.net 2>/dev/null | grep -E "^ *[0-9] s:"
