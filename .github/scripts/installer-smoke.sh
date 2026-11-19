#!/usr/bin/env bash
set -eu

abs_path() {
  local raw="$1"
  local dir
  dir="$(cd -P "$(dirname "$raw")" && pwd)" || return 1
  printf '%s/%s\n' "$dir" "$(basename "$raw")"
}

fail() {
  printf 'FAIL: %s\n' "$*" >&2
  exit 1
}

pass() {
  printf 'PASS: %s\n' "$*"
}

detect_host_rid() {
  local sys arch
  sys="$(uname -s)"
  arch="$(uname -m)"
  case "$sys:$arch" in
    Linux:x86_64) HOST_RID="linux-x64" ;;
    Darwin:arm64) HOST_RID="osx-arm64" ;;
    Darwin:x86_64)
      if [ "$(sysctl -n hw.optional.arm64 2>/dev/null || printf '0')" = "1" ]; then
        HOST_RID="osx-arm64"
      else
        fail "Unsupported smoke-test host: $sys/$arch"
      fi
      ;;
    *)
      fail "Unsupported smoke-test host: $sys/$arch"
      ;;
  esac
}

sha256_of() {
  local file="$1"
  if command -v sha256sum >/dev/null 2>&1; then
    sha256sum "$file" | awk '{print $1}'
  else
    shasum -a 256 "$file" | awk '{print $1}'
  fi
}

free_port() {
  python3 - <<'PY'
import socket
with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind(("127.0.0.1", 0))
    print(s.getsockname()[1])
PY
}

port_is_busy() {
  local port="$1"
  python3 - "$port" <<'PY'
import socket, sys
port = int(sys.argv[1])
for family, addr in [(socket.AF_INET, "127.0.0.1"), (socket.AF_INET6, "::1")]:
    try:
        with socket.socket(family, socket.SOCK_STREAM) as s:
            s.settimeout(0.5)
            if s.connect_ex((addr, port)) == 0:
                sys.exit(0)
    except OSError:
        pass
sys.exit(1)
PY
}

poll_url_200() {
  local url="$1"
  python3 - "$url" <<'PY'
import sys, time, urllib.request
url = sys.argv[1]
for _ in range(60):
    try:
        with urllib.request.urlopen(url, timeout=2) as resp:
            if resp.status == 200:
                sys.exit(0)
    except Exception:
        pass
    time.sleep(0.5)
sys.exit(1)
PY
}

login_and_save_cookies() {
  local port="$1"
  local cookie_file="$2"
  python3 - "$port" "$cookie_file" <<'PY'
import http.cookiejar
import re
import sys
import urllib.parse
import urllib.request

port = sys.argv[1]
cookie_file = sys.argv[2]
base = f"http://127.0.0.1:{port}"

jar = http.cookiejar.MozillaCookieJar(cookie_file)
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))

login_resp = opener.open(f"{base}/Account/Login", timeout=10)
if login_resp.status != 200:
    raise SystemExit(f"Expected HTTP 200 from GET /Account/Login, got {login_resp.status}")
html = login_resp.read().decode("utf-8", errors="replace")

match = re.search(
    r'<input[^>]*name="__RequestVerificationToken"[^>]*value="([^"]+)"'
    r'|<input[^>]*value="([^"]+)"[^>]*name="__RequestVerificationToken"',
    html,
)
if not match:
    raise SystemExit("Antiforgery token not found on /Account/Login")
token = match.group(1) or match.group(2)

form_data = urllib.parse.urlencode({
    "__RequestVerificationToken": token,
    "Username": "admin",
    "Password": "Admin@123",
}).encode("utf-8")
req = urllib.request.Request(f"{base}/Account/Login", data=form_data, method="POST")
post_resp = opener.open(req, timeout=15)
if "/Account/Login" in post_resp.geturl():
    raise SystemExit("Admin login did not redirect away from /Account/Login")

home_resp = opener.open(f"{base}/", timeout=10)
if home_resp.status != 200 or "/Account/Login" in home_resp.geturl():
    raise SystemExit(f"Authenticated GET / failed: status={home_resp.status}, url={home_resp.geturl()}")

if len(jar) == 0:
    raise SystemExit("No cookies stored after admin login")

jar.save(cookie_file, ignore_discard=True, ignore_expires=True)
PY
}

verify_session_cookie() {
  local port="$1"
  local cookie_file="$2"
  python3 - "$port" "$cookie_file" <<'PY'
import http.cookiejar
import sys
import urllib.request

port = sys.argv[1]
cookie_file = sys.argv[2]
base = f"http://127.0.0.1:{port}"

jar = http.cookiejar.MozillaCookieJar(cookie_file)
jar.load(cookie_file, ignore_discard=True, ignore_expires=True)
if len(jar) == 0:
    raise SystemExit("Loaded cookie jar is empty")

opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))
for path in ["/", "/Products"]:
    resp = opener.open(f"{base}{path}", timeout=10)
    if resp.status != 200:
        raise SystemExit(f"Expected 200 for {path}, got {resp.status}")
    if "/Account/Login" in resp.geturl():
        raise SystemExit(f"Session cookie was rejected on {path} (redirected to {resp.geturl()})")
PY
}

set_db_sentinel() {
  local db_path="$1"
  python3 - "$db_path" <<'PY'
import sqlite3, sys
db_path = sys.argv[1]
with sqlite3.connect(db_path) as conn:
    conn.execute("PRAGMA user_version = 4242;")
    conn.commit()
    val = conn.execute("PRAGMA user_version;").fetchone()[0]
    if val != 4242:
        raise SystemExit(f"Failed to set user_version sentinel; got {val}")
PY
}

check_db_sentinel() {
  local db_path="$1"
  python3 - "$db_path" <<'PY'
import sqlite3, sys
db_path = sys.argv[1]
with sqlite3.connect(db_path) as conn:
    uv = conn.execute("PRAGMA user_version;").fetchone()[0]
    ic = conn.execute("PRAGMA integrity_check;").fetchone()[0]
    if uv != 4242:
        raise SystemExit(f"Expected user_version=4242 in {db_path}, got {uv}")
    if ic != "ok":
        raise SystemExit(f"PRAGMA integrity_check failed for {db_path}: {ic}")
PY
}

read_installed_tag() {
  local env_file="$1"
  awk -F= '$1 == "TAG" { print $2; exit }' "$env_file"
}

cleanup() {
  if [ -n "${S7_PID:-}" ] && kill -0 "$S7_PID" 2>/dev/null; then
    kill "$S7_PID" 2>/dev/null || true
    local i=0
    while [ "$i" -lt 20 ]; do
      if ! kill -0 "$S7_PID" 2>/dev/null; then
        break
      fi
      sleep 0.25
      i=$((i + 1))
    done
    kill -9 "$S7_PID" 2>/dev/null || true
    wait "$S7_PID" 2>/dev/null || true
  fi

  if [ -n "${BIN_DIR:-}" ] && [ -x "${BIN_DIR}/mobileshop" ] && [ -f "${ROOT:-}/state/app.pid" ]; then
    MOBILESHOP_ROOT="$ROOT" MOBILESHOP_BIN_DIR="$BIN_DIR" MOBILESHOP_PORT="${PORT:-5143}" \
      "$BIN_DIR/mobileshop" stop >/dev/null 2>&1 || true
  fi
  if [ -n "${ADOPT_BIN:-}" ] && [ -x "${ADOPT_BIN}/mobileshop" ] && [ -f "${ADOPT_ROOT:-}/state/app.pid" ]; then
    MOBILESHOP_ROOT="$ADOPT_ROOT" MOBILESHOP_BIN_DIR="$ADOPT_BIN" MOBILESHOP_PORT="${ADOPT_PORT:-5143}" \
      "$ADOPT_BIN/mobileshop" stop >/dev/null 2>&1 || true
  fi

  if [ -n "${WORK_DIR:-}" ] && [ -d "$WORK_DIR" ]; then
    local pid_file pid
    while IFS= read -r pid_file; do
      [ -f "$pid_file" ] || continue
      pid="$(cat "$pid_file" 2>/dev/null || true)"
      case "$pid" in
        ''|*[!0-9]*) continue ;;
      esac
      if kill -0 "$pid" 2>/dev/null; then
        kill "$pid" 2>/dev/null || true
        sleep 0.2
        kill -9 "$pid" 2>/dev/null || true
      fi
    done < <(find "$WORK_DIR" -name 'app.pid' 2>/dev/null || true)
    rm -rf "$WORK_DIR"
  fi
}

main() {
  if [ "$#" -ne 2 ]; then
    printf 'Usage: %s <path-to-install.sh> <path-to-archive.tar.gz>\n' "$0" >&2
    exit 2
  fi

  local install_arg="$1"
  local archive_arg="$2"
  [ -f "$install_arg" ] || fail "Installer script not found: $install_arg"
  [ -f "$archive_arg" ] || fail "Release archive not found: $archive_arg"

  INSTALL_SH="$(abs_path "$install_arg")"
  ARCHIVE_PATH="$(abs_path "$archive_arg")"
  detect_host_rid

  WORK_DIR="$(mktemp -d "${TMPDIR:-/tmp}/mobileshop-installer-smoke.XXXXXX")"
  S7_PID=""
  ADOPT_ROOT=""
  ADOPT_BIN=""
  ADOPT_PORT=""

  export HOME="$WORK_DIR/home"
  export XDG_CONFIG_HOME="$WORK_DIR/xdg-config"
  export XDG_DATA_HOME="$WORK_DIR/xdg-data"
  mkdir -p "$HOME" "$XDG_CONFIG_HOME" "$XDG_DATA_HOME"

  ROOT="$WORK_DIR/root"
  BIN_DIR="$WORK_DIR/bin"
  PORT="$(free_port)"
  local cookies_file="$WORK_DIR/cookies.txt"

  trap cleanup EXIT INT TERM

  # S1: Fresh install, login as admin, stop, write sentinel, start again
  MOBILESHOP_ROOT="$ROOT" MOBILESHOP_BIN_DIR="$BIN_DIR" MOBILESHOP_PORT="$PORT" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --archive "$ARCHIVE_PATH" --no-service

  poll_url_200 "http://127.0.0.1:${PORT}/Account/Login" || fail "S1 app did not return 200 on /Account/Login"
  login_and_save_cookies "$PORT" "$cookies_file" || fail "S1 admin login failed"

  MOBILESHOP_PORT="$PORT" "$BIN_DIR/mobileshop" stop >/dev/null
  set_db_sentinel "$ROOT/data/MobileShop.db"

  MOBILESHOP_PORT="$PORT" "$BIN_DIR/mobileshop" start >/dev/null
  poll_url_200 "http://127.0.0.1:${PORT}/Account/Login" || fail "S1 app did not restart cleanly"
  verify_session_cookie "$PORT" "$cookies_file" || fail "S1 session cookie failed after restart"
  pass "S1 fresh install, login, sentinel write, restart"

  # S2: Re-run reports up to date
  local orig_tag rerun_out
  orig_tag="$(read_installed_tag "$ROOT/state/installed.env")"
  [ -n "$orig_tag" ] || fail "S2 missing TAG in installed.env"
  rerun_out="$(MOBILESHOP_ROOT="$ROOT" MOBILESHOP_BIN_DIR="$BIN_DIR" MOBILESHOP_PORT="$PORT" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --archive "$ARCHIVE_PATH" --no-service)"
  printf '%s' "$rerun_out" | grep -q "Already up to date (${orig_tag})." || fail "S2 expected up-to-date message, got: $rerun_out"
  pass "S2 re-run reports up to date (${orig_tag})"

  # S3: Repackage publish folder as v9.9.9, update while session cookie exists
  verify_session_cookie "$PORT" "$cookies_file" || fail "S3 pre-update cookie check failed"
  local repack_src="$WORK_DIR/repack-src"
  local repack_out="$WORK_DIR/repack-out"
  local repack_top="" entry
  mkdir -p "$repack_src" "$repack_out"
  tar -xzf "$ARCHIVE_PATH" -C "$repack_src"
  for entry in "$repack_src"/*; do
    [ -d "$entry" ] || continue
    repack_top="$entry"
    break
  done
  [ -n "$repack_top" ] || fail "S3 could not find extracted top-level directory"

  local v9_name="MobileShop-v9.9.9-${HOST_RID}"
  local v9_archive="$WORK_DIR/${v9_name}.tar.gz"
  cp -Rp "$repack_top" "$repack_out/$v9_name"
  COPYFILE_DISABLE=1 tar -C "$repack_out" -czf "$v9_archive" "$v9_name"
  local v9_sha
  v9_sha="$(sha256_of "$v9_archive")"

  MOBILESHOP_PORT="$PORT" MOBILESHOP_BIN_DIR="$BIN_DIR" \
    "$BIN_DIR/mobileshop" update --version v9.9.9 --archive "$v9_archive" --sha256 "$v9_sha" --no-service

  [ "$(readlink "$ROOT/app/current")" = "versions/v9.9.9" ] || fail "S3 current does not point to versions/v9.9.9"
  [ -d "$ROOT/app/versions/$orig_tag" ] || fail "S3 original version folder missing: $orig_tag"
  [ -d "$ROOT/app/versions/v9.9.9" ] || fail "S3 new version folder missing: v9.9.9"
  verify_session_cookie "$PORT" "$cookies_file" || fail "S3 session cookie rejected after update"
  check_db_sentinel "$ROOT/data/MobileShop.db"
  pass "S3 update to v9.9.9 preserved cookie, sentinel, and both version folders"

  # S4: rollback --yes restores previous version and sentinel
  MOBILESHOP_PORT="$PORT" MOBILESHOP_BIN_DIR="$BIN_DIR" \
    "$BIN_DIR/mobileshop" rollback --yes --no-service
  [ "$(readlink "$ROOT/app/current")" = "versions/${orig_tag}" ] || fail "S4 rollback did not restore versions/${orig_tag}"
  poll_url_200 "http://127.0.0.1:${PORT}/Account/Login" || fail "S4 rolled-back app not answering 200"
  check_db_sentinel "$ROOT/data/MobileShop.db"
  pass "S4 rollback --yes restored ${orig_tag} with sentinel intact"

  # S5: prune --keep 1 --yes removes v9.9.9 and keeps current and data
  "$BIN_DIR/mobileshop" prune --keep 1 --yes
  [ -d "$ROOT/app/versions/$orig_tag" ] || fail "S5 prune deleted current version"
  [ ! -d "$ROOT/app/versions/v9.9.9" ] || fail "S5 prune did not remove v9.9.9"
  check_db_sentinel "$ROOT/data/MobileShop.db"
  pass "S5 prune --keep 1 --yes kept current and DB"

  # Stop primary instance cleanly before S6 copy & S8 adopt
  MOBILESHOP_PORT="$PORT" "$BIN_DIR/mobileshop" stop >/dev/null

  # S6: Direct Development launch against copy of data refuses destructive init
  local s6_data="$WORK_DIR/s6-data"
  local s6_port s6_out="$WORK_DIR/s6.out"
  cp -Rp "$ROOT/data" "$s6_data"
  s6_port="$(free_port)"

  local s6_rc=0
  if python3 - "$ROOT/app/current/MobileShop.Web" "$ROOT/app/current" "$s6_data" "$s6_port" "$s6_out" <<'PY'; then
import os, subprocess, sys
exe, content_root, data_dir, port, out_file = sys.argv[1:6]
env = os.environ.copy()
env["ASPNETCORE_ENVIRONMENT"] = "Development"
env["DOTNET_ENVIRONMENT"] = "Development"
env["ASPNETCORE_CONTENTROOT"] = content_root
env["MOBILESHOP_DATA_DIRECTORY"] = data_dir
env["ASPNETCORE_URLS"] = f"http://127.0.0.1:{port}"
env["DOTNET_BUNDLE_EXTRACT_BASE_DIR"] = os.path.join(os.path.dirname(data_dir), "s6-extract")
proc = subprocess.run(
    [exe],
    cwd=data_dir,
    env=env,
    stdout=subprocess.PIPE,
    stderr=subprocess.STDOUT,
    text=True,
    timeout=60,
)
with open(out_file, "w", encoding="utf-8") as fh:
    fh.write(proc.stdout)
sys.exit(proc.returncode)
PY
    fail "S6 expected non-zero exit when running Development against published data folder"
  else
    s6_rc=$?
  fi
  [ "$s6_rc" -ne 0 ] || fail "S6 expected non-zero exit code"
  grep -q "Refusing to run the destructive Development initialization" "$s6_out" || fail "S6 missing guard error message in output"
  check_db_sentinel "$s6_data/MobileShop.db"
  pass "S6 Development initialization refused cleanly and sentinel survived"

  # S7: Direct run with no URL variables defaults to http://localhost:5143/Account/Login
  if port_is_busy 5143; then
    printf 'Notice: port 5143 is already in use; skipping S7 default-port check.\n'
  else
    local s7_data="$WORK_DIR/s7-data"
    local s7_extract="$WORK_DIR/s7-extract"
    cp -Rp "$ROOT/data" "$s7_data"
    mkdir -p "$s7_extract"
    (
      cd "$s7_data" || exit 1
      exec env \
        -u ASPNETCORE_URLS \
        -u ASPNETCORE_HTTP_PORTS \
        -u ASPNETCORE_HTTPS_PORTS \
        -u DOTNET_URLS \
        -u urls \
        -u HTTP_PORTS \
        -u HTTPS_PORTS \
        ASPNETCORE_ENVIRONMENT=Production \
        DOTNET_ENVIRONMENT=Production \
        ASPNETCORE_CONTENTROOT="$ROOT/app/current" \
        MOBILESHOP_DATA_DIRECTORY="$s7_data" \
        DOTNET_BUNDLE_EXTRACT_BASE_DIR="$s7_extract" \
        "$ROOT/app/current/MobileShop.Web" >"$WORK_DIR/s7.log" 2>&1
    ) &
    S7_PID="$!"
    poll_url_200 "http://localhost:5143/Account/Login" || fail "S7 app did not listen on default http://localhost:5143/Account/Login"
    kill "$S7_PID" 2>/dev/null || true
    wait "$S7_PID" 2>/dev/null || true
    S7_PID=""
    pass "S7 direct run listened on default http://localhost:5143"
  fi

  # S8: Adopt into a fresh root, then update
  ADOPT_ROOT="$WORK_DIR/adopt-root"
  ADOPT_BIN="$WORK_DIR/adopt-bin"
  ADOPT_PORT="$(free_port)"

  MOBILESHOP_ROOT="$ADOPT_ROOT" MOBILESHOP_BIN_DIR="$ADOPT_BIN" MOBILESHOP_PORT="$ADOPT_PORT" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" adopt "$ROOT/data"
  [ -f "$ADOPT_ROOT/data/MobileShop.db" ] || fail "S8 adopt did not create MobileShop.db"
  check_db_sentinel "$ADOPT_ROOT/data/MobileShop.db"

  MOBILESHOP_ROOT="$ADOPT_ROOT" MOBILESHOP_BIN_DIR="$ADOPT_BIN" MOBILESHOP_PORT="$ADOPT_PORT" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" update --archive "$ARCHIVE_PATH" --no-service

  poll_url_200 "http://127.0.0.1:${ADOPT_PORT}/Account/Login" || fail "S8 adopted app did not become healthy after update"
  check_db_sentinel "$ADOPT_ROOT/data/MobileShop.db"
  MOBILESHOP_PORT="$ADOPT_PORT" "$ADOPT_BIN/mobileshop" stop >/dev/null
  pass "S8 adopt into fresh root and update succeeded"

  # S9: uninstall keeps data
  "$BIN_DIR/mobileshop" uninstall --yes
  [ ! -d "$ROOT/app" ] || fail "S9 app directory still exists after uninstall"
  [ ! -d "$ROOT/state" ] || fail "S9 state directory still exists after uninstall"
  [ ! -d "$ROOT/cache" ] || fail "S9 cache directory still exists after uninstall"
  [ ! -e "$BIN_DIR/mobileshop" ] || fail "S9 PATH symlink still exists after uninstall"
  [ -f "$ROOT/data/MobileShop.db" ] || fail "S9 uninstall removed data/MobileShop.db"
  check_db_sentinel "$ROOT/data/MobileShop.db"
  pass "S9 uninstall preserved data/MobileShop.db"

  printf 'ALL 9 RELEASE INSTALLER SCENARIOS PASSED (%s)\n' "$HOST_RID"
}

main "$@"
