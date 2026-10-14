# Reviewer Audit — Job A: plan.md (Stage 7 + Proposed stages 7–16)

## Verified
- Rules read: `AGENTS.md`, `roles/reviewer.md`, `planner.md`, `chatbot.md`, `project.md`, `TODO.md`. Repo fetched: HEAD `6f2b88d`, no upstream change; reviewed `plan.md` is the uncommitted working copy (106,886 B). Baseline CI: Action: #618 — Success (last code commit `597fba4`; HEAD is docs-only).
- Steps 7.1–7.3 dry-run on copies of the real files: block extraction appends 75 lines, 0 hex, CSS parses with 0 errors (tinycss2); 7.2 edits keep the BOM (`ef bb bf`) and give numstat `2 1`; 7.3 old blocks each match exactly once; every Verify grep passes. Files and symbols exist; the focus-ring duplicate is in `app-theme.css`; `form-floating` has 0 uses.
- 7.1 is inert: 74 new names (75 lines incl. the `@supports` fallback), 0 collisions and 0 uses in any css, cshtml or js.
- 7.2 effect enumerated against the shipped Bootstrap 5.3.3: 52 variables differ in dark; the app's `:root` overrides (`--bs-body-bg/-color`, `--bs-border-color`, `--bs-link-*`, `--bs-secondary/-tertiary-color`) win by order; `.table`, `.nav-tabs`, `.alert*`, `.input-group-text`, `.modal-content`, `:disabled` and `.jalali-picker-feedback` are overridden by the app. Residual visible changes: F-12 chevron (`#343a40` to `#dee2e6`), F-13 `.btn-close` (dark-only rule exists), F-44 person-picker hover (not under `.product-picker-results`), `<code>` colour (Details, Error). Context7 and Bootstrap docs confirm `data-bs-theme="dark"` on `<html>` is the documented route.
- Reference claims re-checked true: F-01..F-03, F-06, F-09, F-12, F-13, F-17 (24 of 29, tag-aware), F-18, F-21, F-22 (11 + Transactions script), F-27..F-29, F-33..F-35, F-37, F-44, F-45 (scoped `.footer` absolute vs app-theme static; MS Learn confirms selector rewriting with `b-` attribute). External: `prefers-reduced-transparency` Chrome/Edge 119, Firefox and Safari unsupported (web-features, Oct 2026); cross-document view transitions Chrome/Edge 126, Safari 18.2, Firefox none; WCAG levels; Apple "Adopting Liquid Glass" statements (title-style headers, sheets more opaque when expanded, concentric shapes, no layering).
- F-19 is stronger than written: `person-picker.js` hides the result list on blur and has no key handler, so a keyboard user cannot reach or select a result (2.1.1).

## Findings
No CRITICAL or HIGH.

MEDIUM
- R4 F-15 / COMP-004a–d: counts and split are wrong. Occurrences are 113 (111 was a line count): 105 in the 11 `_ProductCreate*Form` partials, 1 each in 7 of 11 Create pages (AppleId, Cable, Glass, Laptop, Phone, SmartWatch, Tablet), 1 in Edit, 0 in `_ProductEditForm`; all 115 partial controls have no `id`. Re-cut the sub-tasks and effort; add "give every control an id (idPrefix)".
- Baseline timing: DISC-005 is "folded into QA-001" (Stage 16) but R19 wants it before Stage 8, and Step 7.2 is visible with a "looks identical" check and no baseline. Add an explicit owner baseline gate before Step 7.2.
- Mission §11 gap: no list of security-sensitive UI changes for separate review (Login, Profile password fields, `_PasswordField`, Apple ID plaintext display, anti-forgery token handling in modal partials and `product-create-modal.js`, factor download links).
- Stage 8 mixes three concerns (visual controls, markup accessibility over 19 files, type tokens; 11 tasks incl. L): split label association plus validation (COMP-004, COMP-005) into its own stage ahead of the visuals.
- R24 misstates SHELL-003 acceptance: Stage 5 delays are `transition-delay: 0ms, 0ms, <n>ms, <n>ms` per `.navigation-link:nth-child(k)` (45–230 ms), `0s, 0s, 0.082s, 0.082s` appears nowhere; `.is-initializing` has two rules (`grep -c` = 2, not one).
- Step 7.1 `sed` uses `$` anchors on plan.md and appends to LF files: not CRLF-safe on a Windows checkout (no `.gitattributes`; fails safe at Verify but costs a round). Add `tr -d '\r'`, drop the `$`.
- Step 7.3 is unrelated cleanup (AGENTS "Minimal change": mark OUT OF SCOPE): drop it or label it in the step itself, not only in FOUND-003.
- Token block lacks `--success` and `--info` (mission §6C); add aliases to `--accent`.
- End state unclear: "≤ 8 CSS files" ignores `components.css` and `site.css` (QA-006 deletes only `app-theme.css`); no task for font preload (R7) or the `_Icons` sprite (R11.D).

LOW
- 7.1 duplicate check (`grep -o '^\s+--x:' | sort | uniq -d`) is indentation-sensitive and only passes because the indented `@supports` `--ease-spring` differs; use `[[:space:]]` and strip indentation (expect exactly `--ease-spring`).
- Step 7.4 omits template fields (Symbols, Depends on, Edge cases, Tests, Verify).
- Bare `COMP-004` (9 times) and `ROUTE-008` (3) are undefined parent IDs.
- R7: "hex literals outside tokens.css: 0" holds for the five app stylesheets only (scoped template file has 9 lines); "15 literal durations" are 15 lines.
- F-08 `.btn` is about 38px (0.55rem x2 + 21px + 2px at 14px root), not 36px; "Chrome 118/119+" is 119; R14 "password reveal pattern unchanged" should say plaintext `<code>` display unchanged (project.md); R8 lists 3.2.6 and 3.3.7 as AA, they are Level A.
- Step 7.2 can be re-rated LOW risk / HIGH confidence now that the effect set is enumerated; its manual check should also name the Error-page request id `<code>`.
- Not audited though listed in the mission: scrollbar appearance, helper text (`form-text`), checkbox alignment, icon sizing (placeholder contrast passes: `--ink-muted` on ground at least 6:1 [C]); add INFO rows or list in R23.
- Login card is G3 while R9 #1 keeps forms opaque: state the exception. New partials (`_FormField`, `_Modal`, `_Icons`, `_PasswordField`, `_PageHead`, `_StatTile`) touch AGENTS "speculative abstractions": justify with counts when each stage activates.

## Missing Implementation Details (expand when each stage activates; none blocks Stage 7)
- Exact legacy-to-new button class map (9 variants to 4); `_FormField`, `_PasswordField`, `_Modal` parameters and the id/idPrefix scheme; segmented-control markup and ARIA pattern; ambient-layer gradient positions and values; scroll-edge fade CSS; end-state CSS file list.

## Owner decisions (not plan defects)
- D-01 / Q-01: the plan keeps the whole reference in `plan.md`, which conflicts with `planner.md` ("active stage only, overwrite"). AGENTS.md says report, never invent: the owner decides to accept it or move the Reference out. Q-02..Q-10 stay open; none blocks Stage 7.
- Not verified here: official HIG Materials page (JavaScript-only; the Firecrawl call was not approved, mirror only), browser-rendered effects ([U] items), SF Pro Rounded licence, Safari `backdrop-filter: url()` behaviour (community sources).

## Status
Job A: **APPROVED** — Stage 7 (Steps 7.1–7.4) and the ten Proposed stages; MEDIUM/LOW are optional notes.

## Gate
Next: orchestrator — answer D-01, commit `plan.md` and `audit.md`. Then Planner — add Stages 7–16 to `TODO.md`, delete `## Proposed stages`, fold the cheap MEDIUM/LOW notes. Then Actor — Step 7.1 only.
