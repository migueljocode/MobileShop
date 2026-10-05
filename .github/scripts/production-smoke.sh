#!/usr/bin/env bash
set -euo pipefail

ROOT="$(git rev-parse --show-toplevel)"
DB="$ROOT/MobileShop.db"
LOG_DIR="$ROOT/MobileShop.Log"
SMOKE_DIR="$ROOT/TestResults/ProductionSmoke"

if [[ "${CI:-}" != "true" && "${MOBILESHOP_SMOKE_ALLOW_DELETE:-}" != "1" ]]; then
  echo "Refusing to run Production smoke outside CI because it deletes these workspace artifacts:" >&2
  printf "  %s\\n" "$DB" "$DB-wal" "$DB-shm" "$DB.*.bak" "$LOG_DIR" >&2
  echo "Set CI=true in CI, or explicitly set MOBILESHOP_SMOKE_ALLOW_DELETE=1 for a disposable local workspace." >&2
  exit 1
fi
DEV_LOG="$SMOKE_DIR/development.log"
PROD_GUARD_LOG="$SMOKE_DIR/production-guard.log"
MIGRATE_LOG="$SMOKE_DIR/migrate.log"
PROD_LOG="$SMOKE_DIR/production.log"

mkdir -p "$SMOKE_DIR"
rm -f "$DB" "$DB"-wal "$DB"-shm
rm -f "$DB".*.bak
rm -rf "$LOG_DIR"

APP_PID=""

cleanup() {
  if [[ -n "${APP_PID:-}" ]] && kill -0 "$APP_PID" 2>/dev/null; then
    kill "$APP_PID" 2>/dev/null || true
    for _ in {1..20}; do
      if ! kill -0 "$APP_PID" 2>/dev/null; then
        break
      fi
      sleep 0.25
    done
    kill -9 "$APP_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

wait_for_home() {
  local url="$1"
  local log="$2"
  for _ in {1..60}; do
    if curl --silent --show-error --fail --max-time 2 -o /dev/null "$url"; then
      return 0
    fi
    if ! kill -0 "$APP_PID" 2>/dev/null; then
      echo "Application exited before becoming ready." >&2
      tail -n 80 "$log" >&2 || true
      return 1
    fi
    sleep 1
  done
  echo "Application did not become ready within 60 seconds." >&2
  tail -n 80 "$log" >&2 || true
  return 1
}

start_app() {
  local environment="$1"
  local log="$2"
  APP_PID=""
  ASPNETCORE_ENVIRONMENT="$environment"   ASPNETCORE_URLS="http://127.0.0.1:5099"     dotnet run --no-build --no-launch-profile --project src/MobileShop.Web >"$log" 2>&1 &
  APP_PID=$!
}

stop_app() {
  cleanup
  APP_PID=""
}

fingerprint() {
  local output="$1"
  {
    echo "People=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM People;')"
    echo "Products=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM Products;')"
    echo "Phones=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM Phones;')"
    echo "AppleIds=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM AppleIds;')"
    echo "Transactions=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM Transactions;')"
    echo "Categories=$(sqlite3 "$DB" 'SELECT COUNT(*) FROM Categories;')"
    echo "Integrity=$(sqlite3 "$DB" 'PRAGMA integrity_check;')"
  } >"$output"
}

start_app "Development" "$DEV_LOG"
wait_for_home "http://127.0.0.1:5099/" "$DEV_LOG"
stop_app

test -f "$DB"
fingerprint "$SMOKE_DIR/fingerprint-before.txt"
cp "$SMOKE_DIR/fingerprint-before.txt" "$SMOKE_DIR/fingerprint-before-copy.txt"

set +e
timeout 120 env ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS="http://127.0.0.1:5099"   dotnet run --no-build --no-launch-profile --project src/MobileShop.Web >"$PROD_GUARD_LOG" 2>&1
GUARD_EXIT=$?
set -e

if [[ "$GUARD_EXIT" -eq 124 ]]; then
  echo "Production startup guard exceeded the 120-second timeout." >&2
  cat "$PROD_GUARD_LOG" >&2
  exit 1
fi
if [[ "$GUARD_EXIT" -eq 0 ]]; then
  echo "Production startup unexpectedly succeeded against a no-history database." >&2
  cat "$PROD_GUARD_LOG" >&2
  exit 1
fi
grep -F -- "--migrate-database" "$PROD_GUARD_LOG"

set +e
timeout 120 env ASPNETCORE_ENVIRONMENT=Production   dotnet run --no-build --no-launch-profile --project src/MobileShop.Web -- --migrate-database >"$MIGRATE_LOG" 2>&1
MIGRATE_EXIT=$?
set -e

if [[ "$MIGRATE_EXIT" -eq 124 ]]; then
  echo "The --migrate-database command exceeded the 120-second timeout." >&2
  cat "$MIGRATE_LOG" >&2
  exit 1
fi
if [[ "$MIGRATE_EXIT" -ne 0 ]]; then
  echo "--migrate-database failed with exit code $MIGRATE_EXIT." >&2
  cat "$MIGRATE_LOG" >&2
  exit "$MIGRATE_EXIT"
fi
grep -F "Legacy database baselined successfully" "$MIGRATE_LOG"

BACKUPS=( "$DB".*.bak )
if [[ "${#BACKUPS[@]}" -ne 1 || ! -f "${BACKUPS[0]}" ]]; then
  echo "Expected exactly one verified database backup." >&2
  printf '%s\\n' "${BACKUPS[@]}" >&2
  exit 1
fi

HISTORY_COUNT="$(sqlite3 "$DB" 'SELECT COUNT(*) FROM __EFMigrationsHistory;')"
[[ "$HISTORY_COUNT" == "7" ]]

fingerprint "$SMOKE_DIR/fingerprint-after-migrate.txt"
diff -u "$SMOKE_DIR/fingerprint-before.txt" "$SMOKE_DIR/fingerprint-after-migrate.txt"

start_app "Production" "$PROD_LOG"
wait_for_home "http://127.0.0.1:5099/" "$PROD_LOG"

for route in / /Products /Products/SecondHand /Transactions /People/Customers /People/Sellers /Reports/ProfitLoss; do
  curl --silent --show-error --fail --max-time 10 -o /dev/null "http://127.0.0.1:5099$route"
done

for route in / /Transactions /Reports/ProfitLoss; do
  curl --silent --show-error --fail --max-time 10 "http://127.0.0.1:5099$route" | grep -F "IRR" >/dev/null
done

SOLD_PRODUCTS="$(curl --silent --show-error --fail --max-time 10 "http://127.0.0.1:5099/Products?availability=sold")"
grep -F 'text-bg-secondary">Sold' <<<"$SOLD_PRODUCTS" >/dev/null
if grep -F 'text-bg-success">Available' <<<"$SOLD_PRODUCTS" >/dev/null; then
  echo "Sold availability filter rendered an Available badge." >&2
  exit 1
fi

AVAILABLE_PRODUCTS="$(curl --silent --show-error --fail --max-time 10 "http://127.0.0.1:5099/Products?availability=available")"
grep -F 'text-bg-success">Available' <<<"$AVAILABLE_PRODUCTS" >/dev/null
if grep -F 'text-bg-secondary">Sold' <<<"$AVAILABLE_PRODUCTS" >/dev/null; then
  echo "Available availability filter rendered a Sold badge." >&2
  exit 1
fi

curl --silent --show-error --fail --max-time 10 -o /dev/null "http://127.0.0.1:5099/Products?type=phone&availability=sold"

stop_app
if grep -Eq "\[ERR\]|\[FTL\]|fail:|crit:|Unhandled exception" "$PROD_LOG"; then
  echo "Production log contains an error-level entry." >&2
  grep -En "\[ERR\]|\[FTL\]|fail:|crit:|Unhandled exception" "$PROD_LOG" >&2 || true
  exit 1
fi
fingerprint "$SMOKE_DIR/fingerprint-after-production.txt"
diff -u "$SMOKE_DIR/fingerprint-after-migrate.txt" "$SMOKE_DIR/fingerprint-after-production.txt"

BACKUPS=( "$DB".*.bak )
[[ "${#BACKUPS[@]}" -eq 1 && -f "${BACKUPS[0]}" ]]

{
  echo "Backup:"
  printf '%s\\n' "${BACKUPS[@]}"
  echo
  echo "Migration command output:"
  cat "$MIGRATE_LOG"
} >"$SMOKE_DIR/backup-and-migration.txt"

echo "Production smoke passed."
