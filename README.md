   docker compose exec backend node -e "fetch('https://<convex-host>/version').then(r=>console.log('OK', r.status)).catch(e=>console.log('FAILED', e.cause && e.cause.code))"
