curl -s -X POST https://helixview.fhpl.net/api/claim/save \
  -H "Content-Type: application/json" -d '{}'



curl -s -X POST https://helixview.fhpl.net/api/claim/save \
  -H "Content-Type: application/json" -d '{
    "claimId": "99999999999",
    "slNo": 1,
    "userId": 1,
    "context": { "roleId": 1, "regionId": 1, "createdUserRegionId": 1 }
  }'
