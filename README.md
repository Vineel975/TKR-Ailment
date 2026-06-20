root@ip-10-11-2-214:/home/ubuntu/claim-processing# bash scripts/test.sh --proxy
Proxy mode enabled — HTTPS domains:
  App:    https://claims-helixview.fhpl.net
  Convex: https://claims-backend-helixview.fhpl.net
  Site:   https://claims-auth-helixview.fhpl.net
[+] up 3/3
 ✔ Container claim-processing-postgres-1  Running                                                                   0.0s
 ✔ Container claim-processing-backend-1   Running                                                                   0.0s
 ✔ Container claim-processing-dashboard-1 Running                                                                   0.0s
Waiting for Convex backend at http://127.0.0.1:3310/version ...
[+] pull 0/1
 ⠴ Image ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor Pulling                             3.6s
[+] pull 0/1ARNING: Some service image(s) must be built from source by running:
 ⠦ Image ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor Pulling                             3.6s
failed to prepare extraction snapshot "extract-186669209-Jd6z sha256:2fea40bb2eb3c9855bdbd014e7c4ca5fc67245d8dfc200856ce34489b1aabdeb": failed to create prepare snapshot dir: failed to create temp dir: mkdir /var/lib/containerd/io.containerd.snapshotter.v1.overlayfs/snapshots/new-2526111892: no space left on device
