printf 'FROM ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals\nRUN grep -rlE "claims-backend-helixview[.]fhpl[.]net|http://127[.]0[.]0[.]1:3210" /app/.next | xargs -r sed -i -e "s#claims-backend-helixview[.]fhpl[.]net#claims-backend-helixview-stage.fhpl.net#g" -e "s#http://127[.]0[.]0[.]1:3210#https://claims-backend-helixview-stage.fhpl.net#g"\n' > /home/ubuntu/claim-processing/Dockerfile.stage


docker build -t claim-processing-stage:preauth-finals - < /home/ubuntu/claim-processing/Dockerfile.stage

docker run --rm --entrypoint sh claim-processing-stage:preauth-finals -c "grep -rhoE 'https?://[a-z0-9.-]*(helixview|127[.]0[.]0[.]1)[a-z0-9.:-]*' /app/.next | sort | uniq -c"



root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker build -t claim-processing-stage:preauth-finals - < /home/ubuntu/claim-processing/Dockerfile.stage
[+] Building 5.4s (5/5) FINISHED                         docker:default
 => [internal] load build definition from Dockerfile               0.0s
 => => transferring dockerfile: 392B                               0.0s
 => [internal] load metadata for ghcr.io/adityamiskin/claim-proce  0.0s
 => [internal] load .dockerignore                                  0.0s
 => => transferring context: 2B                                    0.0s
 => [1/2] FROM ghcr.io/adityamiskin/claim-processing/next-convex:  5.1s
 => => resolve ghcr.io/adityamiskin/claim-processing/next-convex:  0.0s
 => => extracting sha256:f791e47cf52abf7b364878f2141547e3781072e4  5.0s
 => ERROR [2/2] RUN grep -rlE "claims-backend-helixview[.]fhpl[.]  0.0s
------
 > [2/2] RUN grep -rlE "claims-backend-helixview[.]fhpl[.]net|http://127[.]0[.]0[.]1:3210" /app/.next | xargs -r sed -i -e "s#claims-backend-helixview[.]fhpl[.]net#claims-backend-helixview-stage.fhpl.net#g" -e "s#http://127[.]0[.]0[.]1:3210#https://claims-backend-helixview-stage.fhpl.net#g":
------
Dockerfile:2
--------------------
   1 |     FROM ghcr.io/adityamiskin/claim-processing/next-convex:preauth-finals
   2 | >>> RUN grep -rlE "claims-backend-helixview[.]fhpl[.]net|http://127[.]0[.]0[.]1:3210" /app/.next | xargs -r sed -i -e "s#claims-backend-helixview[.]fhpl[.]net#claims-backend-helixview-stage.fhpl.net#g" -e "s#http://127[.]0[.]0[.]1:3210#https://claims-backend-helixview-stage.fhpl.net#g"
   3 |
--------------------
ERROR: failed to build: failed to solve: failed to compute cache key: failed to unmount /var/lib/containerd/tmpmounts/containerd-mount2262053732: failed to unmount target /var/lib/containerd/tmpmounts/containerd-mount2262053732: device or resource busy
