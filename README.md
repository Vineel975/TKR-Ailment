   docker compose exec web node -e "fetch('http://backend:3210/version').then(r=>console.log('internal', r.status)).catch(e=>console.log('internal FAILED', e.cause && e.cause.code))"
