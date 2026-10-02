#!/usr/bin/env bash
set -u
usage(){ cat <<'EOF'
Usage: log.sh --level LEVEL [--above|--below] [filters] [-n N] [-p|--path PATH] [PATH|-]
LEVEL: debug info warning error fatal. Exact is default.
--above/--below include selected plus more/less severe.
-n/--number N prints first N matching events; N must be positive.
-p/--path PATH selects one log file; PATH may also be supplied positionally. Use - for stdin.
--date-from/--date-to: YYYY-MM-DD. --date-time-from/--date-time-to: "YYYY-MM-DD HH:mm:ss".
Date/datetime modes cannot mix. From-only uses current local date/time as to; to-only uses first event as from.
Default input: logs/app-*.log. PATH selects one file literally; - reads stdin and pipelines. Events/help stdout; diagnostics stderr; invalid usage is non-zero.
Bounds are inclusive. From-only uses current local date/time; to-only infers the lower bound from the first event; no bounds means no time restriction. Date and datetime modes cannot mix.
Examples: ./log.sh --level error; ./log.sh --level error --path 'logs/my app.log'; ./log.sh --level error --above -n 20; ./log.sh --level warning --date-from 2026-01-01 --date-to 2026-01-31; ./log.sh --level error --date-time-from '2026-01-01 08:00:00' --date-time-to '2026-01-01 18:00:00'; cat logs/app-*.log | ./log.sh --level error.
Invalid examples: --date-from 2026-02-30, reversed ranges, --above --below, and -n 0 are rejected with stderr diagnostics.
EOF
}
die(){ printf 'log.sh: %s\n' "$*" >&2; exit 2; }
rank(){ case "$1" in debug)echo 0;;info)echo 1;;warning)echo 2;;error)echo 3;;fatal)echo 4;;*)return 1;;esac; }
valid_date(){ [[ "$1" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}$ ]]||return 1; date -d "$1" '+%Y-%m-%d' >/dev/null 2>&1&&[[ "$(date -d "$1" '+%Y-%m-%d')" == "$1" ]]&&return 0; date -j -f '%Y-%m-%d' "$1" '+%Y-%m-%d' >/dev/null 2>&1; }
valid_dt(){ [[ "$1" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}[[:space:]][0-9]{2}:[0-9]{2}:[0-9]{2}$ ]]||return 1;valid_date "${1:0:10}"||return 1;local h=${1:11:2} m=${1:14:2} s=${1:17:2};((10#$h<24&&10#$m<60&&10#$s<60));}
E=();T=();L=()
read_events(){ local x c='' t='' r='' re='^([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2}\.[0-9]{3}) [+-][0-9]{2}:[0-9]{2}';re+=$'\t(DBG|INF|WRN|ERR|FTL)\t';while IFS= read -r x||[[ -n "$x" ]];do if [[ "$x" =~ $re ]];then [[ -z "$c" ]]||{ E+=( "$c" );T+=( "$t" );L+=( "$r" );};t="${BASH_REMATCH[1]}";case "${BASH_REMATCH[2]}" in DBG)r=0;;INF)r=1;;WRN)r=2;;ERR)r=3;;FTL)r=4;;esac;c="$x";elif [[ -n "$c" ]];then c+=$'\n'"$x";fi;done;[[ -z "$c" ]]||{ E+=( "$c" );T+=( "$t" );L+=( "$r" );};}
main(){ local lev='' mode=0 n='' p='' df='' dt='' tf='' tt=''
while (($#));do case "$1" in
-h|--help)usage;return;;
-l|--level) (($#>1))||die "missing value for $1";lev="$2";shift 2;;
--above)((mode==0))||die "--above and --below are mutually exclusive";mode=1;shift;;
--below)((mode==0))||die "--above and --below are mutually exclusive";mode=-1;shift;;
-n|--number)(($#>1))||die "missing value for $1";n="$2";shift 2;;
-p|--path)(($#>1))||die "missing value for $1";[[ -z "$p" ]]||die "multiple input paths";p="$2";shift 2;;
--date-from)(($#>1))||die "missing value for --date-from";df="$2";shift 2;;
--date-to)(($#>1))||die "missing value for --date-to";dt="$2";shift 2;;
--date-time-from)(($#>1))||die "missing value for --date-time-from";tf="$2";shift 2;;
--date-time-to)(($#>1))||die "missing value for --date-time-to";tt="$2";shift 2;;
-)p=-;shift;;
-*)die "unknown option: $1";;
*)[[ -z "$p" ]]||die "multiple input paths";p="$1";shift;;
esac;done
[[ -n "$lev" ]]||die "--level is required (use --help)";local want;want=$(rank "$lev")||die "invalid level '$lev'";
if [[ -n "$n" ]] && ! [[ "$n" =~ ^[1-9][0-9]*$ ]]; then die "--number must be a positive integer"; fi
if { [[ -n "$df" ]] || [[ -n "$dt" ]]; } && { [[ -n "$tf" ]] || [[ -n "$tt" ]]; }; then die "date and datetime filters cannot be mixed"; fi
if [[ -n "$df" ]] && ! valid_date "$df"; then die "invalid --date-from '$df'; expected YYYY-MM-DD"; fi
if [[ -n "$dt" ]] && ! valid_date "$dt"; then die "invalid --date-to '$dt'; expected YYYY-MM-DD"; fi
if [[ -n "$tf" ]] && ! valid_dt "$tf"; then die "invalid --date-time-from '$tf'; expected YYYY-MM-DD HH:mm:ss"; fi
if [[ -n "$tt" ]] && ! valid_dt "$tt"; then die "invalid --date-time-to '$tt'; expected YYYY-MM-DD HH:mm:ss"; fi
if [[ -z "$p" ]]; then p='logs/app-*.log'; fi
if [[ "$p" == - ]];then read_events;else
 if [[ "$p" == logs/app-*.log ]]; then shopt -s nullglob; local fs=( $p ); [[ -n "${fs[0]-}" ]]||die "input log file not found: $p"; local f; for f in "${fs[@]}";do read_events<"$f";done
 else [[ -f "$p" ]]||die "input log file not found: $p"; read_events<"$p"
 fi
fi
local first="${T[0]-}"
if [[ -n "$df" || -n "$dt" ]]; then [[ -n "$first" ]]||die "cannot infer lower date bound from empty input";[[ -n "$df" ]]||df="${first:0:10}";[[ -n "$dt" ]]||dt="$(date '+%Y-%m-%d')";[[ "$df" > "$dt" ]]&&die "date range is reversed";
elif [[ -n "$tf" || -n "$tt" ]]; then [[ -n "$first" ]]||die "cannot infer lower datetime bound from empty input";[[ -n "$tf" ]]||tf="${first:0:19}";[[ -n "$tt" ]]||tt="$(date '+%Y-%m-%d %H:%M:%S')";[[ "$tf" > "$tt" ]]&&die "datetime range is reversed";fi
local i ok c=0 stamp;for i in "${!E[@]}";do
if ((mode==0));then ok=$((L[i]==want));elif ((mode>0));then ok=$((L[i]>=want));else ok=$((L[i]<=want));fi
((ok))||continue;stamp="${T[i]}";
if [[ -n "$df" ]] && { [[ "${stamp:0:10}" < "$df" ]] || [[ "${stamp:0:10}" > "$dt" ]]; }; then continue; fi
if [[ -n "$tf" ]] && { [[ "${stamp:0:19}" < "$tf" ]] || [[ "${stamp:0:19}" > "$tt" ]]; }; then continue; fi
printf '%s\n' "${E[i]}";((c++));if [[ -n "$n" ]] && (( c >= n )); then break; fi
done
}
main "$@"