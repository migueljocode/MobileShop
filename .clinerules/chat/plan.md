# Plan — Stage X — Persian (Shamsi/Jalali) dates and factor polish

## Requirements
- Functional (from `to-do.md`)
  - **One shared Jalali date formatter** (reusable by later UI polish stages).
  - **Factor PDF** transaction and generated-at dates shown in **Shamsi (Jalali)**.
  - **Small** typography / spacing / alignment polish on the factor PDF only (not a full redesign — that is Stage AC).
  - Keep existing **Persian/RTL** and **English** PDF tests green.
- Non-functional: CI is the gate; no schema/migration; no Api host/auth.
- Constraints: Vazirmatn + QuestPDF stay the only PDF stack; Apple ID plaintext policy untouched.

## Decisions (labelled)
- **D1:** Formatter lives in **`MobileShop.Models`** (e.g. `MobileShop.Models.Extensions.JalaliDateExtensions` or `Formatting/JalaliDateFormatter`) so Web and Services can both call it without a Services→Web dependency. Prefer **`System.Globalization.PersianCalendar`** — no new NuGet package.
- **D2:** Public API (minimal):
  - `ToJalaliDateString(this DateTime value)` → e.g. `1404/07/13`
  - `ToJalaliDateTimeString(this DateTime value)` → e.g. `1404/07/13 14:30` (time stays 24h clock from the instant; document Kind handling: format the **local calendar components of the given DateTime** without changing storage).
- **D3:** **Factor PDF only** switches date display to Jalali in this stage (`RenderFactorHeader` GeneratedAt, `RenderFactorTable` row dates). English `Generate(InvoiceViewModel)` and Persian invoice `GeneratePersian` keep Gregorian unless a test already expects otherwise — **do not** change invoice date lines unless a failing test forces a one-line consistency fix.
- **D4:** UI list pages (Transactions Index, etc.) **out of scope** for date format changes here; the shared helper is the hand-off for Stage AC / later polish.
- **D5:** Factor polish is **light**: tighten padding/column balance / header hierarchy only where cheap; no new sections, no new PDF library, no redesign of Buy/Sell pages.
- **A1:** CI gate; report Action #; no local `dotnet`.
- **A2:** Known conversion fixture for tests (e.g. a fixed UTC/local pair → documented Jalali string) so the helper is proven without OCR on PDF bytes.

## Reviewer Briefing
- **Step 1 LOW/HIGH:** pure Models helper + unit tests; easy to verify.
- **Step 2 MEDIUM/MEDIUM:** QuestPdfGenerator string changes + layout polish; PDF tests must stay green; avoid accidental English-invoice churn.
- Factor already RTL + Vazirmatn; only date strings are Gregorian today (`yyyy/MM/dd HH:mm`).

## [ ] Step 1 — Shared Jalali formatter + unit tests

- Files
  - **Create:** `src/MobileShop.Models/Extensions/JalaliDateExtensions.cs` (or `Formatting/` if Extensions is crowded — prefer Extensions to match `MoneyExtensions`).
  - **Create/modify tests:** `src/MobileShop.Tests/Models/Extensions/JalaliDateExtensionsTests.cs` — at least 2–3 fixed Gregorian→Jalali assertions + null/default edge if applicable (DateTime is non-nullable).
  - Do not touch: PDF, Web, Api, schema.

- Current → Desired: no shared helper → extension methods used by later steps.

- Verify: CI green; Action #.

- Done when: helper exists, tests pass, Job B PASS.

- Risk: LOW
- Confidence: HIGH

## [ ] Step 2 — Factor PDF uses Jalali + light polish

- Files
  - Modify: `QuestPdfGenerator.cs` — replace factor date `ToString("yyyy/MM/dd HH:mm")` (header + table) with Jalali helpers.
  - Optional small layout polish in the same factor methods only (padding, column widths, header weight) — keep changes reviewable in one diff.
  - Tests: existing `QuestPdfGeneratorTests` must stay green; add assertion that a known date’s Jalali string appears in PDF text **only if** the suite already extracts text; otherwise rely on Step 1 unit tests + `AssertValidPdf`.
  - Do not touch: English invoice layout beyond necessity; Web pages; Api host.

- Done when: factor dates Shamsi; polish applied; CI green; Job B PASS.

- Risk: MEDIUM
- Confidence: MEDIUM

## [ ] Step 3 — Final Stage X validation

- Full CI green; scope check; reviewer ticks Stage X in `to-do.md`.

- Risk: LOW
- Confidence: HIGH

## Global Definition of Done
- Shared Jalali formatter in Models with tests.
- Factor PDF shows Shamsi dates.
- Light factor polish only; Persian/RTL + English PDF tests green; no schema/Api host/auth churn.

## Execution notes
One step → one commit → green Action → merge → STOP for Job B. Do not edit plan/audit/todo. Do not run local `dotnet`.

**Awaiting Job A** before Act on Step 1.
