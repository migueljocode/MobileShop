## Actor report — Stage 20, Step 2 — Group rows into factor lines

- Commit: `92340102` feat(models) — `TransactionFactorRowViewModel` gains trailing optional `SpecKey`/`SpecLabel`; new `TransactionFactorLineViewModel` (`PaidAmount` = `UnitPrice` × `Quantity`); `TransactionFactorViewModel.Lines` groups by (`ProductLabel`, `SpecKey`, `Direction`, `FinishedPrice`, `PersonLabel`) in first-appearance order; `TotalPrice` unchanged.
- Files: modified the three view-model files above; created `TransactionFactorLineViewModel.cs` and `src/MobileShop.Tests/Models/ViewModels/TransactionFactorViewModelTests.cs`.
- Existing callers (`ToFactorRow` overloads) compile unchanged; they pass no spec data yet, which Step 3 fills in.
- Deviation: none.
- CI: run #700 Success on `92340102`.
