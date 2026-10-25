#!/usr/bin/env bash
# Run from the solution source root (the folder containing the project folders), e.g. src/. Needs rg.
# Moves file-level usings of every project into <project>/GlobalUsings.cs.
set -euo pipefail

FILE=GlobalUsings.cs
USING='^using +(static +)?([A-Za-z_]\w* *= *[^;]+|[A-Za-z_:][A-Za-z0-9_.:]*) *;'
command -v rg >/dev/null || { echo "rg not found" >&2; exit 1; }

for project in */; do
	compgen -G "$project*.csproj" >/dev/null || continue
	(
		cd "$project"
		mapfile -t files < <(rg -l -g '*.cs' -g "!$FILE" -e "$USING" . || true)
		((${#files[@]})) || exit 0

		usings=$({
			if [[ -f $FILE && -s $FILE ]]; then
				cat "$FILE"
				# A missing trailing newline would glue the next using onto the last line.
				[[ -n $(tail -c1 "$FILE") ]] && echo
			fi
			rg -INo -e "$USING" "${files[@]}" | sed 's/^/global /'
		} | LC_ALL=C sort -u)

		sed -i -E "/$USING/d; /./,\$!d" "${files[@]}"
		printf '%s\n' "$usings" >"$FILE"
	)
done
