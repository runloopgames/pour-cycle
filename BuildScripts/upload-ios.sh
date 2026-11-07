#!/bin/bash
set -euo pipefail

echo "Uploading IPA to App Store Connect..."
: "${APP_STORE_KEY_ID:?APP_STORE_KEY_ID is not set}"
: "${APP_STORE_ISSUER_ID:?APP_STORE_ISSUER_ID is not set}"
: "${APP_STORE_PRIVATE_KEY_B64:?APP_STORE_PRIVATE_KEY_B64 is not set}"
: "${APP_STORE_APPLE_ID:?APP_STORE_APPLE_ID is not set}"
: "${UNITY_PLAYER_PATH:?UNITY_PLAYER_PATH is not set}"

appstore_key_dir="$HOME/private_keys"
appstore_key_file="$appstore_key_dir/AuthKey_${APP_STORE_KEY_ID}.p8"
umask 077
mkdir -p "$appstore_key_dir"
printf '%s' "$APP_STORE_PRIVATE_KEY_B64" | openssl base64 -d -A > "$appstore_key_file"
chmod 600 "$appstore_key_file"

xcrun altool --upload-app --type ios --file "$UNITY_PLAYER_PATH" --apiKey "$APP_STORE_KEY_ID" --apiIssuer "$APP_STORE_ISSUER_ID" --apple-id "$APP_STORE_APPLE_ID"

echo "Upload completed successfully. Apple processing must finish before the build appears in TestFlight."
