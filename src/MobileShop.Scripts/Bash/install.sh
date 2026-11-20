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

  UI_TTY=0
  UI_COLOR=0
  UI_TRUECOLOR=0
  UI_UTF8=0
  UI_CURSOR_HIDDEN=0
  UI_LINE_ACTIVE=0
  UI_SPINNER_PID=""
}

init_ui() {
  UI_TTY=0
  UI_COLOR=0
  UI_TRUECOLOR=0
  UI_UTF8=0

  if [ "${MOBILESHOP_NO_UI:-0}" = "1" ]; then
    return 0
  fi

  if [ "${MOBILESHOP_FORCE_UI:-0}" = "1" ] || [ -t 1 ]; then
    UI_TTY=1
  fi

  if [ "$UI_TTY" -eq 1 ] && [ -z "${NO_COLOR:-}" ] && [ "${TERM:-}" != "dumb" ]; then
    UI_COLOR=1
    case "${COLORTERM:-}" in
      truecolor|24bit) UI_TRUECOLOR=1 ;;
      *)
        case "${TERM_PROGRAM:-}" in
          iTerm.app|WezTerm|ghostty|vscode) UI_TRUECOLOR=1 ;;
        esac
        ;;
    esac
  fi

  case "${LC_ALL:-${LC_CTYPE:-${LANG:-}}}" in
    *[Uu][Tt][Ff]-8*|*[Uu][Tt][Ff]8*) UI_UTF8=1 ;;
  esac
  if [ "${MOBILESHOP_FORCE_UI:-0}" = "1" ] && [ "$UI_UTF8" -eq 0 ]; then
    UI_UTF8=1
  fi
}

ui_hide_cursor() {
  if [ "${UI_TTY:-0}" -eq 1 ] && [ "${UI_CURSOR_HIDDEN:-0}" -eq 0 ]; then
    printf '\033[?25l'
    UI_CURSOR_HIDDEN=1
  fi
}

ui_show_cursor() {
  if [ "${UI_CURSOR_HIDDEN:-0}" -eq 1 ]; then
    printf '\033[?25h'
    UI_CURSOR_HIDDEN=0
  fi
}

cleanup_ui() {
  if [ -n "${UI_SPINNER_PID:-}" ]; then
    kill "$UI_SPINNER_PID" 2>/dev/null || true
    wait "$UI_SPINNER_PID" 2>/dev/null || true
    UI_SPINNER_PID=""
  fi
  if [ "${UI_LINE_ACTIVE:-0}" -eq 1 ]; then
    printf '\r\033[2K'
    UI_LINE_ACTIVE=0
  fi
  ui_show_cursor
}

cleanup_all_runtime() {
  cleanup_ui
  cleanup_lock
}

ui_c() {
  local role="$1"
  [ "${UI_COLOR:-0}" -eq 1 ] || return 0
  case "$role" in
    reset) printf '\033[0m' ;;
    bold) printf '\033[1m' ;;
    dim) printf '\033[2m' ;;
    cyan)
      if [ "$UI_TRUECOLOR" -eq 1 ]; then printf '\033[38;2;34;211;238m'; else printf '\033[38;5;45m'; fi
      ;;
    blue)
      if [ "$UI_TRUECOLOR" -eq 1 ]; then printf '\033[38;2;59;130;246m'; else printf '\033[38;5;39m'; fi
      ;;
    violet)
      if [ "$UI_TRUECOLOR" -eq 1 ]; then printf '\033[38;2;168;85;247m'; else printf '\033[38;5;141m'; fi
      ;;
    green)
      if [ "$UI_TRUECOLOR" -eq 1 ]; then printf '\033[38;2;52;211;153m'; else printf '\033[38;5;42m'; fi
      ;;
    amber)
      if [ "$UI_TRUECOLOR" -eq 1 ]; then printf '\033[38;2;251;191;36m'; else printf '\033[38;5;214m'; fi
      ;;
    red)
      if [ "$UI_TRUECOLOR" -eq 1 ]; then printf '\033[38;2;248;113;113m'; else printf '\033[38;5;203m'; fi
      ;;
    muted)
      if [ "$UI_TRUECOLOR" -eq 1 ]; then printf '\033[38;2;148;163;184m'; else printf '\033[38;5;245m'; fi
      ;;
    white)
      if [ "$UI_TRUECOLOR" -eq 1 ]; then printf '\033[38;2;241;245;249m'; else printf '\033[38;5;255m'; fi
      ;;
  esac
}

render_gradient_lines() {
  if [ "${UI_COLOR:-0}" -ne 1 ]; then
    cat
    return 0
  fi
  awk -v tc="${UI_TRUECOLOR:-0}" '
    function is_cont(b) { return (b >= 128 && b < 192) }
    BEGIN {
      for (i = 1; i < 256; i++) ord[sprintf("%c", i)] = i
      pal[1]=51; pal[2]=45; pal[3]=39; pal[4]=33; pal[5]=69; pal[6]=105; pal[7]=141; pal[8]=177
    }
    {
      line = $0
      n = length(line)
      k = 0
      i = 1
      while (i <= n) {
        c = substr(line, i, 1)
        o = (c in ord) ? ord[c] : 0
        ch = c
        i++
        if (o >= 192) {
          while (i <= n && is_cont(ord[substr(line, i, 1)])) {
            ch = ch substr(line, i, 1)
            i++
          }
        }
        k++
        chars[k] = ch
      }
      out = ""
      shift = (NR - 1) * 0.04
      for (j = 1; j <= k; j++) {
        if (chars[j] == " ") {
          out = out " "
          continue
        }
        t = (k > 1) ? ((j - 1) / (k - 1)) : 0
        t = t * 0.8 + shift
        if (t > 1) t = 1
        if (t < 0) t = 0
        if (tc == 1) {
          if (t < 0.5) {
            u = t * 2
            r = int(34 + (59 - 34) * u)
            g = int(211 + (130 - 211) * u)
            b = int(238 + (246 - 238) * u)
          } else {
            u = (t - 0.5) * 2
            r = int(59 + (168 - 59) * u)
            g = int(130 + (85 - 130) * u)
            b = int(246 + (247 - 246) * u)
          }
          out = out sprintf("\033[38;2;%d;%d;%dm%s", r, g, b, chars[j])
        } else {
          idx = int(t * 7.99) + 1
          out = out sprintf("\033[38;5;%dm%s", pal[idx], chars[j])
        }
      }
      print out "\033[0m"
    }
  '
}

render_banner() {
  [ "${UI_TTY:-0}" -eq 1 ] || return 0
  [ "${MOBILESHOP_BANNER_SHOWN:-0}" != "1" ] || return 0
  export MOBILESHOP_BANNER_SHOWN=1

  local ver
  ver="$(installer_version_label)"
  printf '\n'
  if [ "$UI_UTF8" -eq 1 ]; then
    render_gradient_lines <<'BANNER_UTF8'
  ███╗   ███╗ ██████╗ ██████╗ ██╗██╗     ███████╗███████╗██╗  ██╗ ██████╗ ██████╗
  ████╗ ████║██╔═══██╗██╔══██╗██║██║     ██╔════╝██╔════╝██║  ██║██╔═══██╗██╔══██╗
  ██╔████╔██║██║   ██║██████╔╝██║██║     █████╗  ███████╗███████║██║   ██║██████╔╝
  ██║╚██╔╝██║██║   ██║██╔══██╗██║██║     ██╔══╝  ╚════██║██╔══██║██║   ██║██╔═══╝
  ██║ ╚═╝ ██║╚██████╔╝██████╔╝██║███████╗███████╗███████║██║  ██║╚██████╔╝██║
  ╚═╝     ╚═╝ ╚═════╝ ╚═════╝ ╚═╝╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚═╝
BANNER_UTF8
    printf '  '
    printf '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━' | render_gradient_lines
  else
    render_gradient_lines <<'BANNER_ASCII'
   __  __       _     _ _       _____ _
  |  \/  |     | |   (_) |     / ____| |
  | \  / | ___ | |__  _| | ___| (___ | |__   ___  _ __
  | |\/| |/ _ \|  _ \| | |/ _ \\___ \|  _ \ / _ \|  _ \
  | |  | | (_) | |_) | | |  __/____) | | | | (_) | |_) |
  |_|  |_|\___/|_.__/|_|_|\___|_____/|_| |_|\___/| .__/
                                                 | |
                                                 |_|
BANNER_ASCII
    printf '  '
    printf -- '------------------------------------------------------------------------------' | render_gradient_lines
  fi

  printf '  %s%sMobileShop%s  %s·  Self-hostable Inventory & Sales Management  ·  installer %s%s\n\n'     "$(ui_c bold)" "$(ui_c white)" "$(ui_c reset)" "$(ui_c muted)" "$ver" "$(ui_c reset)"
}

ui_progress_bar_str() {
  local pct="$1"
  local width="${2:-22}"
  awk -v pct="$pct" -v w="$width" -v utf8="${UI_UTF8:-0}" -v color="${UI_COLOR:-0}" -v tc="${UI_TRUECOLOR:-0}" '
    BEGIN {
      if (pct < 0) pct = 0
      if (pct > 100) pct = 100
      filled = int((pct * w + 50) / 100)
      if (pct > 0 && filled == 0) filled = 1
      fill_ch = (utf8 == 1) ? "█" : "#"
      empty_ch = (utf8 == 1) ? "░" : "-"
      pal[1]=51; pal[2]=45; pal[3]=39; pal[4]=33; pal[5]=69; pal[6]=105; pal[7]=141; pal[8]=177
      out = ""
      for (i = 1; i <= w; i++) {
        if (i <= filled) {
          if (color == 1) {
            t = (w > 1) ? (i - 1) / (w - 1) : 0
            if (tc == 1) {
              if (t < 0.5) {
                u = t * 2
                r = int(34 + (59 - 34) * u)
                g = int(211 + (130 - 211) * u)
                b = int(238 + (246 - 238) * u)
              } else {
                u = (t - 0.5) * 2
                r = int(59 + (168 - 59) * u)
                g = int(130 + (85 - 130) * u)
                b = int(246 + (247 - 246) * u)
              }
              out = out sprintf("\033[38;2;%d;%d;%dm%s", r, g, b, fill_ch)
            } else {
              idx = int(t * 7.99) + 1
              out = out sprintf("\033[38;5;%dm%s", pal[idx], fill_ch)
            }
          } else {
            out = out fill_ch
          }
        } else {
          if (color == 1) out = out sprintf("\033[38;5;239m%s", empty_ch)
          else out = out empty_ch
        }
      }
      if (color == 1) out = out "\033[0m"
      printf "%s", out
    }
  '
}

ui_phase() {
  local step="$1"
  local total="$2"
  local title="$3"
  [ "${UI_TTY:-0}" -eq 1 ] || return 0
  local pct bar
  pct=$((step * 100 / total))
  bar="$(ui_progress_bar_str "$pct" 20)"
  printf '\n  %s[%d/%d]%s %s %s%3d%%%s  %s%s%s\n'     "$(ui_c violet)" "$step" "$total" "$(ui_c reset)"     "$bar"     "$(ui_c cyan)" "$pct" "$(ui_c reset)"     "$(ui_c bold)" "$title" "$(ui_c reset)"
}

render_install_card() {
  local action="$1"
  local tag="$2"
  local rid="$3"
  local port="$4"
  local root_dir="$5"
  local svc="$6"
  [ "${UI_TTY:-0}" -eq 1 ] || return 0

  local tl="+" bl="+" h="-" v="|"
  if [ "$UI_UTF8" -eq 1 ]; then
    tl="╭"; bl="╰"; h="─"; v="│"
  fi
  local rule="" i=0
  while [ "$i" -lt 62 ]; do
    rule="${rule}${h}"
    i=$((i + 1))
  done

  printf '  %s%s%s%s\n' "$(ui_c violet)" "$tl" "$rule" "$(ui_c reset)"
  printf '  %s%s%s %-12s %s%s%s\n' "$(ui_c violet)" "$v" "$(ui_c muted)" "Action" "$(ui_c bold)" "$action" "$(ui_c reset)"
  printf '  %s%s%s %-12s %s%s%s (%s)\n' "$(ui_c violet)" "$v" "$(ui_c muted)" "Release" "$(ui_c cyan)" "$tag" "$(ui_c reset)" "$rid"
  printf '  %s%s%s %-12s %shttp://localhost:%s%s  (%s)%s\n' "$(ui_c violet)" "$v" "$(ui_c muted)" "Endpoint" "$(ui_c white)" "$port" "$(ui_c muted)" "$svc" "$(ui_c reset)"
  printf '  %s%s%s %-12s %s%s%s\n' "$(ui_c violet)" "$v" "$(ui_c muted)" "Root" "$(ui_c white)" "$root_dir" "$(ui_c reset)"
  printf '  %s%s%s%s\n' "$(ui_c violet)" "$bl" "$rule" "$(ui_c reset)"
}

render_success_card() {
  local tag="$1"
  local rid="$2"
  local port="$3"
  if [ "${UI_TTY:-0}" -ne 1 ]; then
    info "Installed MobileShop ${tag} (${rid})."
    info "Open: http://localhost:${port}"
    return 0
  fi

  log_msg "INFO" "Installed MobileShop ${tag} (${rid})."
  log_msg "INFO" "Open: http://localhost:${port}"

  local tl="+" bl="+" h="-" v="|" ok="OK"
  if [ "$UI_UTF8" -eq 1 ]; then
    tl="╭"; bl="╰"; h="─"; v="│"; ok="✓"
  fi
  local rule="" i=0
  while [ "$i" -lt 62 ]; do
    rule="${rule}${h}"
    i=$((i + 1))
  done

  printf '\n  %s%s%s%s\n' "$(ui_c green)" "$tl" "$rule" "$(ui_c reset)"
  printf '  %s%s%s  %s %sInstalled MobileShop %s (%s).%s\n' \
    "$(ui_c green)" "$v" "$(ui_c bold)" "$ok" "$(ui_c white)" "$tag" "$rid" "$(ui_c reset)"
  printf '  %s%s%s  %-10s %shttp://localhost:%s%s\n' \
    "$(ui_c green)" "$v" "$(ui_c muted)" "Open:" "$(ui_c cyan)" "$port" "$(ui_c reset)"
  printf '  %s%s%s  %-10s %smobileshop status | logs | doctor%s\n' \
    "$(ui_c green)" "$v" "$(ui_c muted)" "Manage:" "$(ui_c white)" "$(ui_c reset)"
  printf '  %s%s%s%s\n' "$(ui_c green)" "$bl" "$rule" "$(ui_c reset)"
}

render_shimmer_frame() {
  local frame="$1"
  local elapsed_ds="$2"
  local label="$3"
  awk -v frame="$frame" -v ds="$elapsed_ds" -v label="$label"       -v utf8="${UI_UTF8:-0}" -v color="${UI_COLOR:-0}" -v tc="${UI_TRUECOLOR:-0}" '
    BEGIN {
      if (utf8 == 1) {
        split("⠋,⠙,⠹,⠸,⠼,⠴,⠦,⠧,⠇,⠏", sp, ",")
        sp_n = 10
        track = "━"
        dot = "●"
        trail = "╸"
      } else {
        split("|,/,-,\\", sp, ",")
        sp_n = 4
        track = "-"
        dot = "*"
        trail = "="
      }
      s = sp[(frame % sp_n) + 1]
      w = 12
      cycle = (w - 1) * 2
      pos = frame % cycle
      if (pos >= w) pos = cycle - pos
      pos = pos + 1

      bar = ""
      for (i = 1; i <= w; i++) {
        dist = i - pos
        if (dist < 0) dist = -dist
        if (i == pos) ch = dot
        else if (dist == 1) ch = trail
        else ch = track
        if (color == 1) {
          if (i == pos) bar = bar ((tc == 1) ? "\033[38;2;34;211;238m" : "\033[38;5;51m") ch
          else if (dist == 1) bar = bar ((tc == 1) ? "\033[38;2;99;102;241m" : "\033[38;5;75m") ch
          else bar = bar "\033[38;5;239m" ch
        } else {
          bar = bar ch
        }
      }
      if (color == 1) bar = bar "\033[0m"
      sec = ds / 10.0
      if (color == 1) {
        printf "\r  \033[38;5;45m%s\033[0m [%s] %s \033[2m(%.1fs)\033[0m\033[K", s, bar, label, sec
      } else {
        printf "\r  %s [%s] %s (%.1fs)", s, bar, label, sec
      }
    }
  '
}

run_with_spinner() {
  local label="$1"
  shift
  if [ "${UI_TTY:-0}" -ne 1 ]; then
    "$@"
    return $?
  fi

  local out_file err_file
  out_file="$(mktemp "${TMPDIR:-/tmp}/mobileshop-spin-out.XXXXXX")"
  err_file="$(mktemp "${TMPDIR:-/tmp}/mobileshop-spin-err.XXXXXX")"

  "$@" >"$out_file" 2>"$err_file" &
  local cmd_pid="$!"
  UI_SPINNER_PID="$cmd_pid"
  ui_hide_cursor
  UI_LINE_ACTIVE=1

  local frame=0 start_ts last_ts now_ts ds=0 base_ds=0
  start_ts="$(date +%s 2>/dev/null || printf '0')"
  last_ts="$start_ts"
  while kill -0 "$cmd_pid" 2>/dev/null; do
    now_ts="$(date +%s 2>/dev/null || printf '0')"
    if [ "$now_ts" -gt "$last_ts" ]; then
      base_ds=$(((now_ts - start_ts) * 10))
      if [ "$base_ds" -gt "$ds" ]; then
        ds="$base_ds"
      else
        ds=$((ds + 1))
      fi
      last_ts="$now_ts"
    elif [ "$frame" -gt 0 ]; then
      ds=$((ds + 1))
    fi
    render_shimmer_frame "$frame" "$ds" "$label"
    frame=$((frame + 1))
    sleep 0.12
  done

  local rc=0
  if wait "$cmd_pid"; then
    rc=0
  else
    rc=$?
  fi
  UI_SPINNER_PID=""
  now_ts="$(date +%s 2>/dev/null || printf '0')"
  base_ds=$(((now_ts - start_ts) * 10))
  if [ "$base_ds" -gt "$ds" ]; then
    ds="$base_ds"
  elif [ "$ds" -eq 0 ] && [ "$frame" -gt 0 ]; then
    ds=1
  fi

  printf '\r\033[2K'
  UI_LINE_ACTIVE=0
  ui_show_cursor

  local sec_str
  sec_str="$(awk -v ds="$ds" 'BEGIN { printf "%.1fs", ds / 10.0 }')"
  local ok_mark="OK" fail_mark="FAIL"
  if [ "$UI_UTF8" -eq 1 ]; then
    ok_mark="✓"; fail_mark="✗"
  fi

  if [ "$rc" -eq 0 ]; then
    printf '  %s%s%s %s %s(%s)%s\n'       "$(ui_c green)" "$ok_mark" "$(ui_c reset)" "$label" "$(ui_c muted)" "$sec_str" "$(ui_c reset)"
    if [ -s "$err_file" ]; then
      cat "$err_file" >&2
    fi
  else
    printf '  %s%s%s %s %s(failed, %s)%s\n'       "$(ui_c red)" "$fail_mark" "$(ui_c reset)" "$label" "$(ui_c muted)" "$sec_str" "$(ui_c reset)" >&2
    if [ -s "$out_file" ]; then
      cat "$out_file" >&2
    fi
    if [ -s "$err_file" ]; then
      cat "$err_file" >&2
    fi
  fi

  rm -f "$out_file" "$err_file"
  return "$rc"
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
  if [ "${UI_COLOR:-0}" -eq 1 ]; then
    local mark="*"
    [ "${UI_UTF8:-0}" -eq 1 ] && mark="•"
    printf '  %s%s%s %s\n' "$(ui_c cyan)" "$mark" "$(ui_c reset)" "$*"
  else
    printf '%s\n' "$*"
  fi
}

warn() {
  log_msg "WARN" "$@"
  if [ "${UI_COLOR:-0}" -eq 1 ]; then
    printf '  %sWarning:%s %s\n' "$(ui_c amber)" "$(ui_c reset)" "$*" >&2
  else
    printf 'Warning: %s\n' "$*" >&2
  fi
}

die() {
  local code="$1"
  shift
  log_msg "ERROR" "$@"
  cleanup_ui
  if [ "${UI_COLOR:-0}" -eq 1 ]; then
    printf '  %sError:%s %s\n' "$(ui_c red)" "$(ui_c reset)" "$*" >&2
  else
    printf 'Error: %s\n' "$*" >&2
  fi
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

render_download_frame() {
  local cur="$1"
  local total="$2"
  local initial="$3"
  local ds="$4"
  local frame="$5"
  local label="$6"
  local final="$7"

  awk -v cur="$cur" -v total="$total" -v init="$initial" -v ds="$ds"       -v frame="$frame" -v label="$label" -v final="$final"       -v utf8="${UI_UTF8:-0}" -v color="${UI_COLOR:-0}" -v tc="${UI_TRUECOLOR:-0}" '
    function fmt_bytes(b) {
      if (b < 1024) return sprintf("%d B", b)
      if (b < 1048576) return sprintf("%.1f KB", b / 1024.0)
      if (b < 1073741824) return sprintf("%.1f MB", b / 1048576.0)
      return sprintf("%.2f GB", b / 1073741824.0)
    }
    function fmt_speed(bps) {
      if (bps <= 0) return "-- B/s"
      if (bps < 1024) return sprintf("%.0f B/s", bps)
      if (bps < 1048576) return sprintf("%.1f KB/s", bps / 1024.0)
      return sprintf("%.1f MB/s", bps / 1048576.0)
    }
    function fmt_eta(sec) {
      if (sec < 0) return "--:--"
      h = int(sec / 3600)
      m = int((sec % 3600) / 60)
      s = int(sec % 60)
      if (h > 0) return sprintf("%d:%02d:%02d", h, m, s)
      return sprintf("%02d:%02d", m, s)
    }
    BEGIN {
      if (utf8 == 1) {
        split("⠋,⠙,⠹,⠸,⠼,⠴,⠦,⠧,⠇,⠏", sp, ",")
        sp_n = 10; fill_ch = "█"; empty_ch = "░"; ok_ch = "✓"
      } else {
        split("|,/,-,\\", sp, ",")
        sp_n = 4; fill_ch = "#"; empty_ch = "-"; ok_ch = "OK"
      }
      pal[1]=51; pal[2]=45; pal[3]=39; pal[4]=33; pal[5]=69; pal[6]=105; pal[7]=141; pal[8]=177
      w = 22
      elapsed = ds / 10.0
      delta = cur - init
      if (delta < 0) delta = cur
      bps = (elapsed > 0.05) ? (delta / elapsed) : 0

      if (final == 1) {
        pct = 100
        filled = w
      } else if (total > 0) {
        pct = int((cur * 100) / total)
        if (pct > 100) pct = 100
        if (pct < 0) pct = 0
        filled = int((pct * w) / 100)
        if (cur > 0 && filled == 0) filled = 1
      } else {
        pct = -1
      }

      bar = ""
      if (pct >= 0) {
        for (i = 1; i <= w; i++) {
          if (i <= filled) {
            if (color == 1) {
              t = (w > 1) ? (i - 1) / (w - 1) : 0
              if (tc == 1) {
                if (t < 0.5) {
                  u = t * 2
                  r = int(34 + (59 - 34) * u); g = int(211 + (130 - 211) * u); b = int(238 + (246 - 238) * u)
                } else {
                  u = (t - 0.5) * 2
                  r = int(59 + (168 - 59) * u); g = int(130 + (85 - 130) * u); b = int(246 + (247 - 246) * u)
                }
                bar = bar sprintf("\033[38;2;%d;%d;%dm%s", r, g, b, fill_ch)
              } else {
                idx = int(t * 7.99) + 1
                bar = bar sprintf("\033[38;5;%dm%s", pal[idx], fill_ch)
              }
            } else {
              bar = bar fill_ch
            }
          } else {
            if (color == 1) bar = bar sprintf("\033[38;5;239m%s", empty_ch)
            else bar = bar empty_ch
          }
        }
      } else {
        cycle = (w - 1) * 2
        pos = (frame % cycle)
        if (pos >= w) pos = cycle - pos
        pos = pos + 1
        for (i = 1; i <= w; i++) {
          d = i - pos; if (d < 0) d = -d
          if (d <= 1) {
            bar = bar ((color == 1) ? "\033[38;5;45m" : "") fill_ch
          } else {
            bar = bar ((color == 1) ? "\033[38;5;239m" : "") empty_ch
          }
        }
      }
      if (color == 1) bar = bar "\033[0m"

      size_str = (total > 0) ? (fmt_bytes(cur) " / " fmt_bytes(total)) : fmt_bytes(cur)
      spd_str = fmt_speed(bps)
      if (final == 1) {
        eta_str = sprintf("%.1fs", elapsed)
        icon = (color == 1) ? sprintf("\033[38;5;42m%s\033[0m", ok_ch) : ok_ch
        pct_str = "100%"
        printf "\r  %s %s  [%s] %4s  %s  %s  %s\033[K\n", icon, label, bar, pct_str, size_str, spd_str, eta_str
      } else {
        s = sp[(frame % sp_n) + 1]
        icon = (color == 1) ? sprintf("\033[38;5;45m%s\033[0m", s) : s
        pct_str = (pct >= 0) ? sprintf("%3d%%", pct) : " ... "
        if (total > 0 && cur >= total) eta_str = "ETA 00:00"
        else if (total > 0 && bps > 1) {
          rem_s = (total - cur) / bps
          ceil_s = int(rem_s)
          if (rem_s > ceil_s) ceil_s = ceil_s + 1
          eta_str = "ETA " fmt_eta(ceil_s)
        }
        else eta_str = "ETA --:--"
        if (color == 1) {
          printf "\r  %s %s  [%s] \033[38;5;51m%s\033[0m  %s  \033[2m%s  %s\033[0m\033[K", icon, label, bar, pct_str, size_str, spd_str, eta_str
        } else {
          printf "\r  %s %s  [%s] %s  %s  %s  %s", icon, label, bar, pct_str, size_str, spd_str, eta_str
        }
      }
    }
  '
}

probe_content_length() {
  local url="$1"
  curl -fsSLI --connect-timeout 2 --max-time 4 "$url" 2>/dev/null \
    | awk '{ k = tolower($1); if (k == "content-length:") { v = $2; gsub(/[^0-9]/, "", v); if (v != "") len = v } } END { if (len ~ /^[0-9]+$/) print len; else print 0 }' \
    || printf '0\n'
}

curl_download() {
  local url="$1"
  local dest="$2"
  local label="${3:-$(basename "$dest")}"
  local part="${dest}.part"
  local is_file=0
  case "$url" in
    file://*) is_file=1 ;;
  esac

  if [ "$is_file" -eq 1 ]; then
    rm -f "$part"
  fi

  if [ "${UI_TTY:-0}" -ne 1 ]; then
    if [ "$is_file" -eq 1 ]; then
      if ! curl -fLSs --retry 5 --retry-delay 2 --retry-all-errors "$url" -o "$part"; then
        rm -f "$part"
        return 1
      fi
    else
      if ! curl -fLSs --retry 5 --retry-delay 2 --retry-all-errors -C - "$url" -o "$part"; then
        rm -f "$part"
        return 1
      fi
    fi
    mv -f "$part" "$dest"
    return 0
  fi

  local total_bytes initial_bytes err_file
  total_bytes="$(probe_content_length "$url")"
  case "$total_bytes" in
    ''|*[!0-9]*) total_bytes=0 ;;
  esac
  initial_bytes="$(file_size_bytes "$part")"
  err_file="$(mktemp "${TMPDIR:-/tmp}/mobileshop-dl-err.XXXXXX")"

  if [ "$is_file" -eq 1 ]; then
    curl -fLSs --retry 5 --retry-delay 2 --retry-all-errors "$url" -o "$part" 2>"$err_file" &
  else
    curl -fLSs --retry 5 --retry-delay 2 --retry-all-errors -C - "$url" -o "$part" 2>"$err_file" &
  fi
  local dl_pid="$!"
  UI_SPINNER_PID="$dl_pid"
  ui_hide_cursor
  UI_LINE_ACTIVE=1

  local frame=0 start_ts last_ts now_ts ds=0 base_ds=0 cur_bytes
  start_ts="$(date +%s 2>/dev/null || printf '0')"
  last_ts="$start_ts"
  while kill -0 "$dl_pid" 2>/dev/null; do
    cur_bytes="$(file_size_bytes "$part")"
    now_ts="$(date +%s 2>/dev/null || printf '0')"
    if [ "$now_ts" -gt "$last_ts" ]; then
      base_ds=$(((now_ts - start_ts) * 10))
      if [ "$base_ds" -gt "$ds" ]; then
        ds="$base_ds"
      else
        ds=$((ds + 2))
      fi
      last_ts="$now_ts"
    else
      ds=$((ds + (frame % 2) + 1))
    fi
    render_download_frame "$cur_bytes" "$total_bytes" "$initial_bytes" "$ds" "$frame" "$label" 0
    frame=$((frame + 1))
    sleep 0.15
  done

  local rc=0
  if wait "$dl_pid"; then
    rc=0
  else
    rc=$?
  fi
  UI_SPINNER_PID=""
  now_ts="$(date +%s 2>/dev/null || printf '0')"
  base_ds=$(((now_ts - start_ts) * 10))
  if [ "$base_ds" -gt "$ds" ]; then
    ds="$base_ds"
  elif [ "$ds" -eq 0 ]; then
    ds=1
  fi

  if [ "$rc" -eq 0 ]; then
    cur_bytes="$(file_size_bytes "$part")"
    if [ "$total_bytes" -le 0 ] || [ "$cur_bytes" -gt "$total_bytes" ]; then
      total_bytes="$cur_bytes"
    fi
    render_download_frame "$cur_bytes" "$total_bytes" "$initial_bytes" "$ds" "$frame" "$label" 1
    UI_LINE_ACTIVE=0
    ui_show_cursor
    rm -f "$err_file"
    mv -f "$part" "$dest"
    return 0
  fi

  printf '\r\033[2K'
  UI_LINE_ACTIVE=0
  ui_show_cursor
  if [ -s "$err_file" ]; then
    cat "$err_file" >&2
  fi
  rm -f "$err_file" "$part"
  return 1
}

resolve_latest_tag_to_file() {
  local out_file="$1"
  local tag
  tag="$(resolve_latest_tag)" || return $?
  printf '%s\n' "$tag" > "$out_file"
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

debug_log() {
  [ -n "${MOBILESHOP_DEBUG_FILE:-}" ] || return 0
  printf '%s %s\n' "$(date '+%H:%M:%S' 2>/dev/null || printf '??')" "$*" >> "$MOBILESHOP_DEBUG_FILE" 2>/dev/null || true
}

# True when the pid exists and is not a zombie (a zombie still answers kill -0).
pid_alive() {
  local pid="$1" stat
  kill -0 "$pid" 2>/dev/null || return 1
  stat="$(ps -o stat= -p "$pid" 2>/dev/null || true)"
  case "$stat" in
    *Z*) return 1 ;;
  esac
  return 0
}

is_pid_running() {
  [ -f "$PID_FILE" ] || return 1
  local pid
  pid="$(cat "$PID_FILE" 2>/dev/null || true)"
  case "$pid" in
    ''|*[!0-9]*) return 1 ;;
  esac
  pid_alive "$pid"
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
  while [ "$i" -lt 100 ]; do
    if ! port_is_listening "$port"; then
      if [ "$i" -ge 20 ]; then
        debug_log "wait_for_port_free: port $port freed after ~$((i / 10))s"
      fi
      return 0
    fi
    sleep 0.1
    i=$((i + 1))
  done
  debug_log "wait_for_port_free: port $port still listening after 10s: $(lsof -nP -iTCP:"$port" -sTCP:LISTEN 2>&1 | head -n 3 | tr '\n' ';')"
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
  if pid_alive "$pid"; then
    kill "$pid" 2>/dev/null || true
    i=0
    while [ "$i" -lt 100 ]; do
      if ! pid_alive "$pid"; then
        break
      fi
      sleep 0.1
      i=$((i + 1))
    done
    if [ "$i" -ge 20 ]; then
      debug_log "stop_pid_mode: pid $pid took ~$((i / 10))s after TERM: $(ps -o pid=,ppid=,stat=,command= -p "$pid" 2>&1 | head -n 1)"
    fi
    if pid_alive "$pid"; then
      kill -9 "$pid" 2>/dev/null || true
    fi
  fi
  rm -f "$PID_FILE"
}

inherit_saved_service_mode() {
  [ "$CLI_SERVICE" = "auto" ] || return 0
  read_installed_env
  if [ "$INST_SERVICE_MODE" = "none" ]; then
    CLI_SERVICE="none"
  fi
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
  trap cleanup_all_runtime EXIT INT TERM
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
      # shellcheck disable=SC2016  # intentional backticks around command name in message
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
  # Subshell intentionally isolates env for the one-shot process.
  # shellcheck disable=SC2030,SC2031
  (
    cd "$DATA_DIR" || exit 1
    export ASPNETCORE_ENVIRONMENT="Production"
    export DOTNET_ENVIRONMENT="Production"
    export ASPNETCORE_CONTENTROOT="$content_root"
    export MOBILESHOP_DATA_DIRECTORY="$DATA_DIR"
    export ASPNETCORE_URLS="http://localhost:${CLI_PORT}"
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

  # These exports apply to the exec'd process; SC2031 is a false positive from the
  # earlier subshell in run_with_app_env (different call path).
  # shellcheck disable=SC2031
  export ASPNETCORE_ENVIRONMENT="Production"
  # shellcheck disable=SC2031
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
  render_banner

  local target_tag=""
  if [ -n "$req_version" ]; then
    target_tag="$(validate_tag "$req_version")"
  elif [ -n "$archive_file" ]; then
    [ -f "$archive_file" ] || die 3 "Archive not found: $archive_file"
    target_tag="$(infer_tag_from_archive "$archive_file")"
  else
    ui_phase 1 6 "Resolving latest release"
    local tag_tmp
    tag_tmp="$(mktemp "${TMPDIR:-/tmp}/mobileshop-tag.XXXXXX")"
    if ! run_with_spinner "Querying latest release tag" resolve_latest_tag_to_file "$tag_tmp"; then
      rm -f "$tag_tmp"
      die 3 "Failed to query latest release from ${CLI_BASE_URL}/${CLI_REPO}/releases/latest."
    fi
    local raw_tag
    raw_tag="$(cat "$tag_tmp" 2>/dev/null || true)"
    rm -f "$tag_tmp"
    target_tag="$(validate_tag "$raw_tag")"
  fi

  if [ -n "$INST_TAG" ] && [ "$INST_TAG" = "$target_tag" ] && [ "$force" -eq 0 ]; then
    info "Already up to date (${target_tag})."
    return 0
  fi

  local action_desc="Fresh install"
  if [ -n "$INST_TAG" ] && [ "$INST_TAG" != "$target_tag" ]; then
    action_desc="Upgrade ${INST_TAG} -> ${target_tag}"
  elif [ -n "$INST_TAG" ]; then
    action_desc="Reinstall ${target_tag}"
  fi
  render_install_card "$action_desc" "$target_tag" "$RID" "$CLI_PORT" "$ROOT" "$CLI_SERVICE"

  # Pre-flight checks before modifying anything
  [ -w "$DATA_DIR" ] || die 6 "Data directory is not writable: $DATA_DIR"
  if ! is_our_app_running; then
    if port_is_listening "$CLI_PORT"; then
      die 6 "Port ${CLI_PORT} is already in use by another process."
    fi
  fi

  local archive_name="MobileShop-${target_tag}-${RID}.tar.gz"
  local local_archive=""

  ui_phase 2 6 "Downloading & verifying release assets"
  if [ -n "$archive_file" ]; then
    [ -f "$archive_file" ] || die 3 "Archive not found: $archive_file"
    local_archive="$archive_file"
    local archive_base
    archive_base="$(basename "$archive_file")"
    if [ -n "$sha256_override" ]; then
      if ! run_with_spinner "Verifying SHA-256 checksum" verify_sha256 "$local_archive" "$sha256_override"; then
        die 3 "SHA-256 checksum mismatch for $archive_file."
      fi
    else
      local sidecar_checksums
      sidecar_checksums="$(dirname "$archive_file")/checksums.txt"
      if [ -f "$sidecar_checksums" ]; then
        local expected_sum
        expected_sum="$(lookup_checksum "$sidecar_checksums" "$archive_base")"
        if [ -n "$expected_sum" ]; then
          if ! run_with_spinner "Verifying SHA-256 checksum" verify_sha256 "$local_archive" "$expected_sum"; then
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
    if ! curl_download "${base_dl}/checksums.txt" "$checksums_dest" "checksums.txt"; then
      die 3 "Failed to download checksums.txt for ${target_tag}."
    fi
    local expected_sum
    expected_sum="$(lookup_checksum "$checksums_dest" "$archive_name")"
    [ -n "$expected_sum" ] || die 3 "No checksum entry for ${archive_name} in checksums.txt."

    if ! curl_download "${base_dl}/${archive_name}" "$archive_dest" "$archive_name"; then
      die 3 "Failed to download ${archive_name}."
    fi
    if ! run_with_spinner "Verifying SHA-256 checksum" verify_sha256 "$archive_dest" "$expected_sum"; then
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

  ui_phase 3 6 "Extracting release bundle"
  # Extract and validate in stage dir
  local stage_dir="$VERSIONS_DIR/.stage-${target_tag}"
  safe_rm_rf "$stage_dir"
  mkdir -p "$stage_dir"

  if ! run_with_spinner "Extracting $(basename "$local_archive")" tar -xzf "$local_archive" -C "$stage_dir"; then
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

  ui_phase 4 6 "Backing up & migrating database"
  log_msg "INFO" "Running database migrations for ${target_tag}..."
  local migrate_rc=0
  if [ "${UI_TTY:-0}" -eq 1 ]; then
    if run_with_spinner "Migrating database for ${target_tag}" run_with_app_env "$version_dir" "$version_dir/MobileShop.Web" --migrate-database; then
      migrate_rc=0
    else
      migrate_rc=$?
    fi
  else
    info "Running database migrations for ${target_tag}..."
    if run_with_app_env "$version_dir" "$version_dir/MobileShop.Web" --migrate-database; then
      migrate_rc=0
    else
      migrate_rc=$?
    fi
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

  ui_phase 5 6 "Installing launcher & switching version"
  # Switch current symlink and install launcher
  install_launcher_and_symlink "${target_tag:-}"
  ln -sfn "versions/${target_tag}" "$CURRENT_LINK"

  ui_phase 6 6 "Starting MobileShop & verifying health"
  log_msg "INFO" "Starting MobileShop (${target_tag}) on http://localhost:${CLI_PORT}..."
  local started_ok=0
  if [ "${UI_TTY:-0}" -eq 1 ]; then
    if start_service && run_with_spinner "Waiting for http://localhost:${CLI_PORT}/Account/Login" poll_health "$CLI_PORT"; then
      started_ok=1
    fi
  else
    info "Starting MobileShop (${target_tag}) on http://localhost:${CLI_PORT}..."
    if start_service && poll_health "$CLI_PORT"; then
      started_ok=1
    fi
  fi
  if [ "$started_ok" -ne 1 ]; then
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

  render_success_card "$target_tag" "$RID" "$CLI_PORT"
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

  render_banner
  local tmp_script="$DOWNLOADS_DIR/install.sh.next"
  local tmp_sums="$DOWNLOADS_DIR/checksums-update.txt"
  if ! curl_download "$checksums_url" "$tmp_sums" "checksums.txt"; then
    die 3 "Failed to download checksums.txt from $checksums_url."
  fi
  local expected_sum
  expected_sum="$(lookup_checksum "$tmp_sums" "install.sh")"
  [ -n "$expected_sum" ] || die 3 "No checksum entry for install.sh in checksums.txt."

  if ! curl_download "$script_url" "$tmp_script" "install.sh"; then
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
  inherit_saved_service_mode
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
  inherit_saved_service_mode
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
  init_ui
  trap cleanup_all_runtime EXIT INT TERM
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
