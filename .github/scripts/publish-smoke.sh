#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Usage: $0 <path-to-release-tarball.tar.gz>" >&2
  exit 1
fi

ARCHIVE_PATH="$1"
if [[ ! -f "$ARCHIVE_PATH" ]]; then
  echo "Release archive not found: $ARCHIVE_PATH" >&2
  exit 1
fi

case "$ARCHIVE_PATH" in
  /*) ;;
  *) ARCHIVE_PATH="$(pwd)/$ARCHIVE_PATH" ;;
esac

WORK_DIR="$(mktemp -d "${TMPDIR:-/tmp}/mobileshop-release-smoke.XXXXXX")"
APP_PID=""

cleanup() {
  if [[ -n "${APP_PID:-}" ]] && kill -0 "$APP_PID" 2>/dev/null; then
    kill "$APP_PID" 2>/dev/null || true
    for _ in 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20; do
      if ! kill -0 "$APP_PID" 2>/dev/null; then
        break
      fi
      sleep 0.25
    done
    kill -9 "$APP_PID" 2>/dev/null || true
  fi
  rm -rf "$WORK_DIR"
}
trap cleanup EXIT INT TERM

EXTRACT_DIR="$WORK_DIR/extracted"
mkdir -p "$EXTRACT_DIR"
tar -xzf "$ARCHIVE_PATH" -C "$EXTRACT_DIR"

APP_DIR=""
for entry in "$EXTRACT_DIR"/*; do
  if [[ -d "$entry" ]]; then
    if [[ -n "$APP_DIR" ]]; then
      echo "Expected a single top-level directory inside archive, found multiple." >&2
      exit 1
    fi
    APP_DIR="$entry"
  fi
done

if [[ -z "$APP_DIR" ]]; then
  echo "No top-level directory found inside archive." >&2
  exit 1
fi

cd "$APP_DIR"

for required in \
  "MobileShop.Web" \
  "appsettings.json" \
  "MobileShop.Web.staticwebassets.endpoints.json" \
  "PDF/Fonts/Vazirmatn-Regular.ttf" \
  "PDF/Fonts/Vazirmatn-Bold.ttf" \
  "PDF/Fonts/SFProRounded-Regular.ttf" \
  "PDF/Fonts/SFProRounded-Bold.ttf" \
  "wwwroot/css/base.css" \
  "wwwroot/js/site.js" \
  "wwwroot/lib/chart.js/chart.umd.min.js"
do
  if [[ ! -f "$required" ]]; then
    echo "Missing required release file: $required" >&2
    exit 1
  fi
done

if [[ ! -x "./MobileShop.Web" ]]; then
  echo "MobileShop.Web is not executable after archive extraction." >&2
  exit 1
fi

for forbidden in "appsettings.Development.json" "web.config" "MobileShop.db"; do
  if [[ -e "$forbidden" ]]; then
    echo "Unexpected file in release archive: $forbidden" >&2
    exit 1
  fi
done

if find . \( -name "*.pdb" -o -name "._*" \) -print | grep -q .; then
  echo "Unexpected symbol or metadata files found in release archive:" >&2
  find . \( -name "*.pdb" -o -name "._*" \) -print >&2
  exit 1
fi

# Test --migrate-database in an isolated working directory using a cross-platform timeout.
python3 - <<'PY'
import subprocess, sys
proc = subprocess.run(["./MobileShop.Web", "--migrate-database"], timeout=120)
sys.exit(proc.returncode)
PY

if [[ ! -f "MobileShop.db" ]]; then
  echo "MobileShop.db was not created by --migrate-database." >&2
  exit 1
fi

PORT="$(python3 - <<'PY'
import socket
with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind(("127.0.0.1", 0))
    print(s.getsockname()[1])
PY
)"

APP_LOG="$WORK_DIR/app.log"
ASPNETCORE_URLS="http://127.0.0.1:${PORT}" ./MobileShop.Web >"$APP_LOG" 2>&1 &
APP_PID=$!

python3 - "$PORT" "$APP_LOG" <<'PY'
import http.cookiejar
import json
import re
import sys
import time
import urllib.error
import urllib.parse
import urllib.request

port = sys.argv[1]
log_path = sys.argv[2]
base = f"http://127.0.0.1:{port}"

jar = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))

login_html = None
for _ in range(60):
    try:
        login_html = opener.open(f"{base}/Account/Login", timeout=2).read().decode("utf-8")
        break
    except Exception:
        time.sleep(0.5)

if login_html is None:
    try:
        print(open(log_path, encoding="utf-8", errors="replace").read(), file=sys.stderr)
    except OSError:
        pass
    raise SystemExit("Published application did not become ready within 30 seconds.")

def extract_token(html):
    match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', html)
    if not match:
        raise SystemExit("Antiforgery token missing from page.")
    return match.group(1)

def post_form(path, fields):
    data = urllib.parse.urlencode(fields).encode("utf-8")
    req = urllib.request.Request(f"{base}{path}", data=data, method="POST")
    return opener.open(req, timeout=15)

# 1. Authenticate with initial admin account
login_resp = post_form("/Account/Login", {
    "__RequestVerificationToken": extract_token(login_html),
    "Username": "admin",
    "Password": "Admin@123",
})
if "/Account/Login" in login_resp.geturl():
    raise SystemExit("Admin login did not redirect away from /Account/Login.")

# 2. Verify core pages and static assets
for path in [
    "/",
    "/Products",
    "/Products/SecondHand",
    "/Transactions",
    "/People/Customers",
    "/People/Sellers",
    "/Reports/ProfitLoss",
    "/css/base.css",
    "/js/site.js",
    "/lib/chart.js/chart.umd.min.js",
]:
    resp = opener.open(f"{base}{path}", timeout=10)
    if resp.status != 200:
        raise SystemExit(f"GET {path} returned HTTP {resp.status}")

# 3. Create customer, Apple ID, and sale to exercise DB writes and QuestPDF rendering
sellers = json.loads(opener.open(f"{base}/Products/PersonPicker?handler=SearchSellers", timeout=10).read().decode("utf-8"))
if not sellers:
    raise SystemExit("No sellers found (expected Zone Mobile shop party).")
seller_id = sellers[0]["id"]

sell_html = opener.open(f"{base}/Transactions/Sell", timeout=10).read().decode("utf-8")
customer_payload = json.loads(post_form("/Transactions/Sell?handler=CreateCustomer", {
    "__RequestVerificationToken": extract_token(sell_html),
    "FirstName": "Release",
    "LastName": "Smoke",
    "PhoneNumber": "+98 912 345 6789",
    "NationalId": "1234567890",
}).read().decode("utf-8"))
if not customer_payload.get("succeeded"):
    raise SystemExit(f"Customer creation failed: {customer_payload}")
customer_id = customer_payload["option"]["id"]

apple_html = opener.open(f"{base}/Products/CreateAppleId", timeout=10).read().decode("utf-8")
post_form("/Products/CreateAppleId", {
    "__RequestVerificationToken": extract_token(apple_html),
    "Input.SellerId": str(seller_id),
    "Input.Price": "1000000",
    "Input.ProfitAmount": "200000",
    "Input.Email": "release.smoke@example.com",
    "Input.Password": "SmokePass1",
    "Input.SecurityAnswer1": "Answer1",
    "Input.SecurityAnswer2": "Answer2",
    "Input.SecurityAnswer3": "Answer3",
})

products_html = opener.open(f"{base}/Products", timeout=10).read().decode("utf-8")
product_match = re.search(r'/Products/Details\?id=(\d+)', products_html)
if not product_match:
    raise SystemExit("Created Apple ID product was not listed on /Products.")
product_id = product_match.group(1)

sell_html = opener.open(f"{base}/Transactions/Sell", timeout=10).read().decode("utf-8")
post_form("/Transactions/Sell", {
    "__RequestVerificationToken": extract_token(sell_html),
    "Input.ProductId": str(product_id),
    "Input.CustomerId": str(customer_id),
    "Input.Price": "1200000",
    "Input.Date": "1405/08/22 14:30",
})

tx_html = opener.open(f"{base}/Transactions", timeout=10).read().decode("utf-8")
tx_match = re.search(r'/Transactions/Details\?id=(\d+)', tx_html)
if not tx_match:
    raise SystemExit("Recorded sale transaction was not listed on /Transactions.")
tx_id = tx_match.group(1)

# 4. Verify all PDF endpoints produce real PDF payloads
for pdf_path in [
    f"/Transactions/Details?id={tx_id}&handler=Factor",
    f"/Transactions/Details?id={tx_id}&handler=AppleIdFactor",
    "/Transactions?handler=DownloadFactor",
]:
    pdf_resp = opener.open(f"{base}{pdf_path}", timeout=20)
    pdf_bytes = pdf_resp.read()
    if not pdf_bytes.startswith(b"%PDF-"):
        raise SystemExit(f"PDF endpoint {pdf_path} did not return a valid PDF header.")
    if len(pdf_bytes) < 5000:
        raise SystemExit(f"PDF endpoint {pdf_path} returned suspiciously small payload ({len(pdf_bytes)} bytes).")
    print(f"Verified PDF {pdf_path} ({len(pdf_bytes)} bytes)")
PY

kill "$APP_PID" 2>/dev/null || true
wait "$APP_PID" 2>/dev/null || true
APP_PID=""

if grep -Eq "\[ERR\]|\[FTL\]|fail:|crit:|Unhandled exception" "$APP_LOG"; then
  echo "Application stdout/stderr log contains an error:" >&2
  grep -En "\[ERR\]|\[FTL\]|fail:|crit:|Unhandled exception" "$APP_LOG" >&2 || true
  exit 1
fi

if ls logs/app-*.log >/dev/null 2>&1; then
  if grep -Eq "\tERR\t|\tFTL\t|Unhandled exception" logs/app-*.log; then
    echo "Application file log contains an error:" >&2
    grep -En "\tERR\t|\tFTL\t|Unhandled exception" logs/app-*.log >&2 || true
    exit 1
  fi
fi

echo "Published release smoke test passed cleanly for $ARCHIVE_PATH"
