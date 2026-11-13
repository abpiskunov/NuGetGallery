# Cutover and rollback runbook: legacy host to Azure App Service (Windows)

Scope: the net472 `src/NuGetGallery` web app and the 31 net472 JobRunner jobs, moving from the legacy host to App Service (Windows) and Windows WebJobs. No retarget. The legacy and Azure hosts share one SQL database and one set of storage accounts during the overlap.

## Inputs (prior work, merged to main)

| Topic | Source |
|---|---|
| Assessment: 143/143 units observed, target `Any`; analyzer coverage is partial; 22 landing proposals (21 AppService.Windows, 1 AppService.Linux) | Synthesis task `tsk_01M79K7D9SMHDMEKDY4E3MHWXV` (its doc `by-task/tsk-01m79k7d9smhdmekdy4e3mhwxv/assessment-synthesis.md` was not present in this checkout; figures come from the task's artifact) |
| Web app config, settings list, health/warmup | `docs/AppServiceSettings.md`, `src/NuGetGallery/Web.AppService.config` (PR #4) |
| WebJob packaging (`PackageWebJob`, Continuous vs Triggered) | `by-task/tsk-01m7a1x5pf2dghwavw5m6cqg6r/webjob-packaging.md` (PR #8) |
| Duplicate-execution guard (blob lease) | `by-task/tsk-01m7a1x5pfha7rh9ay1570gzkm/job-guard.md` (PR #6) |
| Additive-only schema policy, CI gate, rollback notes | `docs/shared-database-schema-policy.md`, `build/Test-AdditiveOnlyMigrations.ps1` (PR #5) |

## Known gaps to close before starting (from the assessment synthesis)

- The web app and about 11 net472 job executables have no landing proposal (c02 enrichment gap). App Service (Windows) is assumed from the other 21 proposals and the net472/System.Web evidence; the owner must confirm it before Step 0 is signed off.
- c06's publication key and enrichment receipt are not evidenced.
- Nothing here was exercised on Azure. Treat every step as unverified until run in staging.
- The Run/RunOnce mapping and the Triggered schedules were not cross-checked against the existing job deployment scripts.
- The guard was not tested with Azurite across two processes.

## Roles and decision points

Cutover lead (go/no-go), DB owner (migrations), job owner (guard and WebJobs), on-call (monitoring). Each step names a **Go criterion**. If it is not met, do not proceed; use the rollback section.

## Step 0: Pre-cutover readiness (days before)

1. CI is green on main, including `migration-additive-check`. Run `pwsh build/Test-AdditiveOnlyMigrations.ps1`.
2. The migrator (DatabaseMigrationTools) has been run and the database is at the schema the Azure build expects. It is expand-only, so the legacy host must still work against it. Hosts never auto-migrate.
3. Take a database backup or note a point-in-time restore marker.
4. Provision App Service (Windows, Always On, a staging slot), storage, Key Vault references and Application Insights. Set every setting in `docs/AppServiceSettings.md`, including the **same machine key** as the legacy host (`Gallery.MachineKeyDecryptionKey` / `Gallery.MachineKeyValidationKey`) so cookies and tokens are valid on both hosts.
5. Set connection strings by name (`Gallery.SqlServer`, `Gallery.SupportRequestSqlServer`, `Gallery.ValidationSqlServer`) to the **same** databases as legacy. `Web.AppService.config` fails fast if they are missing.
6. Deploy the web app to the staging slot. Warmup path is `/api/health-probe`; set `WEBSITE_SWAP_WARMUP_PING_PATH=/api/health-probe`.
7. Lower the DNS TTL on the gallery hostname to 60 s at least one old-TTL period before cutover. Record the legacy origin and the Azure hostname.
8. Confirm how the CDN or load balancer origin is switched.

**Go criterion:** the slot's `/api/health-probe` returns 200, and smoke tests (browse, search, package download, sign-in, push to a test feed) pass on the slot URL with production dependencies, read-only where possible.

## Step 1: Job guard on the legacy side (before any Azure job runs)

Per `job-guard.md`, the guard is opt-in via `-JobGuardStorageConnectionString` and uses one blob per job at `{container}/background-jobs/{assemblyName}`.

1. Provision a guard blob per job, **inactive** (`nextdueutc` far in the future).
2. Roll the guard-enabled JobRunner build to **every** legacy copy of each job, with the same guard storage connection string and container.
3. Only after every legacy copy runs with the guard, activate the blobs (set a real `nextdueutc` and `intervalseconds`).
4. Gallery WebBackgrounder jobs (Lucene indexing, `CloudDownloadCountServiceRefreshJob`, `PackageVulnerabilitiesCacheRefreshJob`) are process-local. They run on every instance of both hosts by design and are not guarded, so they need no handover.

**Go criterion:** every legacy job logs a guard claim, and exactly one holder per job blob is observed for at least two intervals.

## Step 2: Deploy WebJobs, idle

1. Build each zip with `msbuild src/<Job>/<Job>.csproj -t:PackageWebJob -p:Configuration=Release -restore` and deploy it to App Service. Continuous jobs (21) carry `is_singleton: true`; Triggered jobs (10) have no schedule until you add one.
2. Set the `JOB_GUARD_STORAGE` and `JOB_ARGS` app settings. `run.cmd` exits 1 if `JOB_GUARD_STORAGE` is missing (fail closed).
3. Copy each Triggered job's schedule from its existing deployment script into `settings.job` / `<WebJobSchedule>` and review by hand.
4. Slot jobs also run on the staging slot; stop any you do not want running.

While legacy holds the lease, the Azure copies are "not claimed" and exit 0. A Continuous WebJob restarts on exit 0 with back-off; this is expected noise, not failure.

**Go criterion:** no job runs twice. For each job blob there is exactly one holder, `generation` increases monotonically, and no duplicate side effects appear (effects are idempotent on `JobOccurrence.DueUtc`) over at least two intervals.

## Step 3: Web traffic switch

1. Announce a window. Freeze non-essential deployments and schema changes.
2. Option A (preferred), gradual: weight traffic with Azure Front Door or Traffic Manager, 5%, 25%, 50%, then 100%, holding 15 minutes at each step.
   Option B, DNS: repoint the hostname to App Service (TTL already lowered). Option C, CDN: change the origin.
3. If using slots, swap staging to production. Confirm warmup completes before the swap finishes.
4. Keep the legacy web hosts running and on the same build version throughout.

**Checks at each step:** HTTP 5xx rate, p95 latency, `/api/health-probe`, sign-in success, package push/restore success, search results from the Azure instance, SQL connection/timeout errors, Application Insights exceptions. Compare against the legacy baseline.

**Go criterion:** error rate and latency within 10% of baseline, and no auth or cookie failures across hosts.

## Step 4: Job handover through the guard

Start only after the web app has been stable at 100% (suggested: 24 hours). For each job, one at a time, least critical first:

1. Stop **all** legacy copies of the job. The legacy holder releases, or its lease expires (15-60 s; default 60).
2. The Azure WebJob claims the lease on its next attempt. Verify the holder, that `generation` incremented, and that the job's outputs resume.
3. Leave the legacy job stopped but installed.
4. Triggered jobs: disable the legacy schedule, then enable the Azure schedule. Never both. The guard prevents concurrent execution but not a missed occurrence; check the next `nextdueutc`.

**Go criterion:** each job has produced at least one successful occurrence on Azure, with no gap longer than twice its interval.

## Step 5: Stabilise

- Keep legacy installed but stopped for the agreed soak period (suggested: 2 weeks).
- Run no contract (destructive) schema step during the soak. Per policy, contract only after all legacy hosts are decommissioned, with a fresh backup and telemetry showing no host touches the old shape.
- Restore the normal DNS TTL only after the soak.

## Rollback

General rule: **roll back hosts, not schema.** Expand-only changes stay in place, and the previous host build works against the migrated database. Never run `Down()` on the shared database.

### Web traffic

1. Trigger: sustained 5xx or auth failures above threshold, signs of data corruption, or a failed Step 3 go criterion.
2. Action: set the Azure weight to 0% (Option A), repoint DNS or origin to legacy (B/C), or swap the slot back. Legacy is still running and current, so this takes minutes (bounded by TTL, 60 s if lowered).
3. Verify legacy health-probe, sign-in and push/restore. Leave Azure running for diagnosis.

### Jobs

1. Stop the Azure WebJob(s) for the affected job.
2. Restart the legacy copies. They claim the lease when it expires (up to the lease length), or immediately if the holder released it. Verify the holder and `generation`.
3. For Triggered jobs, disable the Azure schedule, then re-enable the legacy one. Check for a missed or repeated occurrence and reconcile idempotently.
4. Because every copy on both hosts uses the guard, an overlap during rollback cannot run the same occurrence concurrently. If the guard store is unreachable, jobs fail closed (exit 1). Fix storage connectivity; do **not** disable the guard to force a run while both hosts are live.
5. Web and jobs roll back independently; start with whichever shows the symptoms.

### Schema compatibility

- Both hosts must work with every migration applied so far, which the additive-only check and CI gate enforce.
- If a migration fails midway, EF6 has run each in a transaction. Fix forward with a new additive migration, and make sure `__MigrationHistory` reflects reality before retrying.
- The textual check cannot catch a NOT NULL column without a default, or a new constraint that legacy writers violate. If one breaks the legacy host, fix forward additively (add a default, relax the constraint). A backup restore is the last resort and loses data written since.
- Dual-write features: turn the flag off first, then stop the backfill separately, and leave the new columns.

### Decision matrix

| Symptom | First action |
|---|---|
| Azure web errors or latency | Shift traffic back to legacy |
| Job duplicates observed | Stop the Azure copy; check the lease holder and `generation`; confirm every legacy copy runs the guard build |
| Jobs not running anywhere | Check guard store reachability (fail closed), the blob's `nextdueutc` and `retryafterutc` |
| DB error after migration | Fix forward with an additive migration; roll hosts back if needed |
| Auth/cookie errors across hosts | Compare the machine key and `Auth.*` settings on both hosts |

## Exit criteria

- Azure serves 100% of traffic for the soak period with error rates at or below the legacy baseline.
- Every job runs on Azure with one holder per guard blob and no legacy copy running.
- No contract schema step has been run; legacy decommission is recorded as the trigger for the contract release.

## Limitations

- Procedure only; nothing here was executed on Azure.
- Thresholds, windows and soak durations are suggestions for the owners to confirm.
- The traffic-switch mechanism (Front Door, Traffic Manager, DNS or CDN origin) depends on how production fronts the app, which the repository does not show.
- The SQL schedule-row guard variant was not implemented; only the blob lease exists.
