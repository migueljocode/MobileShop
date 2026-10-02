# Audit — Stage R — Plan Review

## Verdict

**CHANGES REQUESTED**

The Stage R plan is directionally correct and covers the requested cross-platform command family, `-n/--number`, help, streams, stdin, CI, and non-pro usability. Before implementation, one naming contract needs to be made explicit and consistent:

- The plan defines exact commands as `log-<level>` but examples use plural user-facing names such as `log-errors` and `log-warnings`.
- The implementation contract must explicitly list the final command names for every supported level rather than leaving `<level>` ambiguous.
- In particular, define whether the family uses `log-debug`, `log-info`, `log-warning`, `log-error`, `log-fatal`, or pluralized forms such as `log-errors` and `log-warnings`. The above/below variants must follow the same chosen naming convention.
- The mapping between command names and Serilog levels should also be explicit, especially `info` → Information.

Everything else in the reviewed plan is acceptable for Stage R and can remain as written.

**Implementation should not begin until the naming contract is clarified in `.clinerules/chat/plan.md`.**
