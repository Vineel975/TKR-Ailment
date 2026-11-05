   docker exec claim-processing-web-1 node -e "fetch('http://localhost:3000/api/auto/run',{method:'POST'}).then(r=>console.log(r.status))"
