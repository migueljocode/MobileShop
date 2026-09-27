# Act Report — Step 3

## Commit
- **Hash**: 1216bd3
- **Message**: feat(web): add profit estimation (percent ↔ dollars) fields on Create Phone & Create Apple ID pages

## Changes
1. **`src/MobileShop.Models/ViewModels/Web/BindModels/CreatePhoneInputModel.cs`**
   - Added `ProfitPercent?` (nullable decimal, `[Range(0, 100)]`)
   - Added `ProfitAmount?` (nullable decimal, `[Range(0, double.MaxValue)]`)

2. **`src/MobileShop.Models/ViewModels/Web/BindModels/CreateAppleIdInputModel.cs`**
   - Added `ProfitPercent?` (nullable decimal, `[Range(0, 100)]`)
   - Added `ProfitAmount?` (nullable decimal, `[Range(0, double.MaxValue)]`)

3. **`src/MobileShop.Web/Pages/Products/CreatePhone.cshtml`**
   - Added `data-price` attribute to Price input
   - Added Profit % input-group (with `%` suffix) and Profit $ input-group (with `$` prefix) fields after Price
   - Added JS in existing `<script>` block: two-way sync between percent ↔ amount based on Price; handles blank/zero gracefully

4. **`src/MobileShop.Web/Pages/Products/CreateAppleId.cshtml`**
   - Added `data-price` attribute to Price input
   - Added Profit % input-group (with `%` suffix) and Profit $ input-group (with `$` prefix) fields after Price
   - Added `<script>` in `@section Scripts` block with same two-way sync logic

## Verification
- **Command**: `dotnet build src/MobileShop.slnx --nologo`
- **Result**: Build succeeded, 0 Warning(s), 0 Error(s)

## Status
- **COMPLETE** — Step 3 implementation done, awaiting reviewer approval
