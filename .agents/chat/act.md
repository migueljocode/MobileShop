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

# Act — Stage 4 (Installer UI Upgrade & Stage Tracking)

- **Scope**: Upgraded `src/MobileShop.Scripts/Bash/install.sh` with terminal UI visuals and progress telemetry, extended `src/MobileShop.Scripts/tests/test-install.sh` with `T16`, and ticked Stages 1–4 in `.agents/TODO.md`.
- **Installer Enhancements (`src/MobileShop.Scripts/Bash/install.sh`)**:
  - Added capability detection (`init_ui` checking `MOBILESHOP_NO_UI`, `MOBILESHOP_FORCE_UI`, `[ -t 1 ]`, `NO_COLOR`, `TERM`, `COLORTERM`, `TERM_PROGRAM`, UTF-8 locale) with automatic cursor hide/show and interrupt cleanup (`cleanup_ui`, `cleanup_all_runtime`).
  - Added multi-byte UTF-8 safe diagonal TrueColor (256-color fallback) `awk` gradient renderer (`render_gradient_lines`) and `MobileShop` block/ASCII banner (`render_banner`, preserving `__VERSION__` placeholder and single-banner guard `MOBILESHOP_BANNER_SHOWN`).
  - Added `[1/6]`..`[6/6]` phase progress bar (`ui_phase`, `ui_progress_bar_str`), metadata & completion cards (`render_install_card`, `render_success_card`), and animated Braille/ASCII spinner with sliding shimmer track (`run_with_spinner`, `render_shimmer_frame`).
  - Upgraded `curl_download` with `probe_content_length`, live gradient progress bar, downloaded/total bytes, transfer speed (`B/s`, `KB/s`, `MB/s`), percentage (`0%`–`100%`), and ceiling-rounded `ETA MM:SS` countdown (`render_download_frame`).
- **Verification**:
  - `bash -n src/MobileShop.Scripts/Bash/install.sh src/MobileShop.Scripts/tests/test-install.sh` -> syntax OK.
  - `bash src/MobileShop.Scripts/tests/test-install.sh` -> `ALL 16 INSTALLER SCENARIOS PASSED (linux-x64)` (including `T16 animated UI, gradient banner, ETA & NO_COLOR`).
- **Status**: COMPLETE (Stages 1–4 ticked in `.agents/TODO.md`)
