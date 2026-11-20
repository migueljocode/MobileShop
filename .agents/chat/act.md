# Act — Stage 3 (Steps 9–11)

- **Scope**: Headphone Web UI (create page, products tab, navigation menu link, edit form & page model, details block, index category mapping), Web page tests, README inventory list, and final validation.
- **Verification**:
  - `./src/MobileShop.Scripts/Bash/check.sh` -> `STATUS: PASS` (`restore: PASS`, `build: PASS`, `test: PASS`, `graphify: PASS`, `graph_commit: PASS` at graph commit `d1537de`)
  - Targeted Stage 3 + migrator tests (`CreateAccessoryModelTests`, `EditModelTests`, `DatabaseMigratorTests`) -> `63/63 Passed`
  - `DatabaseMigratorTests` -> `19/19 Passed`
  - Step 1 migration grep (`AlterColumn|DropColumn|InsertData|UpdateData|\.Sql\(` on `src/MobileShop.Dal/Migrations/20261121100000_AddHeadphones.cs`) -> clean (no matches)
  - `src/MobileShop.Api` untouched (`git diff --name-only 81cb777..HEAD -- src/MobileShop.Api` empty)
  - GitHub Actions CI -> Pending at commit time (run triggered on push to `main`).
- **Files changed (Stage 3)**:
  - Created: `src/MobileShop.Web/Pages/Products/CreateHeadphone.cshtml`, `src/MobileShop.Web/Pages/Products/CreateHeadphone.cshtml.cs`
  - Modified: `src/MobileShop.Web/Pages/Products/Index.cshtml`, `src/MobileShop.Web/Pages/Products/Index.cshtml.cs`, `src/MobileShop.Web/ViewComponents/NavigationMenuViewComponent.cs`, `src/MobileShop.Web/Pages/Products/Edit.cshtml.cs`, `src/MobileShop.Web/Pages/Shared/_ProductEditForm.cshtml`, `src/MobileShop.Web/Pages/Products/Details.cshtml`, `src/MobileShop.Tests/Web/Pages/Products/CreateAccessoryModelTests.cs`, `src/MobileShop.Tests/Web/Pages/Products/EditModelTests.cs`, `README.md`, `.agents/chat/plan.md`, `.agents/chat/act.md`
- **Full change set (`81cb777..HEAD`)**:
  - Stage 1: `HeadphoneKind`, `HeadphoneConnection`, `Headphone`, `HeadphoneConfiguration`, `Product`, `AppDbContext`, `20261121100000_AddHeadphones(.Designer).cs`, `AppDbContextModelSnapshot`, `MigrationChainTests`, `SampleDataLoader`, `SampleDataInitializer`, `sample-data.json`, `docs/erd.mermaid`
  - Stage 2: `CreateHeadphoneInputModel`, `EditProductInputModel`, `ProductDetailsViewModel`, `ProductSpecExtensions`, `InvoiceProfileExtensions`, `IProductsDataService`, `ApiProductsDataService`, `ProductsDataService`, `TransactionsDataService`, `ProductSpecExtensionsTests`, `ProductsDataServiceTests`, `TransactionsDataServiceTests`
  - Stage 3: listed above
- **Manual smoke checklist**:
  1. Create 2 headphones with color and guarantee via **Products → Headphones → Create headphone**.
  2. Confirm both appear in the `Headphones` tab on `/Products/Index`.
  3. Open Details for a headphone and verify Kind, Connection, Noise cancelling, Connector, Microphone, Color, Guarantee, and Notes.
  4. Edit color, headphone fields, and guarantee on `/Products/Edit` (confirm Guarantee block is visible and no Second-hand section appears).
  5. Sell one headphone via **Transactions → Record a sale** and verify the invoice lines include headphone specs.
  6. Upgrade over an existing SQLite database and confirm existing tables/rows are preserved unchanged.
- **Status**: COMPLETE (Stages 1–3 complete)
