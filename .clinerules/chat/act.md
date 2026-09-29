# Act Report — Stage B Step 3
- Commit: 707b69e — test(home): strengthen dashboard coverage
- Verification:
  - `dotnet build src/MobileShop.slnx --nologo && dotnet test src/MobileShop.slnx --nologo --no-build` -> exit 0; 0 warnings, 0 errors; 456 passed, 0 failed, 2 skipped (458 total).
  - Chain 2 guard: host log contained `Hosting environment: Production` and `Application started` before any route check; the `if (app.Environment.IsDevelopment())` block containing `DatabaseInitializer.InitializeForDevelopment` was therefore skipped (no EnsureDeleted/EnsureCreated/Seed lines in the log).
  - Routes: `200 /`, `200 /Products`, `200 /Products/SecondHand`, `200 /People/Customers`, `200 /People/Sellers`, `200 /Transactions`, `200 /Reports/ProfitLoss`, `200 /Account/Login`.
  - Database: `stat -c '%s %y' MobileShop.db` before `4096 2026-09-29 17:46:58.851080603 +0330` -> after `299008 2026-09-30 00:04:37.845685056 +0330`, and `MobileShop.db-wal` / `MobileShop.db-shm` no longer exist. Not a Development-init wipe — mechanism corrected in the reviewer note below. Main `MobileShop.db` is gitignored.
- Limitations: none for the step itself. The Step 3 "file size must be identical" criterion is unsound against a WAL-backed database and is superseded by the reviewer note below.
- Friction noted: the interactive command window (~30s) is shorter than the ~44s test run, forcing the test to outlive the foreground `wait` via a redirect log so completion could be observed.
- Problems: None.
- Status: COMPLETE

---

## Reviewer correction (reviewer, after the report — provenance noted)

The report originally attributed the 4,096 -> 299,008 byte growth to *"EF Core lazily initializing the
SQLite schema on first Production read of an empty placeholder file"*. **That mechanism was wrong**, and
the conclusion (no Development wipe) was right for the wrong stated reason. Corrected account:

- The schema was never empty. `InitializeForDevelopment` created and seeded it at 17:46, and that work
  had been sitting in a 638,632-byte `MobileShop.db-wal` because no connection had closed cleanly since.
- The Production pass **read** that data; when the host shut down, SQLite checkpointed the WAL into the
  main file and removed the `-wal`/`-shm` sidecars. That is why the size is now exactly 299,008 bytes —
  the same size as the pre-incident database, because it is the same seeded dataset.
- Confirmed by read-only query (`sqlite3 "file:MobileShop.db?mode=ro"`): Products 17, Phones 7,
  AppleIds 3, Transactions 26, Sellers 3, Customers 4, Manufacturers 6, Colors 6, Guarantees 4,
  SecondHands 2, Users 1.
- The `Hosting environment: Production` guard was the right check and it held. What must **not** be
  inferred from this report is that the dev database is an empty placeholder EF materializes on demand:
  it holds the sample data seeded at 2026-09-29 17:46.

**Superseded criterion — applies to Stage C onward.** Step 3 required `stat -c '%s %y' MobileShop.db` to
be identical before and after the pass. Against a WAL-backed database that check is unsound: any clean
Production pass that reads data checkpoints the WAL and legitimately changes the file's size and mtime.
It passed in Stage A only by accident of timing. Use instead: (1) the guard line says `Production`,
(2) the log shows no `InitializeForDevelopment` activity, and (3) if a data-level check is wanted, a
row-count fingerprint such as
`sqlite3 "file:MobileShop.db?mode=ro" "select (select count(*) from Products), (select count(*) from Phones), (select count(*) from Transactions);"`
before and after. File size alone proves nothing — a wipe-and-reseed also lands near 299 KB.
