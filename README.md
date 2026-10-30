sudo docker compose ls

sudo find / -maxdepth 4 -type d -name 'ai-ana-proxy' 2>/dev/null

grep -h "test.sh\|ai-ana-proxy" ~/.bash_history; sudo grep -h "test.sh\|ai-ana-proxy" /root/.bash_history

sudo docker network inspect edge --format '{{range .Containers}}{{.Name}} {{end}}'
