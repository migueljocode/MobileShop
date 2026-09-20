# Catalog Normalization — Roadmap & Agent Prompts

Status: planning complete, ready to execute. Feed the prompts below to the VS Code
agent one at a time, in order — each assumes the previous ones are done.

## Why this order

1–3 are small, independent cleanups that reduce risk before the big schema change lands.
4–6 are the schema work itself — the bulk of it. 7 (PDF shell) has no dependency on the
schema and can happen any time after step 3, including in parallel with 4–6 if you want to
context-switch. 8–9 depend on the new schema existing, so they're last.

| # | Step | Depends on |
|---|------|------------|
| 1 | Drop-and-recreate DB for dev | — |
| 2 | Kill reflection `Hash()`, move admin seeding to Services | — |
| 3 | Consolidate startup guard + trim eager `Find`/`FindAll` overrides | — |
| 4 | New entities, enums, Product/Model/Phone/Guarantee/SecondHand updates | 1–3 |
| 5 | Migration + seed data rewrite | 4 |
| 6 | One stub repo (`Manufacturer`) for DI | 4 |
| 7 | PDF service shell (`IPdfGenerator`/`QuestPdfGenerator`/`PdfSettings`) | — |
| 8 | `ProductDataService` (generalized inventory) | 5, 6 |
| 9 | Invoice ViewModel + per-entity extension methods + `InvoiceDataService` | 5, 7, 8 |

---

## Prompt 1 — Drop-and-recreate DB for dev

> In `MobileShop.Dal.Initialization` (wherever the DB startup / `EnsureDefaultAdminPassword`
> logic currently lives), replace the migration-based startup (`context.Database.Migrate()`)
> with `context.Database.EnsureDeleted()` followed by `context.Database.EnsureCreated()`,
> guarded by `if (env.IsDevelopment())`. Don't touch or remove any migration files yet — just
> stop calling `Migrate()` in the dev path. Since the database is now recreated on every run,
> there's no persisted state to protect between runs, so remove the "is the password hash
> already set" guard in `EnsureDefaultAdminPassword` — it's dead code under this workflow.

## Prompt 2 — Kill reflection Hash(), move admin seeding to Services

> Find the reflection call using `GetMethod("Hash")` in `MobileShop.Dal.Initialization`.
> Move the "ensure a default admin exists with a hashed password" logic out of
> `MobileShop.Dal` entirely and into a new method in `MobileShop.Services` (e.g.
> `AdminSeeder.EnsureDefaultAdmin`), called from `MobileShop.Web`'s `Program.cs` at startup,
> after the DI container is built, so it can resolve `IPasswordHasher` and `IUserRepo`/
> `IUserDataService` normally through the service provider. Delete the reflection code
> entirely and call `_passwordHasher.Hash(...)` directly instead.

## Prompt 3 — Consolidate startup guard + trim eager Find/FindAll overrides

> Two independent changes in one pass — do them in this order and report each separately.
>
> **Part A — Consolidate the startup guard.** Move the `AdminSeeder.EnsureDefaultAdmin()`
> call out of `Program.cs` and into `WebApplicationBuilderExtensions.ConfigureApp()`, placed
> directly after the `DatabaseInitializer.InitializeForDevelopment(app.Services)` call,
> inside the same `if (app.Environment.IsDevelopment())` block, using a scope the same way
> `Program.cs` currently does. `Program.cs` should end up as just:
> ```csharp
> var builder = CreateBuilder(args).ConfigureBuilder();
> var app = builder.Build().ConfigureApp();
> app.Run();
> ```
>
> **Part B — Trim eager Find/FindAll overrides.** In `AppleIdRepo`, `PhoneRepo`,
> `CustomerRepo`, `SellerRepo`, and `TransactionRepo`, remove the overridden `Find`/
> `FindAll` (and their Async versions) that eagerly `.Include()` related data — let them
> fall back to `BaseRepo<T>`'s plain implementation.
>
> This removal will not cause a compile error on its own — any call site relying on a
> populated navigation property from the old eager-loaded `Find`/`FindAll` will still build,
> but will get `null` back at runtime instead. So: search **every** class in
> `MobileShop.Services.DataServices` (AppleId, Customer, Phone, Seller, Transaction, User,
> and any others — not just the ones touched in earlier prompts) for every call to `Find`/
> `FindAll`/their Async versions, and for each one check whether it dereferences a
> navigation property afterward (e.g. `.ProductNavigation`, `.PersonNavigation`,
> `.Transactions`). Where it does, convert that call to the existing `Select`/`SelectAll`
> projection overloads instead, shaped to fetch exactly the data that method needs — don't
> just re-add `.Include()` at the call site.
>
> Report back explicitly which DataService methods you found and converted, not just that
> the build passed — a clean build proves less here than in the last two prompts, since
> this is the first change that can break at runtime without breaking compilation.

## Prompt 4 — Schema normalization

> Implement the following in `MobileShop.Models`. Schema only — do not touch DataServices,
> Repos beyond what's listed, or any Web/Api pages in this step.
>
> **New enums** (`Models/Enums`): `StorageKind` (Ssd, Hdd, MicroSd, Sd, MiniSd, UsbFlash, Cf,
> Other), `CableConnector` (UsbC, UsbA, Lightning, MicroUsb — add others if obviously missing).
>
> **New entities** (all `: BaseEntity`):
> - `Manufacturer(Name)` — unique index on `Name`
> - `Model(ManufacturerId, CategoryId, Name, ModelNumber?)` — unique index on
>   `(ManufacturerId, Name)`. `CategoryId` lives here, not on `Product` — a model's category
>   (Phone, Cable, ...) is a permanent fact about that model, not something that varies per
>   physical unit sold
> - `Category(Name)`
> - `Color(Name)`
> - `StorageCapacity(Gb)` — unique index on `Gb`; shared lookup referenced by both
>   `DeviceSpec` and `PortableStorage` below — do not duplicate the GB number in each
> - `DeviceSpec(ProductId, Ram, StorageCapacityId, ChargeSpeed)` — 1:1 with Product.
>   Document units in XML docs only (Ram/StorageCapacity in GB, ChargeSpeed in Watts) —
>   no unit suffix in the property names
> - `AppleInfo(ProductId, BatteryHealth, ChargeCycle, Notes?)` — 1:1 with Product; this
>   replaces `IPhone` entirely and is not specific to phones
> - `Tablet(ProductId, Notes?)` — stub
> - `SmartWatch(ProductId, Notes?)` — stub
> - `Laptop(ProductId, Cpu, Gpu, DisplaySize, Notes?)`
> - `Cable(ProductId, Connector1, Connector2, Length, Notes?)`
> - `Charger(ProductId, Wattage, Pd, PortCount, Notes?)`
> - `PowerBank(ProductId, CapacityMah, MaxWattage, PortCount, Pd, Notes?)`
> - `PortableStorage(ProductId, Kind, StorageCapacityId, Speed?, Notes?)` — the sellable
>   storage-device category (SD cards, USB drives). This is distinct from
>   `DeviceSpec.StorageCapacityId` (a phone/laptop's built-in capacity) even though both
>   reference the same `StorageCapacity` table — give both very explicit XML docs
>   clarifying the distinction so they aren't confused later
> - `Case(ProductId, Notes?)`
> - `CaseModelFit(CaseId, ModelId)` — explicit many-to-many join entity between Case and
>   Model, unique index on `(CaseId, ModelId)`
> - `Glass(ProductId, Notes?)` — screen protectors
> - `GlassModelFit(GlassId, ModelId)` — explicit many-to-many join entity between Glass and
>   Model, unique index on `(GlassId, ModelId)`. Enforce in Services (not the schema) that a
>   linked Model's `CategoryNavigation.Name` is one of Phone/Tablet/SmartWatch — same
>   enforcement style as "Apple-only" on `AppleInfo`
>
> **Update `Product`**: remove the `Manufacturer` and `Model` string properties, and remove
> any `CategoryId` if already present — category now comes from `Product.ModelNavigation.
> CategoryNavigation`. Add `ModelId` (FK to `Model` only — do NOT add a separate
> `ManufacturerId` or `CategoryId`; both are reached via `Product.ModelNavigation`). Add
> `ColorId?` and `Barcode` (required, unique index). Add navigation properties for all the
> new 1:1 profiles above.
>
> **Update `Phone`**: remove `Color` (now lives on `Product.ColorId`). Add `Notes?`.
>
> **Update `Guarantee` and `SecondHand`**: add `Notes?` to each.
>
> **Delete `IPhone`** entirely, including its Configuration class.
>
> Add a Configuration class (Fluent API, matching the existing style) for every new entity
> — unique indexes as noted above, `DeleteBehavior.NoAction` on relationships matching the
> existing convention. Update `AppDbContext`: remove `DbSet<IPhone>`, add DbSets for every
> new entity.

## Prompt 5 — Migration + seed data

> Generate the EF Core migration for the schema in Prompt 4
> (`dotnet ef migrations add NormalizeCatalog` from `MobileShop.Dal`). Since dev now uses
> `EnsureDeleted`/`EnsureCreated` (Prompt 1), the migration doesn't need to preserve existing
> data, but review the generated SQL for correctness — unique indexes and FK delete behavior
> in particular — before moving on.
>
> Update `sample-data.json` and the `SampleDataLoader`/`SampleData` deserialization types to
> match: add `manufacturers`, `categories`, `colors`, and `storageCapacities` arrays, and a
> `models` array where each entry carries its own `categoryId` (not the products). Update
> `products` entries to reference `modelId`/`colorId`/`barcode` instead of free-text
> manufacturer/model/category — split any existing "128GB"-style suffix out of the old model
> name into a `deviceSpecs` entry rather than keeping it in the model name. Rename the
> `iPhones` array to `appleInfos`, now keyed by `productId` instead of `phoneId`. Add example
> entries for any new profile types (including `glasses`/`glassModelFits`) you want
> represented in sample data.

## Prompt 6 — One stub repo for DI

> Create `IManufacturerRepo : IBaseRepo<Manufacturer>` and `ManufacturerRepo`, in
> `MobileShop.Dal.Repos.Interfaces`/`MobileShop.Dal.Repos`, following the exact bare
> pass-through pattern of `IGuaranteeRepo`/`GuaranteeRepo` — no custom methods needed yet.
> This exists so the DI composition root has at least one concrete example among the new
> entities to register; add repos for the others the same way only once something actually
> needs more than plain CRUD on them.

## Prompt 7 — PDF service shell

> Create `MobileShop.Services/PDF/`, mirroring the existing `Security/` folder structure:
> `IPdfGenerator` (one method, `byte[] Generate(InvoiceViewModel model)` — `InvoiceViewModel`
> can be a minimal placeholder type if it doesn't exist yet), `QuestPdfGenerator :
> IPdfGenerator` (the only class allowed to reference QuestPDF types), and
> `Settings/PdfSettings.cs` (page size, margins, shop name/address/phone/Instagram defaults,
> bound from configuration the same way `AppLoggingSettings` is). Add the QuestPDF package
> to `MobileShop.Services.csproj` and set `QuestPDF.Settings.License = LicenseType.Community;`
> once at startup in `MobileShop.Web`'s `Program.cs`.

## Prompt 8 — ProductDataService

> Generalize `AppleIdDataService.GetInventoryRows()`/`ProductListItemViewModel` into a new
> `IProductDataService`/`ProductDataService` in `MobileShop.Services.DataServices.Dal`,
> following the same `DataServiceBase` pattern as the other data services. It should return
> inventory rows across every category, not just AppleId, including denormalized display
> fields (manufacturer name and category name via `Model.ManufacturerNavigation`/
> `Model.CategoryNavigation`, model name, color name) via `Select`/`SelectAll` projections —
> no eager `Include`s.

## Prompt 9 — Invoice ViewModel + InvoiceDataService

> Build `InvoiceViewModel` matching the original invoice spec: buyer name, national ID,
> phone number, transaction date, guarantee information, ownership-transfer flag (if the
> product is a Phone) — for a Buy-direction transaction instead: seller information, product
> information, date, financial information, product count, notes. Add one extension method
> per relevant profile entity (Phone, AppleId, SecondHand, Guarantee, Laptop, Cable, Charger,
> PowerBank, PortableStorage, Case, Glass, Tablet, SmartWatch) returning
> `IEnumerable<(string Label, string Value)>` with that entity's invoice-relevant extras,
> following the exact pattern of `SecondHandExtensions.GetUsedDurationBreakdown()`. Build
> `IInvoiceDataService`/`InvoiceDataService` in `DataServices.Dal` that assembles the
> ViewModel from a `Transaction` and calls `IPdfGenerator.Generate(...)`.
