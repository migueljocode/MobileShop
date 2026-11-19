Commit: 50e976b — fix(scripts): silence remaining shellcheck SC2016/SC2030/SC2031 in install.sh
Verification: Action: pending (pushed to main; waiting for .NET CI)
Limitations: None — only shellcheck directives; no behaviour change.
Friction noted: Previous partial SC2031 disables were incomplete; one directive before the subshell covers SC2030+SC2031 cleanly.
Problems: None
Status: COMPLETE
