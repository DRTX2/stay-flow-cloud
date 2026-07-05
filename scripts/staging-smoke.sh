#!/usr/bin/env bash
set -euo pipefail

: "${STAGING_API_URL:?Set STAGING_API_URL, e.g. https://api-staging.example.com}"
: "${SMOKE_CLIENT_ID:?Set SMOKE_CLIENT_ID for a client-credentials smoke client}"
: "${SMOKE_CLIENT_SECRET:?Set SMOKE_CLIENT_SECRET for the smoke client}"

api_url="${STAGING_API_URL%/}"

curl -fsS "$api_url/health/live" >/dev/null
curl -fsS "$api_url/health/ready" >/dev/null

token_response="$({
  curl -fsS -X POST "$api_url/connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    --data-urlencode "grant_type=client_credentials" \
    --data-urlencode "client_id=$SMOKE_CLIENT_ID" \
    --data-urlencode "client_secret=$SMOKE_CLIENT_SECRET" \
    --data-urlencode "scope=stayflow.api"
})"

access_token="$(printf '%s' "$token_response" | python3 -c 'import json,sys; print(json.load(sys.stdin)["access_token"])')"

curl -fsS "$api_url/api/v1/analytics/front-desk/today" \
  -H "Authorization: Bearer $access_token" \
  -H "Accept: application/json" >/dev/null

printf 'Staging smoke checks passed for %s\n' "$api_url"
