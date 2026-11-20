#!/usr/bin/env bash
set -eu

init_test_env() {
  SCRIPT_DIR="$(cd -P "$(dirname "$0")" && pwd)"
  REPO_ROOT="$(cd -P "$SCRIPT_DIR/../../.." && pwd)"
  INSTALL_SH="$REPO_ROOT/src/MobileShop.Scripts/Bash/install.sh"

  TEST_TMP="$(mktemp -d "${TMPDIR:-/tmp}/mobileshop-install-tests.XXXXXX")"
  TEST_BIN="$TEST_TMP/test-bin"
  mkdir -p "$TEST_BIN"

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
        printf 'FAIL: unsupported test host %s/%s\n' "$sys" "$arch" >&2
        exit 1
      fi
      ;;
    *)
      printf 'FAIL: unsupported test host %s/%s\n' "$sys" "$arch" >&2
      exit 1
      ;;
  esac

  if [ "$sys" = "Darwin" ]; then
    cat > "$TEST_BIN/codesign" <<'CODESIGN_EOF'
#!/usr/bin/env bash
if [ "${FAKE_CODESIGN_FAIL:-0}" = "1" ]; then
  exit 1
fi
exit 0
CODESIGN_EOF
    chmod +x "$TEST_BIN/codesign"
    PATH="$TEST_BIN:$PATH"
    export PATH
  fi

  # install.sh appends slow stop/port waits here (silent when everything is fast).
  MOBILESHOP_DEBUG_FILE="$TEST_TMP/installer-debug.log"
  export MOBILESHOP_DEBUG_FILE

  SERVER_PID=""
}

cleanup_all() {
  if [ -s "${MOBILESHOP_DEBUG_FILE:-}" ]; then
    printf -- '--- installer slow-wait diagnostics ---\n'
    cat "$MOBILESHOP_DEBUG_FILE"
    printf -- '--- end diagnostics ---\n'
  fi
  if [ -n "${SERVER_PID:-}" ] && kill -0 "$SERVER_PID" 2>/dev/null; then
    kill "$SERVER_PID" 2>/dev/null || true
    wait "$SERVER_PID" 2>/dev/null || true
  fi
  if [ -d "${TEST_TMP:-}" ]; then
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
    done < <(find "$TEST_TMP" -name 'app.pid' 2>/dev/null || true)
    rm -rf "$TEST_TMP"
  fi
}

fail() {
  printf 'FAIL: %s\n' "$*" >&2
  exit 1
}

pass() {
  printf 'PASS: %s\n' "$*"
}

free_port() {
  python3 - <<'PY'
import socket
with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind(("127.0.0.1", 0))
    print(s.getsockname()[1])
PY
}

sha256_of() {
  local file="$1"
  if command -v sha256sum >/dev/null 2>&1; then
    sha256sum "$file" | awk '{print $1}'
  else
    shasum -a 256 "$file" | awk '{print $1}'
  fi
}

make_fake_archive() {
  local out_dir="$1"
  local tag="$2"
  local pending="${3:-0}"
  local fail_migrate="${4:-0}"
  local fail_health="${5:-0}"
  local mutate_on_fail="${6:-0}"

  local work="$TEST_TMP/build-${tag}-${pending}-${fail_migrate}-${fail_health}"
  local top_name="MobileShop-${tag}-${HOST_RID}"
  local app_dir="$work/$top_name"
  rm -rf "$work"
  mkdir -p "$app_dir" "$out_dir"

  cat > "$app_dir/MobileShop.Web" <<FAKE_APP_EOF
#!/usr/bin/env bash
set -eu

MARKER_TAG="${tag}"
FAKE_PENDING="${pending}"
FAKE_FAIL_MIGRATE="${fail_migrate}"
FAKE_FAIL_HEALTH="${fail_health}"
FAKE_MUTATE_ON_FAIL="${mutate_on_fail}"

if [ "\${1:-}" = "--version" ]; then
  printf 'MobileShop %s\n' "\$MARKER_TAG"
  exit 0
fi

if [ "\${1:-}" = "--migrate-database" ]; then
  data_dir="\${MOBILESHOP_DATA_DIRECTORY:-\$PWD}"
  db="\$data_dir/MobileShop.db"
  mkdir -p "\$data_dir"
  if [ "\$FAKE_FAIL_MIGRATE" = "1" ]; then
    if [ "\$FAKE_MUTATE_ON_FAIL" = "1" ] && [ -f "\$db" ]; then
      cp "\$db" "\$data_dir/MobileShop.db.20261119120001.bak"
      printf 'corrupted-before-fail\n' >> "\$db"
    fi
    exit 1
  fi
  if [ ! -f "\$db" ]; then
    printf 'db:%s\n' "\$MARKER_TAG" > "\$db"
  elif [ "\$FAKE_PENDING" = "1" ]; then
    cp "\$db" "\$data_dir/MobileShop.db.20261119120000.bak"
    printf 'migrated:%s\n' "\$MARKER_TAG" >> "\$db"
  fi
  exit 0
fi

if [ "\${1:-}" = "--backup-database" ]; then
  data_dir="\${MOBILESHOP_DATA_DIRECTORY:-\$PWD}"
  db="\$data_dir/MobileShop.db"
  [ -f "\$db" ] || exit 1
  bak="\$data_dir/MobileShop.db.20261119120500.bak"
  cp "\$db" "\$bak"
  printf 'Verified backup: %s\n' "\$bak"
  exit 0
fi

data_dir="\${MOBILESHOP_DATA_DIRECTORY:-\$PWD}"
mkdir -p "\$data_dir/logs"
printf 'log:%s\n' "\$MARKER_TAG" > "\$data_dir/logs/app-20261119.log"
{
  printf 'ASPNETCORE_ENVIRONMENT=%s\n' "\${ASPNETCORE_ENVIRONMENT:-}"
  printf 'DOTNET_ENVIRONMENT=%s\n' "\${DOTNET_ENVIRONMENT:-}"
  printf 'ASPNETCORE_CONTENTROOT=%s\n' "\${ASPNETCORE_CONTENTROOT:-}"
  printf 'MOBILESHOP_DATA_DIRECTORY=%s\n' "\${MOBILESHOP_DATA_DIRECTORY:-}"
  printf 'ASPNETCORE_URLS=%s\n' "\${ASPNETCORE_URLS:-}"
  printf 'DOTNET_BUNDLE_EXTRACT_BASE_DIR=%s\n' "\${DOTNET_BUNDLE_EXTRACT_BASE_DIR:-}"
  printf 'PWD=%s\n' "\$PWD"
  printf 'TAG=%s\n' "\$MARKER_TAG"
} > "\$data_dir/last-run.env"

if [ "\$FAKE_FAIL_HEALTH" = "1" ]; then
  exit 1
fi

port="\${ASPNETCORE_URLS##*:}"
exec python3 - "\$port" "\$MARKER_TAG" <<'PY_SERVER'
import http.server
import sys

port = int(sys.argv[1])
tag = sys.argv[2]

class Handler(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path == "/Account/Login":
            body = f"Login {tag}".encode("utf-8")
            self.send_response(200)
            self.send_header("Content-Type", "text/plain")
            self.send_header("Content-Length", str(len(body)))
            self.end_headers()
            self.wfile.write(body)
        else:
            self.send_response(404)
            self.end_headers()
    def log_message(self, format, *args):
        pass

import socketserver

class FastServer(http.server.HTTPServer):
    # HTTPServer.server_bind() calls socket.getfqdn(), whose reverse lookup stalls ~35s on macOS runners.
    def server_bind(self):
        socketserver.TCPServer.server_bind(self)
        self.server_name = "localhost"
        self.server_port = self.server_address[1]

FastServer(("127.0.0.1", port), Handler).serve_forever()
PY_SERVER
FAKE_APP_EOF
  chmod +x "$app_dir/MobileShop.Web"

  local arc_name="MobileShop-${tag}-${HOST_RID}.tar.gz"
  tar -C "$work" -czf "$out_dir/$arc_name" "$top_name"
  local sum
  sum="$(sha256_of "$out_dir/$arc_name")"
  printf '%s  %s\n' "$sum" "$arc_name" >> "$out_dir/checksums.txt"
}

run_case() {
  local name="$1" started="$SECONDS"
  shift
  "$@"
  pass "$name ($((SECONDS - started))s)"
}

test_t1_platform_refusal() {
  local root="$TEST_TMP/t1-root"
  local rc=0
  if MOBILESHOP_ROOT="$root" MOBILESHOP_UNAME_S="Linux" MOBILESHOP_UNAME_M="aarch64" \
     bash "$INSTALL_SH" install --version v1.0.0 >/dev/null 2>"$TEST_TMP/t1.err"; then
    fail "T1 expected refusal on Linux aarch64"
  else
    rc=$?
  fi
  [ "$rc" -eq 2 ] || fail "T1 expected exit 2 on Linux aarch64, got $rc"
  grep -q 'Unsupported platform:' "$TEST_TMP/t1.err" || fail "T1 missing unsupported platform message"

  rc=0
  if MOBILESHOP_ROOT="$root" MOBILESHOP_UNAME_S="Darwin" MOBILESHOP_UNAME_M="x86_64" \
     bash "$INSTALL_SH" install --version v1.0.0 >/dev/null 2>"$TEST_TMP/t1b.err"; then
    fail "T1 expected refusal on Darwin x86_64 override"
  else
    rc=$?
  fi
  [ "$rc" -eq 2 ] || fail "T1 expected exit 2 on Darwin x86_64, got $rc"
}

test_t2_t3_t4_t8_t9_t12_t14_lifecycle() {
  local root="$TEST_TMP/root with spaces"
  local bin_dir="$TEST_TMP/bin dir"
  local dist="$TEST_TMP/dist-main"
  local port
  port="$(free_port)"

  make_fake_archive "$dist" "v1.0.0" 0 0 0 0
  make_fake_archive "$dist" "v1.1.0" 0 0 0 0
  make_fake_archive "$dist" "v1.2.0" 1 0 0 0

  # T2: Fresh install
  MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.0.0 --archive "$dist/MobileShop-v1.0.0-${HOST_RID}.tar.gz" --no-service >/dev/null

  [ -d "$root/app/versions/v1.0.0" ] || fail "T2 missing version v1.0.0"
  [ -L "$root/app/current" ] || fail "T2 missing current symlink"
  [ "$(readlink "$root/app/current")" = "versions/v1.0.0" ] || fail "T2 current symlink not relative versions/v1.0.0"
  [ -x "$root/app/bin/mobileshop" ] || fail "T2 missing launcher"
  [ -L "$bin_dir/mobileshop" ] || fail "T2 missing PATH symlink"
  [ -f "$root/state/installed.env" ] || fail "T2 missing installed.env"
  [ -f "$root/state/install.log" ] || fail "T2 missing install.log"
  [ -f "$root/data/MobileShop.db" ] || fail "T2 missing MobileShop.db"
  curl -fsS "http://127.0.0.1:${port}/Account/Login" | grep -q 'Login v1.0.0' || fail "T2 app not healthy"

  # T14: Verify launcher pins Production even when ASPNETCORE_ENVIRONMENT=Development is exported
  [ -f "$root/data/last-run.env" ] || fail "T14 missing last-run.env"
  grep -q '^ASPNETCORE_ENVIRONMENT=Production$' "$root/data/last-run.env" || fail "T14 ASPNETCORE_ENVIRONMENT not Production"
  grep -q '^DOTNET_ENVIRONMENT=Production$' "$root/data/last-run.env" || fail "T14 DOTNET_ENVIRONMENT not Production"
  grep -q "^ASPNETCORE_CONTENTROOT=${root}/app/current$" "$root/data/last-run.env" || fail "T14 content root mismatch"
  grep -q "^MOBILESHOP_DATA_DIRECTORY=${root}/data$" "$root/data/last-run.env" || fail "T14 data dir mismatch"
  grep -q "^ASPNETCORE_URLS=http://localhost:${port}$" "$root/data/last-run.env" || fail "T14 URLs mismatch"
  grep -q "^DOTNET_BUNDLE_EXTRACT_BASE_DIR=${root}/cache/extract$" "$root/data/last-run.env" || fail "T14 extract dir mismatch"
  # Compare physical paths: macOS resolves /tmp -> /private/tmp in $PWD after cd.
  actual_pwd="$(grep '^PWD=' "$root/data/last-run.env" | cut -d= -f2-)"
  expected_pwd="$(cd "${root}/data" && pwd -P)"
  [ -n "$actual_pwd" ] || fail "T14 missing PWD in last-run.env"
  [ "$(cd "$actual_pwd" 2>/dev/null && pwd -P)" = "$expected_pwd" ] || fail "T14 cwd mismatch: got [$actual_pwd] expected physical [$expected_pwd]"

  local dev_env_out
  dev_env_out="$(ASPNETCORE_ENVIRONMENT=Development MOBILESHOP_PORT="$port" "$bin_dir/mobileshop" run --version)"
  [ "$dev_env_out" = "MobileShop v1.0.0" ] || fail "T14 launcher run --version failed: $dev_env_out"

  # T12 + status/version/backup: doctor exits 0 when healthy, backup writes a .bak
  MOBILESHOP_PORT="$port" MOBILESHOP_BIN_DIR="$bin_dir" "$bin_dir/mobileshop" doctor >/dev/null || fail "T12 doctor expected 0 when healthy"
  MOBILESHOP_PORT="$port" "$bin_dir/mobileshop" status | grep -q 'Status: running' || fail "status expected running"
  "$bin_dir/mobileshop" version | grep -q 'Installed: v1.0.0' || fail "version expected Installed: v1.0.0"
  "$bin_dir/mobileshop" backup >/dev/null
  [ -f "$root/data/MobileShop.db.20261119120500.bak" ] || fail "backup did not create .bak file"

  # T3: Re-run same tag without --force says up to date
  local rerun_out
  rerun_out="$(MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.0.0 --archive "$dist/MobileShop-v1.0.0-${HOST_RID}.tar.gz" --no-service)"
  printf '%s' "$rerun_out" | grep -q 'Already up to date (v1.0.0).' || fail "T3 expected Already up to date message"

  # T4 part 1: update v1.0.0 -> v1.1.0 with no migration pending preserves DB bytes
  local db_before db_after
  db_before="$(sha256_of "$root/data/MobileShop.db")"
  MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.1.0 --archive "$dist/MobileShop-v1.1.0-${HOST_RID}.tar.gz" --no-service >/dev/null
  db_after="$(sha256_of "$root/data/MobileShop.db")"
  [ "$db_before" = "$db_after" ] || fail "T4 DB changed when no migration pending"
  curl -fsS "http://127.0.0.1:${port}/Account/Login" | grep -q 'Login v1.1.0' || fail "T4 v1.1.0 not running"

  # T4 part 2: update v1.1.0 -> v1.2.0 with FAKE_PENDING=1 updates DB and records LAST_BACKUP
  MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.2.0 --archive "$dist/MobileShop-v1.2.0-${HOST_RID}.tar.gz" --no-service >/dev/null
  grep -q '^migrated:v1.2.0$' "$root/data/MobileShop.db" || fail "T4 v1.2.0 did not migrate DB"
  grep -q '^LAST_BACKUP=.*MobileShop\.db\..*\.bak$' "$root/state/installed.env" || fail "T4 LAST_BACKUP not recorded"
  curl -fsS "http://127.0.0.1:${port}/Account/Login" | grep -q 'Login v1.2.0' || fail "T4 v1.2.0 not running"

  # T8: rollback --yes from v1.2.0 back to v1.1.0 restores LAST_BACKUP and clears LAST_BACKUP
  MOBILESHOP_PORT="$port" MOBILESHOP_BIN_DIR="$bin_dir" "$bin_dir/mobileshop" rollback --yes --no-service >/dev/null
  [ "$(readlink "$root/app/current")" = "versions/v1.1.0" ] || fail "T8 rollback did not switch current to v1.1.0"
  if grep -q 'migrated:v1.2.0' "$root/data/MobileShop.db"; then
    fail "T8 rollback did not restore DB backup"
  fi
  grep -q '^TAG=v1.1.0$' "$root/state/installed.env" || fail "T8 TAG mismatch"
  grep -q '^PREVIOUS=v1.2.0$' "$root/state/installed.env" || fail "T8 PREVIOUS mismatch"
  grep -q '^LAST_BACKUP=$' "$root/state/installed.env" || fail "T8 LAST_BACKUP not cleared"
  curl -fsS "http://127.0.0.1:${port}/Account/Login" | grep -q 'Login v1.1.0' || fail "T8 v1.1.0 not healthy after rollback"

  # T9: prune --keep 1 --yes keeps only current (v1.1.0) and preserves data/
  "$bin_dir/mobileshop" prune --keep 1 --yes >/dev/null
  [ -d "$root/app/versions/v1.1.0" ] || fail "T9 prune deleted current version"
  [ ! -d "$root/app/versions/v1.0.0" ] || fail "T9 prune kept v1.0.0"
  [ ! -d "$root/app/versions/v1.2.0" ] || fail "T9 prune kept v1.2.0"
  [ -f "$root/data/MobileShop.db" ] || fail "T9 prune touched data"

  # T12 failure path: stop app and verify doctor exits 1
  MOBILESHOP_PORT="$port" "$bin_dir/mobileshop" stop >/dev/null
  if MOBILESHOP_PORT="$port" MOBILESHOP_BIN_DIR="$bin_dir" "$bin_dir/mobileshop" doctor >/dev/null; then
    fail "T12 doctor expected exit 1 when stopped"
  fi

  # T11: uninstall keeps data; --purge --yes removes data and ROOT
  "$bin_dir/mobileshop" uninstall --yes >/dev/null
  [ ! -d "$root/app" ] || fail "T11 app still exists after uninstall"
  [ ! -d "$root/state" ] || fail "T11 state still exists after uninstall"
  [ ! -d "$root/cache" ] || fail "T11 cache still exists after uninstall"
  [ ! -e "$bin_dir/mobileshop" ] || fail "T11 symlink not removed"
  [ -f "$root/data/MobileShop.db" ] || fail "T11 uninstall deleted data"

  MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.1.0 --archive "$dist/MobileShop-v1.1.0-${HOST_RID}.tar.gz" --no-service >/dev/null
  "$bin_dir/mobileshop" uninstall --purge --yes >/dev/null
  [ ! -e "$root" ] || fail "T11 --purge --yes did not remove ROOT"
}

test_t5_migrate_failure_rollback() {
  local root="$TEST_TMP/t5-root"
  local bin_dir="$TEST_TMP/t5-bin"
  local dist="$TEST_TMP/dist-t5"
  local port
  port="$(free_port)"

  make_fake_archive "$dist" "v1.0.0" 0 0 0 0
  make_fake_archive "$dist" "v2.0.0" 1 1 0 1

  MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.0.0 --archive "$dist/MobileShop-v1.0.0-${HOST_RID}.tar.gz" --no-service >/dev/null

  local db_before rc=0
  db_before="$(sha256_of "$root/data/MobileShop.db")"
  if MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
     bash "$INSTALL_SH" install --version v2.0.0 --archive "$dist/MobileShop-v2.0.0-${HOST_RID}.tar.gz" --no-service >/dev/null 2>&1; then
    fail "T5 expected exit 5 on migrate failure"
  else
    rc=$?
  fi
  [ "$rc" -eq 5 ] || fail "T5 expected exit 5, got $rc"
  [ "$(readlink "$root/app/current")" = "versions/v1.0.0" ] || fail "T5 current changed on migrate fail"
  [ "$(sha256_of "$root/data/MobileShop.db")" = "$db_before" ] || fail "T5 DB bytes changed after migrate rollback"
  curl -fsS "http://127.0.0.1:${port}/Account/Login" | grep -q 'Login v1.0.0' || fail "T5 old version not restarted"

  MOBILESHOP_PORT="$port" "$bin_dir/mobileshop" stop >/dev/null
}

test_t6_health_failure_rollback() {
  local root="$TEST_TMP/t6-root"
  local bin_dir="$TEST_TMP/t6-bin"
  local dist="$TEST_TMP/dist-t6"
  local port
  port="$(free_port)"

  make_fake_archive "$dist" "v1.0.0" 0 0 0 0
  make_fake_archive "$dist" "v2.0.0" 1 0 1 0

  MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.0.0 --archive "$dist/MobileShop-v1.0.0-${HOST_RID}.tar.gz" --no-service >/dev/null

  local db_before rc=0
  db_before="$(sha256_of "$root/data/MobileShop.db")"
  if MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
     bash "$INSTALL_SH" install --version v2.0.0 --archive "$dist/MobileShop-v2.0.0-${HOST_RID}.tar.gz" --no-service >/dev/null 2>&1; then
    fail "T6 expected exit 5 on health failure"
  else
    rc=$?
  fi
  [ "$rc" -eq 5 ] || fail "T6 expected exit 5, got $rc"
  [ "$(readlink "$root/app/current")" = "versions/v1.0.0" ] || fail "T6 current not reverted"
  [ "$(sha256_of "$root/data/MobileShop.db")" = "$db_before" ] || fail "T6 DB bytes not restored"
  curl -fsS "http://127.0.0.1:${port}/Account/Login" | grep -q 'Login v1.0.0' || fail "T6 v1.0.0 not healthy after rollback"

  MOBILESHOP_PORT="$port" "$bin_dir/mobileshop" stop >/dev/null
}

test_t7_checksum_mismatch() {
  local root="$TEST_TMP/t7-root"
  local bin_dir="$TEST_TMP/t7-bin"
  local dist="$TEST_TMP/dist-t7"
  local port
  port="$(free_port)"

  make_fake_archive "$dist" "v1.0.0" 0 0 0 0
  make_fake_archive "$dist" "v2.0.0" 0 0 0 0

  MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.0.0 --archive "$dist/MobileShop-v1.0.0-${HOST_RID}.tar.gz" --no-service >/dev/null

  local pid_before rc=0
  pid_before="$(cat "$root/state/app.pid")"

  if MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
     bash "$INSTALL_SH" install --version v2.0.0 --archive "$dist/MobileShop-v2.0.0-${HOST_RID}.tar.gz" \
     --sha256 "0000000000000000000000000000000000000000000000000000000000000000" --no-service >/dev/null 2>&1; then
    fail "T7 expected exit 3 on checksum mismatch"
  else
    rc=$?
  fi
  [ "$rc" -eq 3 ] || fail "T7 expected exit 3, got $rc"
  [ "$(cat "$root/state/app.pid")" = "$pid_before" ] || fail "T7 modified running process"
  [ "$(readlink "$root/app/current")" = "versions/v1.0.0" ] || fail "T7 modified current symlink"

  MOBILESHOP_PORT="$port" "$bin_dir/mobileshop" stop >/dev/null
}

test_t10_adopt() {
  local root="$TEST_TMP/t10-root"
  local old_dir="$TEST_TMP/t10-old"
  local rc=0
  mkdir -p "$old_dir"
  printf 'existing-db-bytes\n' > "$old_dir/MobileShop.db"

  MOBILESHOP_ROOT="$root" MOBILESHOP_SERVICE="none" bash "$INSTALL_SH" adopt "$old_dir" >/dev/null
  cmp -s "$old_dir/MobileShop.db" "$root/data/MobileShop.db" || fail "T10 copied DB mismatch"

  # Refuses when target DB already exists
  rc=0
  if MOBILESHOP_ROOT="$root" MOBILESHOP_SERVICE="none" bash "$INSTALL_SH" adopt "$old_dir" >/dev/null 2>&1; then
    fail "T10 expected refusal when target DB exists"
  else
    rc=$?
  fi
  [ "$rc" -eq 6 ] || fail "T10 expected exit 6 when target DB exists, got $rc"

  # Refuses when source WAL is non-empty
  local root2="$TEST_TMP/t10-root2"
  printf 'dirty-wal' > "$old_dir/MobileShop.db-wal"
  rc=0
  if MOBILESHOP_ROOT="$root2" MOBILESHOP_SERVICE="none" bash "$INSTALL_SH" adopt "$old_dir" >/dev/null 2>&1; then
    fail "T10 expected refusal when WAL is non-empty"
  else
    rc=$?
  fi
  [ "$rc" -eq 6 ] || fail "T10 expected exit 6 when WAL is non-empty, got $rc"
}

test_t13_redirect_server_and_update() {
  local root="$TEST_TMP/t13-root"
  local bin_dir="$TEST_TMP/t13-bin"
  local dist="$TEST_TMP/dist-t13"
  local app_port srv_port
  app_port="$(free_port)"
  srv_port="$(free_port)"

  make_fake_archive "$dist" "v1.0.0" 0 0 0 0
  make_fake_archive "$dist" "v2.0.0" 1 0 0 0

  cp "$INSTALL_SH" "$dist/install.sh"
  printf '%s  %s\n' "$(sha256_of "$dist/install.sh")" "install.sh" >> "$dist/checksums.txt"

  python3 - "$srv_port" "$dist" "v2.0.0" >"$TEST_TMP/t13-server.log" 2>&1 <<'PY_GH' &
import http.server
import os
import sys

port = int(sys.argv[1])
dist = sys.argv[2]
latest_tag = sys.argv[3]
repo_prefix = "/migueljocode/MobileShop"

class GitHubMock(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path == f"{repo_prefix}/releases/latest":
            self.send_response(302)
            self.send_header("Location", f"{repo_prefix}/releases/tag/{latest_tag}")
            self.end_headers()
            return
        if self.path == f"{repo_prefix}/releases/tag/{latest_tag}":
            self.send_response(200)
            self.end_headers()
            self.wfile.write(b"ok")
            return
        if self.path.startswith(f"{repo_prefix}/releases/latest/download/"):
            name = self.path.split("/")[-1]
            self.send_response(302)
            self.send_header("Location", f"{repo_prefix}/releases/download/{latest_tag}/{name}")
            self.end_headers()
            return
        prefix = f"{repo_prefix}/releases/download/{latest_tag}/"
        if self.path.startswith(prefix):
            name = self.path[len(prefix):]
            fpath = os.path.join(dist, name)
            if os.path.isfile(fpath):
                data = open(fpath, "rb").read()
                self.send_response(200)
                self.send_header("Content-Length", str(len(data)))
                self.end_headers()
                self.wfile.write(data)
                return
        self.send_response(404)
        self.end_headers()
    def log_message(self, format, *args):
        pass

import socketserver

class FastServer(http.server.HTTPServer):
    # Skip socket.getfqdn() in server_bind(): its reverse lookup stalls ~35s on macOS runners.
    def server_bind(self):
        socketserver.TCPServer.server_bind(self)
        self.server_name = "localhost"
        self.server_port = self.server_address[1]

FastServer(("127.0.0.1", port), GitHubMock).serve_forever()
PY_GH
  SERVER_PID="$!"

  # Wait until mock server responds
  local i=0
  while [ "$i" -lt 30 ]; do
    if curl -s -o /dev/null "http://127.0.0.1:${srv_port}/migueljocode/MobileShop/releases/tag/v2.0.0"; then
      break
    fi
    sleep 0.2
    i=$((i + 1))
  done

  # First install v1.0.0 locally
  MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$app_port" MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.0.0 --archive "$dist/MobileShop-v1.0.0-${HOST_RID}.tar.gz" --no-service >/dev/null

  # Run `mobileshop update` which resolves latest from mock redirect server, verifies install.sh, and updates to v2.0.0
  MOBILESHOP_BASE_URL="http://127.0.0.1:${srv_port}" MOBILESHOP_PORT="$app_port" MOBILESHOP_BIN_DIR="$bin_dir" \
    "$bin_dir/mobileshop" update --no-service >/dev/null

  [ "$(readlink "$root/app/current")" = "versions/v2.0.0" ] || fail "T13 update did not install latest v2.0.0"
  curl -fsS "http://127.0.0.1:${app_port}/Account/Login" | grep -q 'Login v2.0.0' || fail "T13 v2.0.0 not healthy"

  MOBILESHOP_PORT="$app_port" "$bin_dir/mobileshop" stop >/dev/null
  kill "$SERVER_PID" 2>/dev/null || true
  wait "$SERVER_PID" 2>/dev/null || true
  SERVER_PID=""
}

test_t15_lock_refusal() {
  local root="$TEST_TMP/t15-root"
  local dist="$TEST_TMP/dist-t15"
  local port
  port="$(free_port)"
  make_fake_archive "$dist" "v1.0.0" 0 0 0 0
  mkdir -p "$root/state/lock"

  local rc=0
  if MOBILESHOP_ROOT="$root" MOBILESHOP_PORT="$port" MOBILESHOP_SERVICE="none" \
     bash "$INSTALL_SH" install --version v1.0.0 --archive "$dist/MobileShop-v1.0.0-${HOST_RID}.tar.gz" --no-service >/dev/null 2>&1; then
    fail "T15 expected lock refusal"
  else
    rc=$?
  fi
  [ "$rc" -eq 6 ] || fail "T15 expected exit 6 when locked, got $rc"
  [ -d "$root/state/lock" ] || fail "T15 second installer deleted pre-existing lock!"
}

test_t16_ui_progress_eta_and_nocolor() {
  local root="$TEST_TMP/t16-root"
  local bin_dir="$TEST_TMP/t16-bin"
  local dist="$TEST_TMP/dist-t16"
  local nocolor_root="$TEST_TMP/t16-nocolor-root"
  local nocolor_bin="$TEST_TMP/t16-nocolor-bin"
  local plain_root="$TEST_TMP/t16-plain-root"
  local plain_bin="$TEST_TMP/t16-plain-bin"
  local app_port srv_port nocolor_port plain_port
  app_port="$(free_port)"
  srv_port="$(free_port)"
  nocolor_port="$(free_port)"
  plain_port="$(free_port)"

  make_fake_archive "$dist" "v1.0.0" 0 0 0 0
  # Add an incompressible ~180 KB payload inside a custom archive so curl streams multiple progress frames
  python3 - "$dist" "MobileShop-v1.0.0-${HOST_RID}" <<'PY_BIG'
import hashlib, os, sys, tarfile, tempfile
dist, top_name = sys.argv[1:3]
tar_name = f"{top_name}.tar.gz"
tar_path = os.path.join(dist, tar_name)
with tempfile.TemporaryDirectory() as tmpdir:
    with tarfile.open(tar_path, "r:gz") as tf:
        tf.extractall(tmpdir)
    with open(os.path.join(tmpdir, top_name, "payload.bin"), "wb") as f:
        f.write(os.urandom(180 * 1024))
    with tarfile.open(tar_path, "w:gz") as tf:
        tf.add(os.path.join(tmpdir, top_name), arcname=top_name)
h = hashlib.sha256(open(tar_path, "rb").read()).hexdigest()
with open(os.path.join(dist, "checksums.txt"), "w", encoding="utf-8") as f:
    f.write(f"{h}  {tar_name}\n")
PY_BIG

  python3 - "$srv_port" "$dist" "v1.0.0" >"$TEST_TMP/t16-server.log" 2>&1 <<'PY_SRV' &
import http.server, os, sys, time
port = int(sys.argv[1])
dist = sys.argv[2]
latest_tag = sys.argv[3]
repo_prefix = "/migueljocode/MobileShop"

class UiMock(http.server.BaseHTTPRequestHandler):
    def do_HEAD(self):
        prefix = f"{repo_prefix}/releases/download/{latest_tag}/"
        if self.path.startswith(prefix):
            name = self.path[len(prefix):]
            fpath = os.path.join(dist, name)
            if os.path.isfile(fpath):
                self.send_response(200)
                self.send_header("Content-Length", str(os.path.getsize(fpath)))
                self.end_headers()
                return
        self.send_response(404)
        self.end_headers()

    def do_GET(self):
        if self.path == f"{repo_prefix}/releases/latest":
            self.send_response(302)
            self.send_header("Location", f"{repo_prefix}/releases/tag/{latest_tag}")
            self.end_headers()
            return
        if self.path == f"{repo_prefix}/releases/tag/{latest_tag}":
            self.send_response(200)
            self.end_headers()
            self.wfile.write(b"ok")
            return
        prefix = f"{repo_prefix}/releases/download/{latest_tag}/"
        if self.path.startswith(prefix):
            name = self.path[len(prefix):]
            fpath = os.path.join(dist, name)
            if os.path.isfile(fpath):
                with open(fpath, "rb") as fh:
                    data = fh.read()
                self.send_response(200)
                self.send_header("Content-Length", str(len(data)))
                self.end_headers()
                chunk = 32768
                for i in range(0, len(data), chunk):
                    self.wfile.write(data[i:i + chunk])
                    self.wfile.flush()
                    if len(data) > 50000:
                        time.sleep(0.12)
                return
        self.send_response(404)
        self.end_headers()

    def log_message(self, format, *args):
        pass

import socketserver

class FastServer(http.server.HTTPServer):
    # Skip socket.getfqdn() in server_bind(): its reverse lookup stalls ~35s on macOS runners.
    def server_bind(self):
        socketserver.TCPServer.server_bind(self)
        self.server_name = "localhost"
        self.server_port = self.server_address[1]

FastServer(("127.0.0.1", port), UiMock).serve_forever()
PY_SRV
  SERVER_PID="$!"

  local i=0
  while [ "$i" -lt 30 ]; do
    if curl -s -o /dev/null "http://127.0.0.1:${srv_port}/migueljocode/MobileShop/releases/tag/v1.0.0"; then
      break
    fi
    sleep 0.2
    i=$((i + 1))
  done

  local ui_out="$TEST_TMP/t16-ui.out"
  local ui_err="$TEST_TMP/t16-ui.err"
  MOBILESHOP_ROOT="$root" MOBILESHOP_BIN_DIR="$bin_dir" MOBILESHOP_PORT="$app_port" \
    MOBILESHOP_SERVICE="none" MOBILESHOP_BASE_URL="http://127.0.0.1:${srv_port}" \
    MOBILESHOP_FORCE_UI="1" COLORTERM="truecolor" TERM="xterm-256color" LANG="C.UTF-8" \
    bash "$INSTALL_SH" install --no-service >"$ui_out" 2>"$ui_err"

  python3 - "$ui_out" "$ui_err" <<'PY_CHK_UI'
import re, sys
out = open(sys.argv[1], "r", encoding="utf-8", errors="replace").read()
err = open(sys.argv[2], "r", encoding="utf-8", errors="replace").read()
assert err == "", f"T16 expected empty stderr in UI mode, got: {err!r}"
assert "\x1b[38;2;" in out, "T16 expected TrueColor RGB gradient ANSI escape sequences"
plain = re.sub(r"\x1b\[[0-9;?]*[A-Za-z]", "", out)
for token in ["MobileShop", "[1/6]", "[6/6]", "100%", "ETA ", "Installed MobileShop v1.0.0"]:
    assert token in plain, f"T16 missing UI token {token!r}"
PY_CHK_UI
  MOBILESHOP_PORT="$app_port" "$bin_dir/mobileshop" stop >/dev/null

  # Check NO_COLOR=1 emits zero ANSI escapes even when MOBILESHOP_FORCE_UI=1
  local nc_out="$TEST_TMP/t16-nc.out"
  local nc_err="$TEST_TMP/t16-nc.err"
  MOBILESHOP_ROOT="$nocolor_root" MOBILESHOP_BIN_DIR="$nocolor_bin" MOBILESHOP_PORT="$nocolor_port" \
    MOBILESHOP_SERVICE="none" MOBILESHOP_FORCE_UI="1" NO_COLOR="1" \
    bash "$INSTALL_SH" install --version v1.0.0 --archive "$dist/MobileShop-v1.0.0-${HOST_RID}.tar.gz" --no-service >"$nc_out" 2>"$nc_err"
  python3 - "$nc_out" "$nc_err" <<'PY_CHK_NC'
import sys
out = open(sys.argv[1], "r", encoding="utf-8", errors="replace").read()
err = open(sys.argv[2], "r", encoding="utf-8", errors="replace").read()
# Expect NO color/SGR sequences when NO_COLOR=1 (cursor/line-clear escapes still allowed in TTY mode only; NO SGR m sequences)
import re
sgr = re.findall(r"\x1b\[[0-9;]*m", out + err)
assert not sgr, f"T16 expected zero SGR color escapes under NO_COLOR=1, found: {sgr[:5]}"
PY_CHK_NC
  MOBILESHOP_PORT="$nocolor_port" "$nocolor_bin/mobileshop" stop >/dev/null

  # Check piped non-TTY run emits zero ANSI escapes anywhere
  local pl_out="$TEST_TMP/t16-pl.out"
  local pl_err="$TEST_TMP/t16-pl.err"
  MOBILESHOP_ROOT="$plain_root" MOBILESHOP_BIN_DIR="$plain_bin" MOBILESHOP_PORT="$plain_port" \
    MOBILESHOP_SERVICE="none" \
    bash "$INSTALL_SH" install --version v1.0.0 --archive "$dist/MobileShop-v1.0.0-${HOST_RID}.tar.gz" --no-service >"$pl_out" 2>"$pl_err"
  python3 - "$pl_out" "$pl_err" <<'PY_CHK_PL'
import sys
out = open(sys.argv[1], "r", encoding="utf-8", errors="replace").read()
err = open(sys.argv[2], "r", encoding="utf-8", errors="replace").read()
assert "\x1b" not in out and "\x1b" not in err, "T16 expected zero ESC bytes in non-TTY piped mode"
assert "Installed MobileShop v1.0.0" in out, "T16 missing plain install confirmation"
PY_CHK_PL
  MOBILESHOP_PORT="$plain_port" "$plain_bin/mobileshop" stop >/dev/null

  kill "$SERVER_PID" 2>/dev/null || true
  wait "$SERVER_PID" 2>/dev/null || true
  SERVER_PID=""
}

main() {
  init_test_env
  trap cleanup_all EXIT INT TERM

  run_case "T1 platform refusal" test_t1_platform_refusal
  run_case "T2/T3/T4/T8/T9/T11/T12/T14 lifecycle & launcher" test_t2_t3_t4_t8_t9_t12_t14_lifecycle
  run_case "T5 migrate failure rollback" test_t5_migrate_failure_rollback
  run_case "T6 health failure rollback" test_t6_health_failure_rollback
  run_case "T7 checksum mismatch" test_t7_checksum_mismatch
  run_case "T10 adopt" test_t10_adopt
  run_case "T13 redirect server & update" test_t13_redirect_server_and_update
  run_case "T15 lock refusal" test_t15_lock_refusal
  run_case "T16 animated UI, gradient banner, ETA & NO_COLOR" test_t16_ui_progress_eta_and_nocolor

  printf 'ALL 16 INSTALLER SCENARIOS PASSED (%s)\n' "$HOST_RID"
}

main "$@"
