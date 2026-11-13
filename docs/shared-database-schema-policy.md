# Shared database schema policy (expand-only)

While the legacy (.NET Framework) hosts and the Azure hosts run against the **same SQL database**, every schema change must be backward compatible with the older host. Applies to `src/NuGetGallery/Migrations` (EF6 migrations) and `src/DatabaseMigrationTools` (the tool that applies them).

## Rules

1. **Expand only.** `Up()` may: add tables, add nullable columns or columns with defaults, add indexes, add foreign keys/constraints that existing writers cannot violate, add views/procs.
2. **Forbidden in `Up()`:** `DropTable`, `DropColumn`, `RenameColumn/Table/Index`, `AlterColumn` (type/nullability narrowing), `DropIndex`, `DropForeignKey`, `DropPrimaryKey`, `MoveTable`, and raw `Sql()` containing `DROP`, `sp_rename`, `ALTER COLUMN`, `TRUNCATE`, `DELETE FROM`, `UPDATE ... SET`. `Down()` is not checked, but see rollback.
3. **Contract later.** Remove/rename only in a separate release after *all* legacy hosts are decommissioned (or two releases after no host reads the old shape): release N adds the new shape and dual-writes; N+1 reads the new shape; contract after legacy is gone.
4. **Data backfills** are done by a job/script, not in `Up()`, and must be idempotent and batched.
5. **Single migrator.** Only the DatabaseMigrationTools pipeline applies migrations; hosts never auto-migrate at startup.
6. **Deploy order:** migrate DB first (expand), then deploy hosts. Never deploy a host that needs a column before it exists.
7. **Exceptions:** add `// shared-db: allow-breaking <reason/ticket>` in the migration file; requires explicit reviewer approval (CODEOWNERS) and a written compatibility analysis in the PR.

Migrations up to `202608031928454` are grandfathered (baseline in `build/Test-AdditiveOnlyMigrations.ps1`).

## Enforcement

- `build/Test-AdditiveOnlyMigrations.ps1` scans every migration newer than the baseline and fails on forbidden operations in `Up()`. Run locally: `pwsh build/Test-AdditiveOnlyMigrations.ps1`.
- CI job `migration-additive-check` (`.github/workflows/nugetgallery-ci.yml`) runs it on every PR/push.
- Self-test: `pwsh tests/Scripts/Test-AdditiveOnlyMigrations.Tests.ps1`.

Limitation: the check is textual; it does not catch an additive change that is behaviorally incompatible (e.g. a new NOT NULL column without a default, or a new constraint legacy writers violate). Reviewers must check those.

## Rollback notes

- **Roll back hosts, not schema.** Expand-only changes are safe to leave in place; redeploy the previous host build (legacy or Azure) against the migrated DB.
- Do **not** run `Down()` against the shared DB while any host may use the new objects; `Down()` is destructive (drops columns/tables) and may lose data. Use it only on dev/test or after confirming no host reads/writes the objects.
- If a migration itself fails midway, EF6 runs each migration in a transaction; fix forward with a new additive migration. `__MigrationHistory` must reflect reality before retrying.
- Before any contract step, take a database backup/point-in-time restore marker and confirm via telemetry that no host touches the old shape.
- Rolling back a dual-write feature: turn off the flag first, then (separately) stop backfills; leave the new columns.
