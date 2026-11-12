CLAIM=26110544147; docker logs -t claim-processing-web-1 --since 24h 2>&1 | grep -E "autoDecision claim $CLAIM|auto-run.*$CLAIM|adjudicate.*$CLAIM|coding-shadow.*$CLAIM"


CLAIMAI_AUTO_INSURER_CONTAINS=*
CLAIMAI_AUTO_BILL_LOWEST=off


   IMG=ghcr.io/adityamiskin/claim-processing/next-convex:feature-coderefactor
   STAGE=claim-processing-stage:preauth-finals

      docker tag $STAGE claim-processing-stage:previous

         docker pull $IMG
   docker image inspect $IMG --format 'built: {{.Created}}'

      docker build --no-cache -t $STAGE - < Dockerfile.stage.coderefactor

         P='[a-z-]*helixview[-a-z]*[.]fhpl[.]net|127[.]0[.]0[.]1:3210'
   docker run --rm --entrypoint grep $STAGE -rhoIE "$P" /app/.next | sort -u

      docker run --rm --entrypoint sh $STAGE -c '
   echo "1 automatic run (phase 5): $(ls -d /app/.next/server/app/api/auto/run >/dev/null 2>&1 && echo YES || echo NO)";
   echo "2 SBI scope code:          $(grep -rlq --include=*.js CLAIMAI_AUTO_INSURER_CONTAINS /app/.next/server && echo YES || echo NO)";
   echo "3 lens query removed:      $(grep -rlq --include=*.js "Kindly clarify the type of lens used" /app/.next && echo NO || echo YES)"'



      docker compose up -d --force-recreate web
   docker compose logs -f --tail=50 web

       docker compose ps
    docker image prune -f

    docker exec claim-processing-web-1 sh -c '
echo "1 automatic run (phase 5): $(ls -d /app/.next/server/app/api/auto/run >/dev/null 2>&1 && echo YES || echo NO)";
echo "2 SBI scope code:          $(grep -rlq --include=*.js CLAIMAI_AUTO_INSURER_CONTAINS /app/.next/server && echo YES || echo NO)";
echo "3 lens query removed:      $(grep -rlq --include=*.js "Kindly clarify the type of lens used" /app/.next && echo NO || echo YES)";
echo "   CLAIMAI_AUTO_RUN=$(printenv CLAIMAI_AUTO_RUN)  USER_REGION_ID=$(printenv CLAIMAI_AUTO_USER_REGION_ID)";
echo "   INSURER_CONTAINS=$(printenv CLAIMAI_AUTO_INSURER_CONTAINS)  BILL_LOWEST=$(printenv CLAIMAI_AUTO_BILL_LOWEST)"'





   docker tag claim-processing-stage:previous claim-processing-stage:preauth-finals
   docker compose up -d web
   docker compose logs -f --tail=50 web



curl -s -o /dev/null -w "app: %{http_code}\n" http://127.0.0.1:3000/login

curl -s -o /dev/null -w "site: %{http_code}\n" https://claims-helixview-stage.fhpl.net/login

curl -s -w "\nconvex: %{http_code}\n" http://127.0.0.1:3210/version

   docker compose logs -t --since 1h web 2>&1 | grep -E "runTariffMatching completed|Tariff matching FAILED"


AUTO APPROVAL CEILING UPTO Rs.50000/-  for Cataract, After AI Engine rules, If Amount amissible Above 50K than the case should be return to Doctor, If Admissible amount is Less than 50K than Auto approval should go.


free -h
docker stats --no-stream --format "table {{.Name}}\t{{.MemUsage}}\t{{.MemPerc}}\t{{.CPUPerc}}"
ps aux --sort=-%mem | head -12
dmesg -T | grep -i -E "out of memory|killed process" | tail -5

  docker compose up -d --force-recreate web
  docker compose logs -f --tail=30 web

    docker compose restart backend

     CLAIM=26110644150
   docker logs claim-processing-web-1 2>&1 | grep "$CLAIM" | grep -E "flow=|final approvals|autoDecision|auto-run"


      cp .env .env.bak-$(date +%Y%m%d-%H%M)

         grep '^RULES_GITHUB_BASE_BRANCH=' .env
   grep -c '^RULES_AGENT_ENABLED=' .env
   grep -c '^RULES_ASSIST_ENABLED=' .env
   docker compose exec web sh -c 'test -n "$OPENROUTER_API_KEY" && echo key-set || echo key-missing'

      grep -E '^RULES_GITHUB_BASE_BRANCH=|^RULES_AGENT_ENABLED=|^RULES_ASSIST_ENABLED=' .env

   CLAIM=26111044157
   docker logs claim-processing-web-1 2>&1 | grep "$CLAIM" | grep -E "flow=|FINAL|final approvals|autoDecision|auto-run" | tail -20

    cd /home/ubuntu/claim-processing
   sed -i 's#^RULES_GITHUB_BASE_BRANCH=.*#RULES_GITHUB_BASE_BRANCH=feature/coderefactor#' .env
   grep '^RULES_GITHUB_BASE_BRANCH=' .env
   docker compose up -d --force-recreate web

   Run stop="$(od -An -N16 -tx1 /dev/urandom | tr -d ' \n')"; echo "::stop-commands::$stop"; trap 'echo "::$stop::"' EXIT
  stop="$(od -An -N16 -tx1 /dev/urandom | tr -d ' \n')"; echo "::stop-commands::$stop"; trap 'echo "::$stop::"' EXIT
  T="$RUNNER_TEMP/agent-tools"
  fail() {
    echo "- Failed: Write the code. $1" >> "$GITHUB_STEP_SUMMARY"
    exit 1
  }
  [ -n "$ANTHROPIC_API_KEY" ] || fail "The repository has no ANTHROPIC_API_KEY secret. A repository Admin must add it (Settings, Secrets and variables, Actions)."
  model="${AGENT_MODEL:-claude-sonnet-5-5}"
  [[ "$model" =~ ^[a-z0-9][a-z0-9.-]{2,60}$ ]] || fail "The repository variable PREAUTH_RULES_AGENT_MODEL is not a model name."
  budget="${AGENT_BUDGET_USD:-10}"
  [[ "$budget" =~ ^[0-9]{1,3}(\.[0-9]{1,2})?$ ]] || fail "The repository variable PREAUTH_RULES_AGENT_BUDGET_USD is not an amount in US dollars."
  # The agent may read and edit files, and run only the checks its instructions list.
  allowed=(
    "Read" "Edit" "Write" "Glob" "Grep"
    "Bash(npx tsc --noEmit -p .)" "Bash(npx jest)" "Bash(npx jest *)"
    "Bash(RULES_BASELINE_WRITE=1 npx jest *)"
    "Bash(npm test)" "Bash(npm test *)" "Bash(npm run test:finals)" "Bash(npm run test:rules)" "Bash(npm run test:convex)" "Bash(npm run check:cataract-rules)"
    "Bash(npx eslint *)"
    "Bash(bun scripts/rules/recount-catalogue.ts)" "Bash(bun scripts/rules/recount-catalogue.ts *)"
    "Bash(bun scripts/rules/gen-inventory.ts)" "Bash(bun scripts/rules/gen-inventory.ts *)"
    "Bash(git status)" "Bash(git status *)" "Bash(git diff)" "Bash(git diff *)" "Bash(git log *)" "Bash(git show *)" "Bash(ls)" "Bash(ls *)"
  )
  set +e
  "$RUNNER_TEMP/agent-cli/node_modules/.bin/claude" -p \
    --output-format json \
    --model "$model" \
    --max-budget-usd "$budget" \
    --permission-mode dontAsk \
    --tools "Read,Edit,Write,Glob,Grep,Bash" \
    --allowedTools "${allowed[@]}" \
    --disallowedTools "WebFetch" "WebSearch" \
    --append-system-prompt "$(cat "$T/INSTRUCTIONS.md")" \
    --strict-mcp-config \
    --no-session-persistence \
    < "$T/prompt.md" > "$T/agent-output.json" 2> "$T/agent-stderr.txt"
  code=$?
  set -e
  # What the agent said last, for whoever reads this run (typed text: commands are stopped).
  echo "The coding agent exited with code $code. Its last message:"
  jq -r '.result // empty' "$T/agent-output.json" 2> /dev/null | head -c 6000 || true
  echo
  tail -c 2000 "$T/agent-stderr.txt" || true
  if [ "$(jq -r '.is_error // false' "$T/agent-output.json" 2> /dev/null)" = "true" ] && jq -r '.result // ""' "$T/agent-output.json" 2> /dev/null | grep -qi "api key"; then
    fail "Anthropic refused the ANTHROPIC_API_KEY secret. A repository Admin must replace it (Settings, Secrets and variables, Actions), then an Admin can send the request again."
  fi
  if ! bun "$T/pipeline.ts" result --repo . --agent-output "$T/agent-output.json" --out "$RUNNER_TEMP/outcome.json" > "$T/result-out.txt" 2> "$T/result-err.txt"; then
    fail "The coding agent stopped without a usable result: $(head -c 200 "$T/result-err.txt" | tr -d '\n' | sed 's/[.]$//' | bun "$T/pipeline.ts" safe-text). It may have reached its spending limit. An Admin can send the request again; if it stops again, a developer needs to look at this run."
  fi
  cat "$T/result-out.txt" >> "$GITHUB_OUTPUT"
  echo "- Passed: Write the code. The agent finished: $(jq -r .outcome "$RUNNER_TEMP/outcome.json")." >> "$GITHUB_STEP_SUMMARY"
  shell: /usr/bin/bash --noprofile --norc -e -o pipefail {0}
  env:
    AGENT_CLI_VERSION: 2.1.309
    NEXT_TELEMETRY_DISABLED: 1
    PORTAL_LINK_CHECKED: https://claims-helixview-stage.fhpl.net/rules/changes/20261112-091858-maternity-initials-2ae9
    BASE_COMMIT: 6e157d0fbdf02db862fdcbf92ff4fe5aad792310
    ANTHROPIC_API_KEY: ***
    AGENT_MODEL: 
    AGENT_BUDGET_USD: 
    DISABLE_AUTOUPDATER: 1
    CLAUDE_CODE_DISABLE_NONESSENTIAL_TRAFFIC: 1
::stop-commands::***
The coding agent exited with code 1. Its last message:
Credit balance is too low

::***::
Error: Process completed with exit code 1.



