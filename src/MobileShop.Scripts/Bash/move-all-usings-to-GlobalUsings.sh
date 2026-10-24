#!/bin/bash

# use this script only in you solutions root directory.

FILE=GlobalUsings.cs

has_csproj() {
	compgen -G "$1*.csproj" >/dev/null
}

# Process each project directory in the solution root.
for project in */; do
	(

		has_csproj "$project" || exit

		# Enter the project directory without changing the loop's directory.
		cd "$project" || exit

		tempfile=$(mktemp)
		[[ -f "$FILE" ]] && cat "$FILE" >"$tempfile"

		# Convert file-scoped using directives to global using directives.
		fd -e cs -X sed -i '/^using /s/^/global /'

		# Collect unique global using directives into one file.
		rg -IN '^global using ' >>"$tempfile"

		# Remove global using directives from the original C# files.
		fd -e cs -X sed -i '/^global using /d'

		sort -u "$tempfile" >"$FILE"
		rm -f "$tempfile"

		# Delete GlobalUsings.cs if it is empty or missing.
		[[ -s "$FILE" ]] || rm -f "$FILE"
	)
done
