   docker ps --format 'table {{.Names}}\t{{.Image}}\t{{.Status}}'

      docker inspect -f '{{.State.StartedAt}}' <web-container-name>

         docker logs <web-container-name> --since 24h 2>&1 | grep -i "tariff-file-selection"
