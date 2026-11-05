root@AWS-PreProd-Cashless-AI:/home/ubuntu/claim-processing# docker logs claim-processing-web-1 --since 30m 2>&1 | grep 26110544139
[audit/start] claim 26110544139: flow=initial slNo=1 requestTypeId=1 isFinal=false
[ai-usage] {"op":"medical-admissibility","model":"google/gemini-3-flash-preview","claimId":"26110544139","cache":"none","ms":8607,"in":15709,"out":435,"reasoning":0,"cachedIn":0,"ok":true}
[ai-usage] {"op":"base-document","model":"google/gemini-3-flash-preview","claimId":"26110544139","cache":"none","ms":9117,"in":24941,"out":560,"reasoning":0,"cachedIn":0,"ok":true}
[BSI-A] fetching https://spectra-ai.fhpl.net/MedicalScrutiny/GetBSIForAI?ClaimID=26110544139 (with x-staging-key)
[BSI] claim 26110544139 — source: ClaimAI DB reimplementation. Numbers can differ from the Spectra Balance Sum Insured panel (see the [BSI-A] line above for why the Spectra call was not used).
[getClaimHistoryByClaimId] claimId= 26110544139 resolved uhid= "11559249"
[ai-usage] {"op":"coverage-pattern","model":"anthropic/claude-sonnet-4-5","claimId":"26110544139","cache":"hit","ms":0,"in":0,"out":0,"reasoning":0,"cachedIn":0,"ok":true}
[ai-usage] {"op":"benefit-cap-select","model":"anthropic/claude-sonnet-4-5","claimId":"26110544139","cache":"miss","ms":3095,"in":2771,"out":82,"reasoning":0,"cachedIn":0,"ok":true}
[audit/start] pre-seeded billingState for claim 26110544139 basis: bill
[CONVEX A(processPdf:runTariffMatching)] [LOG] '[ai-usage] {"op":"tariff-extraction","model":"google/gemini-3-flash-preview","claimId":"26110544139","cache":"none","ms":5994,"in":21877,"out":116,"reasoning":0,"cachedIn":0,"ok":true}'
[CONVEX A(processPdf:runTariffMatching)] [LOG] '[ai-usage] {"op":"benefit-amount","model":"google/gemini-3-flash-preview","claimId":"26110544139","cache":"none","ms":1918,"in":1684,"out":129,"reasoning":0,"cachedIn":0,"ok":true}'
[BSI-A] fetching https://spectra-ai.fhpl.net/MedicalScrutiny/GetBSIForAI?ClaimID=26110544139 (with x-staging-key)
[previous-claims route] called claimId: 26110544139 memberPolicyId: undefined
[BSI] claim 26110544139 — source: ClaimAI DB reimplementation. Numbers can differ from the Spectra Balance Sum Insured panel (see the [BSI-A] line above for why the Spectra call was not used).
[ai-usage] {"op":"benefit-section-summary:exclusions","model":"anthropic/claude-sonnet-4-5","claimId":"26110544139","cache":"miss","ms":1825,"in":138,"out":9,"reasoning":0,"cachedIn":0,"ok":true}
[ai-usage] {"op":"benefit-cap-select","model":"anthropic/claude-sonnet-4-5","claimId":"26110544139","cache":"miss","ms":2956,"in":2897,"out":82,"reasoning":0,"cachedIn":0,"ok":true}
[ai-usage] {"op":"benefit-section-summary:roomRent","model":"anthropic/claude-sonnet-4-5","claimId":"26110544139","cache":"miss","ms":3125,"in":313,"out":58,"reasoning":0,"cachedIn":0,"ok":true}
[ai-usage] {"op":"coverage-pattern","model":"anthropic/claude-sonnet-4-5","claimId":"26110544139","cache":"miss","ms":2745,"in":1863,"out":116,"reasoning":0,"cachedIn":0,"ok":true}
[ai-usage] {"op":"benefit-section-summary:ailment","model":"anthropic/claude-sonnet-4-5","claimId":"26110544139","cache":"miss","ms":3802,"in":1073,"out":157,"reasoning":0,"cachedIn":0,"ok":true}
[ai-usage] {"op":"coverage-pattern","model":"anthropic/claude-sonnet-4-5","claimId":"26110544139","cache":"miss","ms":2537,"in":1933,"out":107,"reasoning":0,"cachedIn":0,"ok":true}
