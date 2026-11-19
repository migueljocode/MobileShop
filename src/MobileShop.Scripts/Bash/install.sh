#!/usr/bin/env bash
set -eu

init_globals() {
  MOBILESHOP_VERSION="__VERSION__"

  CLI_ROOT="${MOBILESHOP_ROOT:-}"
  CLI_BIN_DIR="${MOBILESHOP_BIN_DIR:-$HOME/.local/bin}"
  CLI_PORT="${MOBILESHOP_PORT:-5143}"
  CLI_SERVICE="${MOBILESHOP_SERVICE:-auto}"
  CLI_REPO="${MOBILESHOP_REPO:-migueljocode/MobileShop}"
  CLI_BASE_URL="${MOBILESHOP_BASE_URL:-https://github.com}"
  CLI_BASE_URL="${CLI_BASE_URL%/}"

  SYS_NAME=""
  ARCH_NAME=""
  RID=""
  OS_KIND=""

  SELF_PATH=""
  ROOT=""
  APP_DIR=""
  VERSIONS_DIR=""
  CURRENT_LINK=""
  BIN_DIR_APP=""
  LAUNCHER=""
  DATA_DIR=""
  LOGS_DIR=""
  STATE_DIR=""
  CACHE_DIR=""
  DOWNLOADS_DIR=""
  EXTRACT_DIR=""
  INSTALLED_ENV=""
  INSTALL_LOG=""
  LOCK_DIR=""
  PID_FILE=""

  LOCK_ACQUIRED=0

  INST_TAG=""
  INST_PREVIOUS=""
  INST_RID=""
  INST_LAST_BACKUP=""
  INST_SERVICE_MODE=""
}

installer_version_label() {
  local placeholder="__""VERSION__"
  if [ "$MOBILESHOP_VERSION" = "$placeholder" ] || [ -z "$MOBILESHOP_VERSION" ]; then
    printf 'dev\n'
  else
    printf '%s\n' "$MOBILESHOP_VERSION"
  fi
}

log_msg() {
  local level="$1"
  shift
  local msg="$*"
  local stamp
  stamp="$(date '+%Y-%m-%d %H:%M:%S' 2>/dev/null || printf 'unknown-time')"
  if [ -n "${INSTALL_LOG:-}" ] && [ -d "${STATE_DIR:-}" ]; then
    printf '[%s] [%s] %s\n' "$stamp" "$level" "$msg" >> "$INSTALL_LOG" 2>/dev/null || true
  fi
}

info() {
  log_msg "INFO" "$@"
  printf '%s\n' "$*"
}

warn() {
  log_msg "WARN" "$@"
  printf 'Warning: %s\n' "$*" >&2
}

die() {
  local code="$1"
  shift
  log_msg "ERROR" "$@"
  printf 'Error: %s\n' "$*" >&2
  exit "$code"
}

resolve_script_path() {
  local src="$1"
  local dir link
  while [ -L "$src" ]; do
    dir="$(cd -P "$(dirname "$src")" && pwd)" || return 1
    link="$(readlink "$src")"
    case "$link" in
      /*) src="$link" ;;
      *) src="$dir/$link" ;;
    esac
  done
  dir="$(cd -P "$(dirname "$src")" && pwd)" || return 1
  printf '%s/%s\n' "$dir" "$(basename "$src")"
}

detect_platform() {
  SYS_NAME="${MOBILESHOP_UNAME_S:-$(uname -s)}"
  ARCH_NAME="${MOBILESHOP_UNAME_M:-$(uname -m)}"

  case "$SYS_NAME:$ARCH_NAME" in
    Linux:x86_64)
      RID="linux-x64"
      OS_KIND="linux"
      ;;
    Darwin:arm64)
      RID="osx-arm64"
      OS_KIND="macos"
      ;;
    Darwin:x86_64)
      if [ -z "${MOBILESHOP_UNAME_M:-}" ] && [ "$(sysctl -n hw.optional.arm64 2>/dev/null || printf '0')" = "1" ]; then
        RID="osx-arm64"
        OS_KIND="macos"
      else
        die 2 "Unsupported platform: this release supports Linux x86_64 and macOS Apple Silicon only."
      fi
      ;;
    *)
      die 2 "Unsupported platform: this release supports Linux x86_64 and macOS Apple Silicon only."
      ;;
  esac
}

detect_os_hint() {
  local sys="${MOBILESHOP_UNAME_S:-$(uname -s)}"
  case "$sys" in
    Darwin) OS_KIND="macos" ;;
    *) OS_KIND="linux" ;;
  esac
}

init_paths() {
  local script_arg="$1"
  SELF_PATH="$(resolve_script_path "$script_arg")"

  if [ -n "$CLI_ROOT" ]; then
    if [ "$CLI_ROOT" != "/" ]; then
      ROOT="${CLI_ROOT%/}"
    else
      ROOT="/"
    fi
  else
    local bin_parent app_parent
    bin_parent="$(basename "$(dirname "$SELF_PATH")")"
    app_parent="$(basename "$(dirname "$(dirname "$SELF_PATH")")")"
    if [ "$bin_parent" = "bin" ] && [ "$app_parent" = "app" ]; then
      ROOT="$(dirname "$(dirname "$(dirname "$SELF_PATH")")")"
    elif [ "$OS_KIND" = "macos" ]; then
      ROOT="$HOME/Library/Application Support/MobileShop"
    else
      ROOT="${XDG_DATA_HOME:-$HOME/.local/share}/mobileshop"
    fi
  fi

  APP_DIR="$ROOT/app"
  VERSIONS_DIR="$ROOT/app/versions"
  CURRENT_LINK="$ROOT/app/current"
  BIN_DIR_APP="$ROOT/app/bin"
  LAUNCHER="$ROOT/app/bin/mobileshop"
  DATA_DIR="$ROOT/data"
  LOGS_DIR="$ROOT/data/logs"
  STATE_DIR="$ROOT/state"
  CACHE_DIR="$ROOT/cache"
  DOWNLOADS_DIR="$ROOT/cache/downloads"
  EXTRACT_DIR="$ROOT/cache/extract"
  INSTALLED_ENV="$ROOT/state/installed.env"
  INSTALL_LOG="$ROOT/state/install.log"
  LOCK_DIR="$ROOT/state/lock"
  PID_FILE="$ROOT/state/app.pid"
}

ensure_layout_dirs() {
  mkdir -p "$VERSIONS_DIR" "$BIN_DIR_APP" "$DATA_DIR" "$LOGS_DIR" "$STATE_DIR" "$DOWNLOADS_DIR" "$EXTRACT_DIR"
  chmod 700 "$DATA_DIR"
}

safe_rm_rf() {
  local target="${1:-}"
  [ -n "$target" ] || die 6 "Refusing to remove an empty path."
  [ -n "$ROOT" ] || die 6 "Refusing to remove without a resolved ROOT."
  [ "$target" != "/" ] || die 6 "Refusing to remove /."
  [ "$target" != "$HOME" ] || die 6 "Refusing to remove HOME."

  case "$target" in
    "$ROOT"|"$ROOT"/*)
      case "$target" in
        */../*|*/..)
          die 6 "Refusing to remove unsafe relative path: $target"
          ;;
      esac
      rm -rf -- "$target"
      ;;
    *)
      die 6 "Refusing to remove path outside ROOT ($ROOT): $target"
      ;;
  esac
}

safe_rm_data_only() {
  local target="${1:-}"
  [ "$target" = "$DATA_DIR" ] || die 6 "Refusing data removal on unexpected path: $target"
  safe_rm_rf "$target"
}

read_installed_env() {
  INST_TAG=""
  INST_PREVIOUS=""
  INST_RID=""
  INST_LAST_BACKUP=""
  INST_SERVICE_MODE=""

  [ -f "$INSTALLED_ENV" ] || return 0
  local line key val
  while IFS= read -r line || [ -n "$line" ]; do
    case "$line" in
      ''|'#'*) continue ;;
      *=*)
        key="${line%%=*}"
        val="${line#*=}"
        case "$key" in
          TAG) INST_TAG="$val" ;;
          PREVIOUS) INST_PREVIOUS="$val" ;;
          RID) INST_RID="$val" ;;
          LAST_BACKUP) INST_LAST_BACKUP="$val" ;;
          SERVICE_MODE) INST_SERVICE_MODE="$val" ;;
        esac
        ;;
    esac
  done < "$INSTALLED_ENV"
}

write_installed_env() {
  local tag="$1"
  local previous="$2"
  local rid="$3"
  local installed_at="$4"
  local last_backup="$5"
  local service_mode="$6"
  local tmp_env="$INSTALLED_ENV.tmp"

  mkdir -p "$STATE_DIR"
  {
    printf 'TAG=%s\n' "$tag"
    printf 'PREVIOUS=%s\n' "$previous"
    printf 'RID=%s\n' "$rid"
    printf 'INSTALLED_AT=%s\n' "$installed_at"
    printf 'LAST_BACKUP=%s\n' "$last_backup"
    printf 'SERVICE_MODE=%s\n' "$service_mode"
  } > "$tmp_env"
  mv -f "$tmp_env" "$INSTALLED_ENV"
}

validate_port() {
  case "$CLI_PORT" in
    ''|*[!0-9]*)
      die 2 "Invalid port '$CLI_PORT': must be an integer between 1 and 65535."
      ;;
  esac
  if [ "$CLI_PORT" -lt 1 ] || [ "$CLI_PORT" -gt 65535 ]; then
    die 2 "Invalid port '$CLI_PORT': must be between 1 and 65535."
  fi
}

validate_service_mode() {
  case "$CLI_SERVICE" in
    auto|none) ;;
    *)
      die 2 "Invalid service mode '$CLI_SERVICE': expected 'auto' or 'none'."
      ;;
  esac
}

validate_tag() {
  local raw="$1"
  [ -n "$raw" ] || die 2 "Release tag cannot be empty."
  case "$raw" in
    */*|*..*|*' '*|*$'\t'*|*$'\n'*)
      die 2 "Invalid release tag '$raw'."
      ;;
  esac
  case "$raw" in
    v*) printf '%s\n' "$raw" ;;
    *) printf 'v%s\n' "$raw" ;;
  esac
}

xml_escape() {
  local s="$1"
  s="${s//&/&amp;}"
  s="${s//</&lt;}"
  s="${s//>/&gt;}"
  printf '%s' "$s"
}

compute_sha256() {
  local file="$1"
  if command -v sha256sum >/dev/null 2>&1; then
    sha256sum "$file" | awk '{print $1}' | tr '[:upper:]' '[:lower:]'
  elif command -v shasum >/dev/null 2>&1; then
    shasum -a 256 "$file" | awk '{print $1}' | tr '[:upper:]' '[:lower:]'
  else
    die 3 "Neither sha256sum nor shasum is installed."
  fi
}

lookup_checksum() {
  local checksums_file="$1"
  local target_name="$2"
  awk -v f="$target_name" '
    {
      name = $2
      sub(/^\*/, "", name)
      if (name == f) {
        print tolower($1)
        exit
      }
    }
  ' "$checksums_file"
}

verify_sha256() {
  local file="$1"
  local expected="$2"
  local actual
  expected="$(printf '%s' "$expected" | tr '[:upper:]' '[:lower:]')"
  actual="$(compute_sha256 "$file")"
  if [ "$actual" != "$expected" ]; then
    return 1
  fi
  return 0
}

curl_download() {
  local url="$1"
  local dest="$2"
  local part="${dest}.part"

  case "$url" in
    file://*)
      rm -f "$part"
      if ! curl -fL --retry 5 --retry-delay 2 --retry-all-errors "$url" -o "$part"; then
        rm -f "$part"
        return 1
      fi
      ;;
    *)
      if ! curl -fL --retry 5 --retry-delay 2 --retry-all-errors -C - "$url" -o "$part"; then
        rm -f "$part"
        return 1
      fi
      ;;
  esac

  mv -f "$part" "$dest"
}

resolve_latest_tag() {
  local latest_url="${CLI_BASE_URL}/${CLI_REPO}/releases/latest"
  local effective
  if ! effective="$(curl -fsSL -o /dev/null -w '%{url_effective}' "$latest_url")"; then
    die 3 "Failed to query latest release from $latest_url."
  fi
  effective="${effective%/}"
  case "$effective" in
    */tag/*)
      local tag="${effective##*/tag/}"
      validate_tag "$tag"
      ;;
    *)
      die 3 "Could not determine release tag from redirect URL: $effective"
      ;;
  esac
}

df_free_bytes() {
  local dir="$1"
  while [ ! -d "$dir" ] && [ "$dir" != "/" ]; do
    dir="$(dirname "$dir")"
  done
  df -Pk "$dir" | awk 'NR==2 {printf "%.0f\n", $4 * 1024}'
}

file_size_bytes() {
  local file="$1"
  if [ -f "$file" ]; then
    wc -c < "$file" | tr -d '[:space:]'
  else
    printf '0\n'
  fi
}

port_is_listening() {
  local port="$1"
  (exec 3<>"/dev/tcp/127.0.0.1/${port}") >/dev/null 2>&1 || (exec 3<>"/dev/tcp/::1/${port}") >/dev/null 2>&1
}

systemd_unit_path() {
  printf '%s/systemd/user/mobileshop.service\n' "${XDG_CONFIG_HOME:-$HOME/.config}"
}

launchd_plist_path() {
  printf '%s/Library/LaunchAgents/com.migueljocode.mobileshop.plist\n' "$HOME"
}

has_systemd_user_bus() {
  command -v systemctl >/dev/null 2>&1 && systemctl --user show-environment >/dev/null 2>&1
}

has_launchd() {
  [ "$OS_KIND" = "macos" ] && command -v launchctl >/dev/null 2>&1
}

effective_service_mode() {
  if [ "$CLI_SERVICE" = "none" ]; then
    printf 'none\n'
    return 0
  fi
  if [ "$OS_KIND" = "linux" ]; then
    if has_systemd_user_bus; then
      printf 'systemd\n'
    else
      printf 'pid\n'
    fi
  elif [ "$OS_KIND" = "macos" ]; then
    if has_launchd; then
      printf 'launchd\n'
    else
      printf 'pid\n'
    fi
  else
    printf 'pid\n'
  fi
}

active_service_mode() {
  read_installed_env
  if [ "$CLI_SERVICE" = "none" ] || [ "$INST_SERVICE_MODE" = "none" ]; then
    printf 'pid\n'
    return 0
  fi
  if [ "$OS_KIND" = "linux" ]; then
    if [ -f "$(systemd_unit_path)" ] && has_systemd_user_bus; then
      printf 'systemd\n'
      return 0
    fi
    if [ -f "$PID_FILE" ]; then
      printf 'pid\n'
      return 0
    fi
    if has_systemd_user_bus; then
      printf 'systemd\n'
    else
      printf 'pid\n'
    fi
  elif [ "$OS_KIND" = "macos" ]; then
    if [ -f "$(launchd_plist_path)" ] && has_launchd; then
      printf 'launchd\n'
      return 0
    fi
    if [ -f "$PID_FILE" ]; then
      printf 'pid\n'
      return 0
    fi
    if has_launchd; then
      printf 'launchd\n'
    else
      printf 'pid\n'
    fi
  else
    printf 'pid\n'
  fi
}

is_pid_running() {
  [ -f "$PID_FILE" ] || return 1
  local pid
  pid="$(cat "$PID_FILE" 2>/dev/null || true)"
  case "$pid" in
    ''|*[!0-9]*) return 1 ;;
  esac
  kill -0 "$pid" 2>/dev/null
}

is_our_app_running() {
  local mode
  mode="$(active_service_mode)"
  case "$mode" in
    systemd)
      systemctl --user is-active --quiet mobileshop.service 2>/dev/null
      ;;
    launchd)
      local uid
      uid="$(id -u)"
      launchctl print "gui/${uid}/com.migueljocode.mobileshop" >/dev/null 2>&1
      ;;
    pid)
      is_pid_running
      ;;
  esac
}

wait_for_port_free() {
  local port="$1"
  local i=0
  while [ "$i" -lt 40 ]; do
    if ! port_is_listening "$port"; then
      return 0
    fi
    sleep 0.5
    i=$((i + 1))
  done
  return 1
}

write_systemd_unit() {
  local unit_path
  unit_path="$(systemd_unit_path)"
  mkdir -p "$(dirname "$unit_path")"
  cat > "$unit_path" <<SYSTEMD_EOF
[Unit]
Description=MobileShop web application
After=network.target

[Service]
Type=simple
ExecStart="${LAUNCHER}" run
Restart=on-failure
RestartSec=3
TimeoutStopSec=30

[Install]
WantedBy=default.target
SYSTEMD_EOF
}

write_launchd_plist() {
  local plist_path log_dir
  plist_path="$(launchd_plist_path)"
  log_dir="$HOME/Library/Logs/MobileShop"
  mkdir -p "$(dirname "$plist_path")" "$log_dir"
  cat > "$plist_path" <<LAUNCHD_EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key>
  <string>com.migueljocode.mobileshop</string>
  <key>ProgramArguments</key>
  <array>
    <string>$(xml_escape "$LAUNCHER")</string>
    <string>run</string>
  </array>
  <key>RunAtLoad</key>
  <true/>
  <key>KeepAlive</key>
  <dict>
    <key>SuccessfulExit</key>
    <false/>
  </dict>
  <key>ExitTimeOut</key>
  <integer>30</integer>
  <key>StandardOutPath</key>
  <string>$(xml_escape "$log_dir/stdout.log")</string>
  <key>StandardErrorPath</key>
  <string>$(xml_escape "$log_dir/stderr.log")</string>
</dict>
</plist>
LAUNCHD_EOF
}

start_pid_mode() {
  if is_pid_running; then
    return 0
  fi
  mkdir -p "$LOGS_DIR" "$STATE_DIR"
  local pid
  MOBILESHOP_ROOT="$ROOT" MOBILESHOP_PORT="$CLI_PORT" MOBILESHOP_SERVICE="none" \
    nohup "$LAUNCHER" run >> "$LOGS_DIR/stdout.log" 2>&1 </dev/null &
  pid="$!"
  printf '%s\n' "$pid" > "$PID_FILE"
}

stop_pid_mode() {
  if [ ! -f "$PID_FILE" ]; then
    return 0
  fi
  local pid i
  pid="$(cat "$PID_FILE" 2>/dev/null || true)"
  case "$pid" in
    ''|*[!0-9]*)
      rm -f "$PID_FILE"
      return 0
      ;;
  esac
  if kill -0 "$pid" 2>/dev/null; then
    kill "$pid" 2>/dev/null || true
    i=0
    while [ "$i" -lt 40 ]; do
      if ! kill -0 "$pid" 2>/dev/null; then
        break
      fi
      sleep 0.5
      i=$((i + 1))
    done
    if kill -0 "$pid" 2>/dev/null; then
      kill -9 "$pid" 2>/dev/null || true
    fi
  fi
  rm -f "$PID_FILE"
}

start_service() {
  local requested
  if [ "$CLI_SERVICE" = "none" ]; then
    start_pid_mode
    return 0
  fi
  requested="$(effective_service_mode)"
  case "$requested" in
    systemd)
      write_systemd_unit
      systemctl --user daemon-reload
      systemctl --user enable --now mobileshop.service
      ;;
    launchd)
      local uid plist_path
      uid="$(id -u)"
      plist_path="$(launchd_plist_path)"
      write_launchd_plist
      launchctl bootout "gui/${uid}/com.migueljocode.mobileshop" >/dev/null 2>&1 || true
      launchctl bootstrap "gui/${uid}" "$plist_path"
      ;;
    pid)
      if [ "$OS_KIND" = "linux" ] && [ "$CLI_SERVICE" = "auto" ]; then
        warn "systemd user bus is not available; falling back to pid mode."
      fi
      start_pid_mode
      ;;
  esac
}

stop_service() {
  local mode
  mode="$(active_service_mode)"
  case "$mode" in
    systemd)
      if [ -f "$(systemd_unit_path)" ] && has_systemd_user_bus; then
        systemctl --user stop mobileshop.service >/dev/null 2>&1 || true
      fi
      stop_pid_mode
      ;;
    launchd)
      if has_launchd; then
        local uid
        uid="$(id -u)"
        launchctl bootout "gui/${uid}/com.migueljocode.mobileshop" >/dev/null 2>&1 || true
      fi
      stop_pid_mode
      ;;
    pid)
      stop_pid_mode
      ;;
  esac
  wait_for_port_free "$CLI_PORT" || true
}

restart_service() {
  local mode
  mode="$(active_service_mode)"
  case "$mode" in
    systemd)
      if [ -f "$(systemd_unit_path)" ]; then
        systemctl --user restart mobileshop.service
      else
        start_service
      fi
      ;;
    launchd)
      local uid plist_path
      uid="$(id -u)"
      plist_path="$(launchd_plist_path)"
      if launchctl print "gui/${uid}/com.migueljocode.mobileshop" >/dev/null 2>&1; then
        launchctl kickstart -k "gui/${uid}/com.migueljocode.mobileshop"
      elif [ -f "$plist_path" ]; then
        launchctl bootstrap "gui/${uid}" "$plist_path"
      else
        start_service
      fi
      ;;
    pid)
      stop_pid_mode
      wait_for_port_free "$CLI_PORT" || true
      start_pid_mode
      ;;
  esac
}

cleanup_lock() {
  if [ "${LOCK_ACQUIRED:-0}" -eq 1 ] && [ -n "${LOCK_DIR:-}" ] && [ -d "$LOCK_DIR" ]; then
    rmdir "$LOCK_DIR" 2>/dev/null || true
  fi
}

acquire_lock() {
  mkdir -p "$STATE_DIR"
  if ! mkdir "$LOCK_DIR" 2>/dev/null; then
    die 6 "Another install or update is already in progress ($LOCK_DIR)."
  fi
  LOCK_ACQUIRED=1
  trap cleanup_lock EXIT INT TERM
}

install_launcher_and_symlink() {
  local tag_hint="${1:-}"
  ensure_layout_dirs
  local self_is_file=0
  case "$0" in
    bash|-bash|sh|-sh) self_is_file=0 ;;
    *)
      if [ -f "$SELF_PATH" ]; then
        self_is_file=1
      fi
      ;;
  esac
  if [ "$SELF_PATH" = "$LAUNCHER" ]; then
    chmod +x "$LAUNCHER"
  elif [ "$self_is_file" -eq 1 ]; then
    cp "$SELF_PATH" "$LAUNCHER.tmp"
    chmod +x "$LAUNCHER.tmp"
    mv -f "$LAUNCHER.tmp" "$LAUNCHER"
  elif [ -n "$tag_hint" ]; then
    local dl_url="${CLI_BASE_URL}/${CLI_REPO}/releases/download/${tag_hint}/install.sh"
    if curl_download "$dl_url" "$LAUNCHER.tmp"; then
      local sums_file="$DOWNLOADS_DIR/checksums-${tag_hint}.txt"
      if [ -f "$sums_file" ]; then
        local exp_sum
        exp_sum="$(lookup_checksum "$sums_file" "install.sh")"
        if [ -n "$exp_sum" ] && ! verify_sha256 "$LAUNCHER.tmp" "$exp_sum"; then
          rm -f "$LAUNCHER.tmp"
          die 3 "SHA-256 checksum mismatch for downloaded install.sh."
        fi
      fi
      chmod +x "$LAUNCHER.tmp"
      mv -f "$LAUNCHER.tmp" "$LAUNCHER"
    else
      die 3 "Running from stdin requires downloading install.sh from $dl_url."
    fi
  else
    die 1 "Cannot locate running script file ($SELF_PATH) to install launcher."
  fi
  mkdir -p "$CLI_BIN_DIR"
  ln -sfn "$LAUNCHER" "$CLI_BIN_DIR/mobileshop"
}

print_path_hint_if_needed() {
  case ":$PATH:" in
    *":${CLI_BIN_DIR}:"*) ;;
    *)
      printf 'Note: add %s to your PATH to run `mobileshop` directly.\n' "$CLI_BIN_DIR"
      ;;
  esac
}

poll_health() {
  local port="$1"
  local url="http://127.0.0.1:${port}/Account/Login"
  local i=0
  local code
  while [ "$i" -lt 60 ]; do
    code="$(curl -s -o /dev/null -w '%{http_code}' --max-time 2 "$url" 2>/dev/null || true)"
    if [ "$code" = "200" ]; then
      return 0
    fi
    sleep 0.5
    i=$((i + 1))
  done
  return 1
}

list_backups_sorted() {
  if [ -d "$DATA_DIR" ]; then
    find "$DATA_DIR" -maxdepth 1 -type f -name 'MobileShop.db.*.bak' | sort
  fi
}

find_new_backup() {
  local before_file="$1"
  local after_file="$2"
  comm -13 "$before_file" "$after_file" | tail -n 1
}

restore_backup_file() {
  local backup_file="$1"
  [ -n "$backup_file" ] || return 0
  [ -f "$backup_file" ] || die 5 "Expected backup file not found for restore: $backup_file"
  rm -f "$DATA_DIR/MobileShop.db-wal" "$DATA_DIR/MobileShop.db-shm"
  cp "$backup_file" "$DATA_DIR/MobileShop.db.restore.tmp"
  chmod 600 "$DATA_DIR/MobileShop.db.restore.tmp"
  mv -f "$DATA_DIR/MobileShop.db.restore.tmp" "$DATA_DIR/MobileShop.db"
  log_msg "INFO" "Restored database backup: $backup_file"
}

run_with_app_env() {
  local content_root="$1"
  local exe_path="$2"
  shift 2

  mkdir -p "$DATA_DIR" "$LOGS_DIR" "$EXTRACT_DIR"
  chmod 700 "$DATA_DIR"
  (
    cd "$DATA_DIR" || exit 1
    export ASPNETCORE_ENVIRONMENT="Production"
    export DOTNET_ENVIRONMENT="Production"
    # shellcheck disable=SC2031
    export ASPNETCORE_CONTENTROOT="$content_root"
    # shellcheck disable=SC2031
    export MOBILESHOP_DATA_DIRECTORY="$DATA_DIR"
    # shellcheck disable=SC2031
    export ASPNETCORE_URLS="http://localhost:${CLI_PORT}"
    # shellcheck disable=SC2031
    export DOTNET_BUNDLE_EXTRACT_BASE_DIR="$EXTRACT_DIR"
    "$exe_path" "$@"
  )
}

cmd_run() {
  [ -L "$CURRENT_LINK" ] || [ -d "$CURRENT_LINK" ] || die 1 "No installed version found at $CURRENT_LINK."
  local exe="$CURRENT_LINK/MobileShop.Web"
  [ -x "$exe" ] || die 1 "Executable not found or not executable: $exe"

  mkdir -p "$DATA_DIR" "$LOGS_DIR" "$EXTRACT_DIR"
  chmod 700 "$DATA_DIR"
  cd "$DATA_DIR" || die 1 "Failed to enter data directory: $DATA_DIR"

  export ASPNETCORE_ENVIRONMENT="Production"
  export DOTNET_ENVIRONMENT="Production"
  # shellcheck disable=SC2031
  export ASPNETCORE_CONTENTROOT="$CURRENT_LINK"
  # shellcheck disable=SC2031
  export MOBILESHOP_DATA_DIRECTORY="$DATA_DIR"
  # shellcheck disable=SC2031
  export ASPNETCORE_URLS="http://localhost:${CLI_PORT}"
  # shellcheck disable=SC2031
  export DOTNET_BUNDLE_EXTRACT_BASE_DIR="$EXTRACT_DIR"

  exec "$exe" "$@"
}

infer_tag_from_archive() {
  local archive_path="$1"
  local base inner trimmed
  base="$(basename "$archive_path")"
  case "$base" in
    MobileShop-v*-"${RID}".tar.gz)
      trimmed="${base#MobileShop-}"
      printf '%s\n' "${trimmed%-"${RID}".tar.gz}"
      return 0
      ;;
  esac
  inner="$(tar -tzf "$archive_path" 2>/dev/null | head -n 1 | cut -d/ -f1 || true)"
  case "$inner" in
    MobileShop-v*-"${RID}")
      trimmed="${inner#MobileShop-}"
      printf '%s\n' "${trimmed%-"${RID}"}"
      return 0
      ;;
  esac
  printf 'v0.0.0-local\n'
}

cmd_install() {
  local req_version="" archive_file="" sha256_override="" force=0

  while [ "$#" -gt 0 ]; do
    case "$1" in
      --version)
        shift
        [ "$#" -gt 0 ] || die 2 "--version requires an argument."
        req_version="$1"
        ;;
      --archive)
        shift
        [ "$#" -gt 0 ] || die 2 "--archive requires an argument."
        archive_file="$1"
        ;;
      --sha256)
        shift
        [ "$#" -gt 0 ] || die 2 "--sha256 requires an argument."
        sha256_override="$1"
        ;;
      --force)
        force=1
        ;;
      --no-service)
        CLI_SERVICE="none"
        ;;
      -h|--help)
        print_usage
        return 0
        ;;
      *)
        die 2 "Unknown option for install: $1"
        ;;
    esac
    shift
  done

  validate_port
  validate_service_mode
  detect_platform
  ensure_layout_dirs
  acquire_lock
  read_installed_env

  local target_tag=""
  if [ -n "$req_version" ]; then
    target_tag="$(validate_tag "$req_version")"
  elif [ -n "$archive_file" ]; then
    [ -f "$archive_file" ] || die 3 "Archive not found: $archive_file"
    target_tag="$(infer_tag_from_archive "$archive_file")"
  else
    target_tag="$(resolve_latest_tag)"
  fi

  if [ -n "$INST_TAG" ] && [ "$INST_TAG" = "$target_tag" ] && [ "$force" -eq 0 ]; then
    info "Already up to date (${target_tag})."
    return 0
  fi

  # Pre-flight checks before modifying anything
  [ -w "$DATA_DIR" ] || die 6 "Data directory is not writable: $DATA_DIR"
  if ! is_our_app_running; then
    if port_is_listening "$CLI_PORT"; then
      die 6 "Port ${CLI_PORT} is already in use by another process."
    fi
  fi

  local archive_name="MobileShop-${target_tag}-${RID}.tar.gz"
  local local_archive=""

  if [ -n "$archive_file" ]; then
    [ -f "$archive_file" ] || die 3 "Archive not found: $archive_file"
    local_archive="$archive_file"
    local archive_base
    archive_base="$(basename "$archive_file")"
    if [ -n "$sha256_override" ]; then
      if ! verify_sha256 "$local_archive" "$sha256_override"; then
        die 3 "SHA-256 checksum mismatch for $archive_file."
      fi
    else
      local sidecar_checksums
      sidecar_checksums="$(dirname "$archive_file")/checksums.txt"
      if [ -f "$sidecar_checksums" ]; then
        local expected_sum
        expected_sum="$(lookup_checksum "$sidecar_checksums" "$archive_base")"
        if [ -n "$expected_sum" ]; then
          if ! verify_sha256 "$local_archive" "$expected_sum"; then
            die 3 "SHA-256 checksum mismatch for $archive_file against $sidecar_checksums."
          fi
        else
          warn "unverified archive ($archive_base not listed in $sidecar_checksums)."
        fi
      else
        warn "unverified archive (no checksums.txt or --sha256 provided)."
      fi
    fi
  else
    local base_dl="${CLI_BASE_URL}/${CLI_REPO}/releases/download/${target_tag}"
    local checksums_dest="$DOWNLOADS_DIR/checksums-${target_tag}.txt"
    local archive_dest="$DOWNLOADS_DIR/${archive_name}"
    if ! curl_download "${base_dl}/checksums.txt" "$checksums_dest"; then
      die 3 "Failed to download checksums.txt for ${target_tag}."
    fi
    local expected_sum
    expected_sum="$(lookup_checksum "$checksums_dest" "$archive_name")"
    [ -n "$expected_sum" ] || die 3 "No checksum entry for ${archive_name} in checksums.txt."

    if ! curl_download "${base_dl}/${archive_name}" "$archive_dest"; then
      die 3 "Failed to download ${archive_name}."
    fi
    if ! verify_sha256 "$archive_dest" "$expected_sum"; then
      rm -f "$archive_dest"
      die 3 "SHA-256 checksum mismatch for ${archive_name}; downloaded file deleted."
    fi
    local_archive="$archive_dest"
  fi

  # Free-space check: >= 3x archive + DB
  local arc_bytes db_bytes req_bytes avail_bytes
  arc_bytes="$(file_size_bytes "$local_archive")"
  db_bytes="$(file_size_bytes "$DATA_DIR/MobileShop.db")"
  req_bytes=$((arc_bytes * 3 + db_bytes))
  avail_bytes="$(df_free_bytes "$ROOT" 2>/dev/null || printf '0')"
  case "$avail_bytes" in
    ''|*[!0-9]*) avail_bytes=0 ;;
  esac
  if [ "$avail_bytes" -lt "$req_bytes" ]; then
    die 6 "Insufficient disk space under $ROOT (need at least ${req_bytes} bytes, have ${avail_bytes} bytes)."
  fi

  # Extract and validate in stage dir
  local stage_dir="$VERSIONS_DIR/.stage-${target_tag}"
  safe_rm_rf "$stage_dir"
  mkdir -p "$stage_dir"

  if ! tar -xzf "$local_archive" -C "$stage_dir"; then
    safe_rm_rf "$stage_dir"
    die 4 "Failed to extract archive: $local_archive"
  fi

  local top_dir="" top_count=0 entry
  for entry in "$stage_dir"/*; do
    [ -e "$entry" ] || continue
    if [ -d "$entry" ]; then
      top_count=$((top_count + 1))
      top_dir="$entry"
    else
      safe_rm_rf "$stage_dir"
      die 4 "Archive must contain a single top-level directory (found file: $(basename "$entry"))."
    fi
  done

  if [ "$top_count" -ne 1 ] || [ -z "$top_dir" ]; then
    safe_rm_rf "$stage_dir"
    die 4 "Archive must contain exactly one top-level directory."
  fi

  if [ ! -f "$top_dir/MobileShop.Web" ] || [ ! -x "$top_dir/MobileShop.Web" ]; then
    safe_rm_rf "$stage_dir"
    die 4 "Extracted archive does not contain an executable MobileShop.Web."
  fi

  if [ "$OS_KIND" = "macos" ]; then
    xattr -dr com.apple.quarantine "$top_dir" >/dev/null 2>&1 || true
    if ! codesign --verify "$top_dir/MobileShop.Web" >/dev/null 2>&1; then
      safe_rm_rf "$stage_dir"
      die 4 "Code signature verification failed for MobileShop.Web."
    fi
  fi

  # Stop running app before placing the version & migrating
  local was_running=0
  if is_our_app_running; then
    was_running=1
    info "Stopping running MobileShop instance..."
    stop_service
  fi

  local version_dir="$VERSIONS_DIR/$target_tag"
  local same_tag_backup="$VERSIONS_DIR/.prev-${target_tag}"
  safe_rm_rf "$same_tag_backup"
  if [ -d "$version_dir" ]; then
    mv "$version_dir" "$same_tag_backup"
  fi
  mv "$top_dir" "$version_dir"
  safe_rm_rf "$stage_dir"

  # Snapshot backup list before migrating
  local before_baks after_baks
  before_baks="$STATE_DIR/baks-before.$$"
  after_baks="$STATE_DIR/baks-after.$$"
  list_backups_sorted > "$before_baks"

  info "Running database migrations for ${target_tag}..."
  local migrate_rc=0
  if run_with_app_env "$version_dir" "$version_dir/MobileShop.Web" --migrate-database; then
    migrate_rc=0
  else
    migrate_rc=$?
  fi

  list_backups_sorted > "$after_baks"
  local new_bak
  new_bak="$(find_new_backup "$before_baks" "$after_baks" || true)"
  rm -f "$before_baks" "$after_baks"

  local prev_tag="$INST_TAG"
  if [ "$prev_tag" = "$target_tag" ]; then
    prev_tag="$INST_PREVIOUS"
  fi

  if [ "$migrate_rc" -ne 0 ]; then
    warn "Database migration failed with exit code ${migrate_rc}; rolling back."
    if [ -n "$new_bak" ]; then
      restore_backup_file "$new_bak"
    fi
    safe_rm_rf "$version_dir"
    if [ -d "$same_tag_backup" ]; then
      mv "$same_tag_backup" "$version_dir"
    fi
    if [ "$was_running" -eq 1 ] && [ -e "$CURRENT_LINK/MobileShop.Web" ]; then
      start_service || true
      poll_health "$CLI_PORT" || true
    fi
    die 5 "Install/update aborted because --migrate-database failed (exit ${migrate_rc})."
  fi

  # Switch current symlink and install launcher
  install_launcher_and_symlink "${target_tag:-}"
  ln -sfn "versions/${target_tag}" "$CURRENT_LINK"

  # Start and health-check
  info "Starting MobileShop (${target_tag}) on http://localhost:${CLI_PORT}..."
  if ! start_service || ! poll_health "$CLI_PORT"; then
    warn "Health check failed on http://localhost:${CLI_PORT}/Account/Login; rolling back."
    stop_service || true
    if [ -n "$new_bak" ]; then
      restore_backup_file "$new_bak"
    fi
    if [ -n "$INST_TAG" ] && [ -d "$VERSIONS_DIR/$INST_TAG" ] && [ "$INST_TAG" != "$target_tag" ]; then
      ln -sfn "versions/${INST_TAG}" "$CURRENT_LINK"
      start_service || true
      poll_health "$CLI_PORT" || true
    elif [ -d "$same_tag_backup" ]; then
      safe_rm_rf "$version_dir"
      mv "$same_tag_backup" "$version_dir"
      ln -sfn "versions/${target_tag}" "$CURRENT_LINK"
      start_service || true
      poll_health "$CLI_PORT" || true
    else
      rm -f "$CURRENT_LINK"
    fi
    die 5 "Health check failed after starting ${target_tag}; rolled back."
  fi

  safe_rm_rf "$same_tag_backup"
  safe_rm_rf "$EXTRACT_DIR"
  mkdir -p "$EXTRACT_DIR"

  local now_utc svc_saved="auto"
  now_utc="$(date -u '+%Y-%m-%dT%H:%M:%SZ' 2>/dev/null || printf 'unknown')"
  if [ "$CLI_SERVICE" = "none" ]; then
    svc_saved="none"
  fi
  write_installed_env "$target_tag" "$prev_tag" "$RID" "$now_utc" "$new_bak" "$svc_saved"

  info "Installed MobileShop ${target_tag} (${RID})."
  info "Open: http://localhost:${CLI_PORT}"
  print_path_hint_if_needed
}

cmd_update() {
  local use_archive=0 req_version="" arg
  for arg in "$@"; do
    case "$arg" in
      --archive) use_archive=1 ;;
      --no-service) CLI_SERVICE="none" ;;
      -h|--help)
        print_usage
        return 0
        ;;
    esac
  done

  if [ "$use_archive" -eq 1 ]; then
    cmd_install "$@"
    return $?
  fi

  local idx=0
  local args_arr=()
  while [ "$#" -gt 0 ]; do
    args_arr[idx]="$1"
    if [ "$1" = "--version" ] && [ "$#" -gt 1 ]; then
      req_version="$2"
    fi
    idx=$((idx + 1))
    shift
  done

  detect_platform
  ensure_layout_dirs

  local dl_tag="$req_version"
  if [ -n "$dl_tag" ]; then
    dl_tag="$(validate_tag "$dl_tag")"
  fi

  local script_url checksums_url
  if [ -n "$dl_tag" ]; then
    script_url="${CLI_BASE_URL}/${CLI_REPO}/releases/download/${dl_tag}/install.sh"
    checksums_url="${CLI_BASE_URL}/${CLI_REPO}/releases/download/${dl_tag}/checksums.txt"
  else
    script_url="${CLI_BASE_URL}/${CLI_REPO}/releases/latest/download/install.sh"
    checksums_url="${CLI_BASE_URL}/${CLI_REPO}/releases/latest/download/checksums.txt"
  fi

  local tmp_script="$DOWNLOADS_DIR/install.sh.next"
  local tmp_sums="$DOWNLOADS_DIR/checksums-update.txt"
  if ! curl_download "$checksums_url" "$tmp_sums"; then
    die 3 "Failed to download checksums.txt from $checksums_url."
  fi
  local expected_sum
  expected_sum="$(lookup_checksum "$tmp_sums" "install.sh")"
  [ -n "$expected_sum" ] || die 3 "No checksum entry for install.sh in checksums.txt."

  if ! curl_download "$script_url" "$tmp_script"; then
    die 3 "Failed to download latest installer from $script_url."
  fi
  if ! verify_sha256 "$tmp_script" "$expected_sum"; then
    rm -f "$tmp_script"
    die 3 "SHA-256 checksum mismatch for downloaded install.sh."
  fi

  chmod +x "$tmp_script"
  export MOBILESHOP_ROOT="$ROOT"
  export MOBILESHOP_BIN_DIR="$CLI_BIN_DIR"
  export MOBILESHOP_PORT="$CLI_PORT"
  export MOBILESHOP_SERVICE="$CLI_SERVICE"
  export MOBILESHOP_REPO="$CLI_REPO"
  export MOBILESHOP_BASE_URL="$CLI_BASE_URL"
  if [ "${#args_arr[@]}" -gt 0 ]; then
    exec bash "$tmp_script" install "${args_arr[@]}"
  else
    exec bash "$tmp_script" install
  fi
}

cmd_rollback() {
  local yes=0
  while [ "$#" -gt 0 ]; do
    case "$1" in
      --yes|-y) yes=1 ;;
      --no-service) CLI_SERVICE="none" ;;
      -h|--help)
        print_usage
        return 0
        ;;
      *)
        die 2 "Unknown option for rollback: $1"
        ;;
    esac
    shift
  done

  detect_platform
  ensure_layout_dirs
  acquire_lock
  read_installed_env

  [ -n "$INST_PREVIOUS" ] || die 6 "No previous version recorded in $INSTALLED_ENV."
  [ -d "$VERSIONS_DIR/$INST_PREVIOUS" ] || die 6 "Previous version directory is missing: $VERSIONS_DIR/$INST_PREVIOUS"

  if [ -n "$INST_LAST_BACKUP" ]; then
    printf 'Warning: restoring backup %s will discard any data entered since %s was installed.\n' "$INST_LAST_BACKUP" "$INST_TAG"
    if [ "$yes" -ne 1 ]; then
      printf 'Type "yes" to confirm rollback and database restore: '
      local reply=""
      read -r reply || true
      if [ "$reply" != "yes" ]; then
        die 6 "Rollback cancelled."
      fi
    fi
  fi

  stop_service || true

  if [ -n "$INST_LAST_BACKUP" ]; then
    restore_backup_file "$INST_LAST_BACKUP"
  fi

  ln -sfn "versions/${INST_PREVIOUS}" "$CURRENT_LINK"
  start_service || die 5 "Failed to start rolled-back version $INST_PREVIOUS."
  if ! poll_health "$CLI_PORT"; then
    die 5 "Rolled-back version $INST_PREVIOUS did not become healthy on port $CLI_PORT."
  fi

  local now_utc
  now_utc="$(date -u '+%Y-%m-%dT%H:%M:%SZ' 2>/dev/null || printf 'unknown')"
  write_installed_env "$INST_PREVIOUS" "$INST_TAG" "${INST_RID:-$RID}" "$now_utc" "" "${INST_SERVICE_MODE:-$CLI_SERVICE}"
  info "Rolled back from ${INST_TAG} to ${INST_PREVIOUS}."
}

cmd_prune() {
  local keep=2 yes=0
  while [ "$#" -gt 0 ]; do
    case "$1" in
      --keep)
        shift
        [ "$#" -gt 0 ] || die 2 "--keep requires an argument."
        keep="$1"
        ;;
      --yes|-y) yes=1 ;;
      -h|--help)
        print_usage
        return 0
        ;;
      *)
        die 2 "Unknown option for prune: $1"
        ;;
    esac
    shift
  done

  case "$keep" in
    ''|*[!0-9]*) die 2 "--keep must be a positive integer." ;;
  esac
  [ "$keep" -ge 1 ] || die 2 "--keep must be at least 1."

  detect_os_hint
  ensure_layout_dirs

  local current_name=""
  if [ -L "$CURRENT_LINK" ]; then
    current_name="$(basename "$(readlink "$CURRENT_LINK")")"
  fi

  local sorted_others=() entry base
  # shellcheck disable=SC2012
  while IFS= read -r base; do
    [ -n "$base" ] || continue
    case "$base" in
      .stage-*|.prev-*) continue ;;
    esac
    [ "$base" != "$current_name" ] || continue
    entry="$VERSIONS_DIR/$base"
    [ -d "$entry" ] || continue
    sorted_others[${#sorted_others[@]}]="$base"
  done < <(ls -1t "$VERSIONS_DIR" 2>/dev/null || true)

  local keep_others=$((keep - 1))
  local to_remove=() idx=0
  if [ "${#sorted_others[@]}" -gt 0 ]; then
    for base in "${sorted_others[@]}"; do
      if [ "$idx" -ge "$keep_others" ]; then
        to_remove[${#to_remove[@]}]="$VERSIONS_DIR/$base"
      fi
      idx=$((idx + 1))
    done
  fi

  if [ "${#to_remove[@]}" -gt 0 ]; then
    printf 'Removable version directories:\n'
    for entry in "${to_remove[@]}"; do
      printf '  %s\n' "$entry"
    done
    if [ "$yes" -ne 1 ]; then
      printf 'Proceed with removal? [y/N] '
      local reply=""
      read -r reply || true
      case "$reply" in
        y|Y|yes|YES) ;;
        *)
          info "Prune cancelled."
          return 0
          ;;
      esac
    fi
    for entry in "${to_remove[@]}"; do
      safe_rm_rf "$entry"
    done
  else
    info "No old version directories to prune."
  fi

  safe_rm_rf "$DOWNLOADS_DIR"
  safe_rm_rf "$EXTRACT_DIR"
  mkdir -p "$DOWNLOADS_DIR" "$EXTRACT_DIR"
  info "Prune complete."
}

cmd_adopt() {
  [ "$#" -eq 1 ] || die 2 "Usage: mobileshop adopt <old-data-dir>"
  local src_dir="$1"
  [ -d "$src_dir" ] || die 2 "Source directory not found: $src_dir"

  detect_os_hint
  ensure_layout_dirs

  local src_db="$src_dir/MobileShop.db"
  local src_wal="$src_dir/MobileShop.db-wal"
  local dst_db="$DATA_DIR/MobileShop.db"

  [ ! -e "$dst_db" ] || die 6 "Refusing adopt: target database already exists at $dst_db."
  [ -f "$src_db" ] || die 6 "Refusing adopt: source database missing at $src_db."
  if [ -s "$src_wal" ]; then
    die 6 "Refusing adopt: non-empty WAL found at $src_wal. Stop the old MobileShop app cleanly so it checkpoints first."
  fi
  if is_our_app_running; then
    die 6 "Refusing adopt: MobileShop is currently running. Stop it first."
  fi

  local tmp_db="$DATA_DIR/MobileShop.db.adopt.tmp"
  cp "$src_db" "$tmp_db"
  if ! cmp -s "$src_db" "$tmp_db"; then
    rm -f "$tmp_db"
    die 6 "Verification failed after copying database from $src_db."
  fi
  chmod 600 "$tmp_db"
  mv -f "$tmp_db" "$dst_db"

  info "Adopted database from $src_db to $dst_db (original left untouched)."
  info "Next step: run 'mobileshop update' (or 'mobileshop install'); the first migration creates a verified backup."
}

cmd_backup() {
  detect_os_hint
  ensure_layout_dirs
  [ -x "$CURRENT_LINK/MobileShop.Web" ] || die 1 "No installed executable found at $CURRENT_LINK/MobileShop.Web."
  run_with_app_env "$CURRENT_LINK" "$CURRENT_LINK/MobileShop.Web" --backup-database
}

cmd_start() {
  validate_port
  validate_service_mode
  detect_platform
  ensure_layout_dirs
  [ -x "$CURRENT_LINK/MobileShop.Web" ] || die 1 "MobileShop is not installed at $CURRENT_LINK."
  install_launcher_and_symlink ""
  start_service
  info "Started MobileShop (http://localhost:${CLI_PORT})."
}

cmd_stop() {
  validate_port
  detect_os_hint
  ensure_layout_dirs
  stop_service
  info "Stopped MobileShop."
}

cmd_restart() {
  validate_port
  validate_service_mode
  detect_platform
  ensure_layout_dirs
  [ -x "$CURRENT_LINK/MobileShop.Web" ] || die 1 "MobileShop is not installed at $CURRENT_LINK."
  install_launcher_and_symlink ""
  restart_service
  info "Restarted MobileShop (http://localhost:${CLI_PORT})."
}

cmd_status() {
  validate_port
  detect_os_hint
  init_paths "$0"
  read_installed_env
  local running="stopped"
  if is_our_app_running; then
    running="running"
  fi
  printf 'Tag: %s\n' "${INST_TAG:-none}"
  printf 'Status: %s\n' "$running"
  printf 'URL: http://localhost:%s\n' "$CLI_PORT"
  printf 'Service mode: %s\n' "$(active_service_mode)"
}

cmd_logs() {
  detect_os_hint
  init_paths "$0"
  local newest=""
  newest="$(find "$LOGS_DIR" -maxdepth 1 -type f -name 'app-*.log' 2>/dev/null | sort | tail -n 1 || true)"
  [ -n "$newest" ] || die 1 "No log files found in $LOGS_DIR."
  tail -n 100 -f "$newest"
}

cmd_version() {
  detect_os_hint
  init_paths "$0"
  read_installed_env
  printf 'Installer: %s\n' "$(installer_version_label)"
  printf 'Installed: %s\n' "${INST_TAG:-none}"
}

cmd_doctor() {
  local failures=0
  local check_ok=1
  validate_port
  detect_os_hint
  read_installed_env

  if (detect_platform) >/dev/null 2>&1; then
    detect_platform
    printf 'PASS  platform: %s (%s/%s)\n' "$RID" "$SYS_NAME" "$ARCH_NAME"
  else
    printf 'FAIL  platform: unsupported (%s/%s)\n' "${MOBILESHOP_UNAME_S:-$(uname -s)}" "${MOBILESHOP_UNAME_M:-$(uname -m)}"
    failures=$((failures + 1))
    check_ok=0
  fi

  if [ -n "$INST_TAG" ]; then
    printf 'PASS  installed tag: %s\n' "$INST_TAG"
  else
    printf 'FAIL  installed tag: none (not installed)\n'
    failures=$((failures + 1))
  fi

  if [ -L "$CURRENT_LINK" ] && [ -x "$CURRENT_LINK/MobileShop.Web" ]; then
    printf 'PASS  current target: %s\n' "$(readlink "$CURRENT_LINK")"
  else
    printf 'FAIL  current target: missing or non-executable (%s)\n' "$CURRENT_LINK"
    failures=$((failures + 1))
  fi

  if is_our_app_running; then
    printf 'PASS  service: running (%s)\n' "$(active_service_mode)"
  else
    printf 'FAIL  service: stopped (%s)\n' "$(active_service_mode)"
    failures=$((failures + 1))
  fi

  if port_is_listening "$CLI_PORT"; then
    printf 'PASS  port: %s is listening\n' "$CLI_PORT"
  else
    printf 'FAIL  port: %s is not listening\n' "$CLI_PORT"
    failures=$((failures + 1))
  fi

  local free_b
  free_b="$(df_free_bytes "$ROOT" 2>/dev/null || printf '0')"
  case "$free_b" in
    ''|*[!0-9]*) free_b=0 ;;
  esac
  if [ "$free_b" -gt 0 ]; then
    printf 'PASS  free space: %s bytes\n' "$free_b"
  else
    printf 'FAIL  free space: unable to inspect %s\n' "$ROOT"
    failures=$((failures + 1))
  fi

  if [ -d "$DATA_DIR" ] && [ -w "$DATA_DIR" ]; then
    printf 'PASS  data directory: writable (%s)\n' "$DATA_DIR"
  else
    printf 'FAIL  data directory: not writable (%s)\n' "$DATA_DIR"
    failures=$((failures + 1))
  fi

  if [ -f "$DATA_DIR/MobileShop.db" ]; then
    printf 'PASS  database: %s (%s bytes)\n' "$DATA_DIR/MobileShop.db" "$(file_size_bytes "$DATA_DIR/MobileShop.db")"
  else
    printf 'FAIL  database: missing (%s)\n' "$DATA_DIR/MobileShop.db"
    failures=$((failures + 1))
  fi

  local newest_bak
  newest_bak="$(list_backups_sorted | tail -n 1 || true)"
  if [ -n "$newest_bak" ]; then
    printf 'PASS  newest backup: %s\n' "$newest_bak"
  else
    printf 'WARN  newest backup: none found\n'
  fi

  case ":$PATH:" in
    *":${CLI_BIN_DIR}:"*)
      printf 'PASS  PATH: contains %s\n' "$CLI_BIN_DIR"
      ;;
    *)
      printf 'WARN  PATH: %s is not on PATH\n' "$CLI_BIN_DIR"
      ;;
  esac

  if [ "$check_ok" -eq 1 ] && [ "$OS_KIND" = "linux" ]; then
    local glibc_ver
    glibc_ver="$(getconf GNU_LIBC_VERSION 2>/dev/null || printf 'unknown')"
    printf 'PASS  glibc: %s\n' "$glibc_ver"
    if command -v ldconfig >/dev/null 2>&1; then
      local ldcache
      ldcache="$(ldconfig -p 2>/dev/null || true)"
      if printf '%s' "$ldcache" | grep -q 'libstdc++\.so'; then
        printf 'PASS  libstdc++: present\n'
      else
        printf 'WARN  libstdc++: not detected in ldconfig cache\n'
      fi
      if printf '%s' "$ldcache" | grep -q 'libicu'; then
        printf 'PASS  ICU: present\n'
      else
        printf 'WARN  ICU: not detected; set DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 if runtime fails\n'
      fi
    fi
  elif [ "$check_ok" -eq 1 ] && [ "$OS_KIND" = "macos" ]; then
    if [ -x "$CURRENT_LINK/MobileShop.Web" ]; then
      if codesign --verify "$CURRENT_LINK/MobileShop.Web" >/dev/null 2>&1; then
        printf 'PASS  codesign: verified\n'
      else
        printf 'FAIL  codesign: verification failed for %s/MobileShop.Web\n' "$CURRENT_LINK"
        failures=$((failures + 1))
      fi
    fi
  fi

  if [ "$failures" -gt 0 ]; then
    return 1
  fi
  return 0
}

cmd_uninstall() {
  local purge=0 yes=0
  while [ "$#" -gt 0 ]; do
    case "$1" in
      --purge) purge=1 ;;
      --yes|-y) yes=1 ;;
      --no-service) CLI_SERVICE="none" ;;
      -h|--help)
        print_usage
        return 0
        ;;
      *)
        die 2 "Unknown option for uninstall: $1"
        ;;
    esac
    shift
  done

  detect_os_hint
  init_paths "$0"

  if [ -d "$STATE_DIR" ] || [ -d "$APP_DIR" ]; then
    stop_service || true
  fi

  if [ "$OS_KIND" = "linux" ]; then
    local unit_path
    unit_path="$(systemd_unit_path)"
    if [ -f "$unit_path" ]; then
      if has_systemd_user_bus; then
        systemctl --user disable mobileshop.service >/dev/null 2>&1 || true
      fi
      rm -f "$unit_path"
      if has_systemd_user_bus; then
        systemctl --user daemon-reload >/dev/null 2>&1 || true
      fi
    fi
  elif [ "$OS_KIND" = "macos" ]; then
    local plist_path
    plist_path="$(launchd_plist_path)"
    if [ -f "$plist_path" ]; then
      if has_launchd; then
        launchctl bootout "gui/$(id -u)/com.migueljocode.mobileshop" >/dev/null 2>&1 || true
      fi
      rm -f "$plist_path"
    fi
  fi

  local bin_link="$CLI_BIN_DIR/mobileshop"
  if [ -L "$bin_link" ]; then
    local target
    target="$(readlink "$bin_link")"
    if [ "$target" = "$LAUNCHER" ]; then
      rm -f "$bin_link"
    fi
  fi

  if [ -d "$APP_DIR" ]; then safe_rm_rf "$APP_DIR"; fi
  if [ -d "$STATE_DIR" ]; then safe_rm_rf "$STATE_DIR"; fi
  if [ -d "$CACHE_DIR" ]; then safe_rm_rf "$CACHE_DIR"; fi

  if [ "$purge" -eq 1 ]; then
    if [ "$yes" -ne 1 ]; then
      printf 'Type "delete" to permanently remove all data under %s: ' "$ROOT"
      local reply=""
      read -r reply || true
      if [ "$reply" != "delete" ]; then
        info "Purge aborted; data directory preserved at $DATA_DIR."
        return 0
      fi
    fi
    if [ -d "$DATA_DIR" ]; then safe_rm_data_only "$DATA_DIR"; fi
    if [ -d "$ROOT" ]; then safe_rm_rf "$ROOT"; fi
    info "Uninstalled MobileShop and purged $ROOT."
  else
    info "Uninstalled MobileShop; data preserved at $DATA_DIR."
  fi
}

print_usage() {
  cat <<'USAGE'
Usage: mobileshop <command> [options]

Commands:
  install [--version TAG] [--archive FILE] [--sha256 HEX] [--force] [--no-service]
  update  [--version TAG] [--archive FILE] [--sha256 HEX] [--force] [--no-service]
  rollback [--yes] [--no-service]
  prune [--keep N] [--yes]
  adopt <dir>
  backup
  start | stop | restart | status | logs
  doctor | version
  uninstall [--purge] [--yes]
  run [app-args...]
USAGE
}

main() {
  init_globals
  detect_os_hint
  init_paths "$0"

  local cmd="${1:-install}"
  if [ "$#" -gt 0 ]; then
    shift
  fi

  case "$cmd" in
    install) cmd_install "$@" ;;
    update) cmd_update "$@" ;;
    rollback) cmd_rollback "$@" ;;
    prune) cmd_prune "$@" ;;
    adopt) cmd_adopt "$@" ;;
    backup) cmd_backup "$@" ;;
    start) cmd_start "$@" ;;
    stop) cmd_stop "$@" ;;
    restart) cmd_restart "$@" ;;
    status) cmd_status "$@" ;;
    logs) cmd_logs "$@" ;;
    doctor) cmd_doctor "$@" ;;
    version) cmd_version "$@" ;;
    uninstall) cmd_uninstall "$@" ;;
    run) cmd_run "$@" ;;
    -h|--help|help) print_usage ;;
    *)
      die 2 "Unknown command: $cmd"
      ;;
  esac
}

main "$@"
