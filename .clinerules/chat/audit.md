# Audit — Job B: Stage Y Step 2 (ModelState repair)

**Verdict: FAIL**

**Repair commit:** `2a9f067` — `ModelState.Clear()` + `TryValidateModel(input)`  
**CI:** Action **#484 — Failure** (3 tests)

## Production intent: OK
Clearing ModelState then validating only the create model is the right approach for AJAX handlers next to `[BindProperty] Input`.

## Failure
`PersonCreateHandlerTests` all NRE:

```
PageModel.TryValidateModel(Object model, String name)
```

`TryValidateModel` needs a configured **PageContext** / object validator. Unit tests construct `new BuyModel(...)` without that infrastructure, so the handler throws before the service runs.

## Required fix
Prefer **DataAnnotations** validation that works in both host and tests, e.g.:

```csharp
var results = new List<ValidationResult>();
if (!Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true))
    return CreateErrorResult(...);
```

(or set up `PageContext` + `IObjectModelValidator` in tests — heavier).

Re-run until Action is green. **Do not start Step 3.**
