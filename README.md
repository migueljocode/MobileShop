# MobileShop
ASP.NET Core inventory &amp; sales system for a local mobile phone shop, with IMEI-tracked stock and invoice generation.

i am now in stage 3 and should continue it then when it is completed push it to github then ask claude to verify it.

## Database upgrades (Production)

Production database schema changes are explicit and backed up. Do not rely on normal Production startup to create or upgrade the database.

From the repository root, run:

```bash
dotnet run --project src/MobileShop.Web -- --migrate-database
```

The migration command verifies a database backup before changing an existing schema. Backups are written beside the database as `MobileShop.db.<timestamp>.bak`. A second run against an already-current database reports that it is up to date and does not create another backup.

The command reports one of these outcomes:

- **Created** — the database file was absent (or was an existing empty file), so all migrations were applied without needing a backup.
- **UpToDate** — the database already has the complete migration history and no schema change was needed.
- **Upgraded** — a database with migration history had pending migrations; a verified backup was created before the upgrade.
- **Baselined** — a legacy database had no EF migration history, but its schema exactly matched the current model; a verified backup was created before the migration history was recorded.

If a legacy database has no migration history and its schema does not exactly match the current model, the command refuses to modify it. Resolve that schema difference deliberately before attempting an upgrade; normal Production startup also refuses stale or unrecognized schemas and does not mutate the database.

The CI Production smoke script is intentionally destructive to its workspace: it deletes the repository-root `MobileShop.db`, its SQLite `-wal`/`-shm` files, matching `.bak` files, and `MobileShop.Log` before creating its disposable test database. It refuses to run locally unless `CI=true` or `MOBILESHOP_SMOKE_ALLOW_DELETE=1` is explicitly set.
