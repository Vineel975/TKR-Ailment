   docker compose ps
   docker compose exec web node -e "fetch('http://backend:3210/version').then(r=>console.log('internal', r.status)).catch(e=>console.log('internal FAILED', e.cause && e.cause.code))"


      docker compose exec backend printenv CONVEX_CLOUD_ORIGIN
   docker compose exec web node -e "fetch('<paste the origin here>/version').then(r=>console.log('public', r.status)).catch(e=>console.log('public FAILED', e.cause && e.cause.code))"
