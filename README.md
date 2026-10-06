   echo | openssl s_client -connect <convex-host>:443 -servername <convex-host> 2>/dev/null | grep -E "^ *[0-9] s:|Verify return code"
