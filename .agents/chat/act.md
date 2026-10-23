## Actor report — Stage 20, Step 1 — Spec identity helpers

- Commits: `86a8d6f` feat(models) — new `ProductSpecExtensions.cs` and its tests; `1cdb2bf` fix(tests) — helper name; `594f69e` fix(tests) — storage id tied to capacity.
- Files: created `src/MobileShop.Models/Extensions/ProductSpecExtensions.cs`, `src/MobileShop.Tests/Models/Extensions/ProductSpecExtensionsTests.cs`. Nothing else in source changed.
- CI: run #697 Failure (compile error: `MakeMakePhone` in the test file, from a bad string replacement; read from check-run annotations); run #698 Failure (test run, logs not readable from the sandbox; inferred as the storage test, whose two products shared `StorageCapacityId`); run #699 Success on `594f69e`.
- Deviation: none from the plan. Step 1 is complete; no `Products`, entity, migration or Api file changed.
- Evidence: CI #699 Success.
