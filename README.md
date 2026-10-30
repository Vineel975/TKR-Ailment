cd /home/ubuntu/claim-processing
sed -i 's#^NEXT_IMAGE=.*#NEXT_IMAGE="claim-processing-stage:preauth-finals"#' .env
sed -i '/^CONVEX_SELF_HOSTED_ADMIN_KEY=/d' .env
grep -E '^(NEXT_IMAGE|WEB_PORT|CONVEX_BACKEND_PORT|CONVEX_SITE_PORT)=' .env
