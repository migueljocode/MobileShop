Commit: 34885a2 — fix(scripts): make T14 cwd assertion robust on macOS path canonicalization
(also 50e976b — fix(scripts): silence remaining shellcheck SC2016/SC2030/SC2031 in install.sh)
Verification: Action: pending (run after 34885a2)
Limitations: None
Friction noted: macOS CI uses physical /private/tmp while the test held the logical path; Ubuntu was already green after the shellcheck-only fix.
Problems: None
Status: COMPLETE
