root@ip-10-11-2-214:/home/ubuntu/claim-processing# docker compose logs web --since 15m | grep uploadToConvex
web-1  | [uploadToConvex] Attempt 1/3 failed: fetch failed (cause: UNABLE_TO_VERIFY_LEAF_SIGNATURE)
web-1  | [uploadToConvex] Attempt 2/3 failed: fetch failed (cause: UNABLE_TO_VERIFY_LEAF_SIGNATURE)
web-1  | [uploadToConvex] Attempt 3/3 failed: fetch failed (cause: UNABLE_TO_VERIFY_LEAF_SIGNATURE)
