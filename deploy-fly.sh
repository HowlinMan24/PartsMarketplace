#!/usr/bin/env bash
set -euo pipefail

if ! command -v flyctl >/dev/null 2>&1; then
  echo "flyctl not found. Install from https://fly.io/docs/hands-on/install-flyctl/"
  exit 1
fi

if [ "$#" -lt 1 ]; then
  echo "Usage: $0 <fly-app-name> [connection-string]"
  exit 1
fi

APP_NAME="$1"
DB_CONN="${2-}"

if ! flyctl apps info "$APP_NAME" >/dev/null 2>&1; then
  echo "Creating Fly app: $APP_NAME"
  flyctl apps create "$APP_NAME"
else
  echo "Fly app $APP_NAME already exists"
fi

if [ -z "$DB_CONN" ]; then
  echo "Please paste your Supabase connection string (will be set as secret):"
  read -r DB_CONN
fi

if [ -n "$DB_CONN" ]; then
  flyctl secrets set ConnectionStrings__DefaultConnection="$DB_CONN"
fi

# Deploy
flyctl deploy --config fly.toml

echo "Deployed $APP_NAME. Use 'flyctl open' to view the app." 
