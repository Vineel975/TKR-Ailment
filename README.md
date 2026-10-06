   docker compose exec web sh -c 'grep -rl "(cause: " /app/.next/server 2>/dev/null | head -1'
