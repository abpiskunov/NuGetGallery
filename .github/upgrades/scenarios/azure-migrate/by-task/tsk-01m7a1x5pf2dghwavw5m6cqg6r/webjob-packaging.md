# WebJob packaging for net472 JobRunner jobs

Shared target: `build/WebJob.targets` (imported from `Directory.Build.targets` when a project sets `WebJobType`). No TargetFramework changes.

Build one job: `msbuild src/<Job>/<Job>.csproj -t:PackageWebJob -p:Configuration=Release -restore` -> `src/<Job>/bin/webjob/<Job>.zip` (job binaries + `run.cmd` + `settings.job` at the zip root).

## Mapping (31 jobs)
- `JobRunner.Run` (loops with sleep) -> `Continuous` (21), `settings.job` has `is_singleton: true`.
- `JobRunner.RunOnce` -> `Triggered` (10): DatabaseMigrationTools, Db2AzureSearch, GitHubIndexer, Services.Revalidate, Validation.Orchestrator, Stats.CDNLogsSanitizer, UpdateBlobProperties, PackageSigning.ProcessSignature, PackageSigning.ValidateCertificate, Validation.Symbols. Optional `<WebJobSchedule>` (NCRONTAB) sets `schedule`.
- Every other JobRunner job (including ArchivePackages) is Continuous.

## Duplicate-execution guard
`run.cmd` always passes `-JobGuardStorageConnectionString "%JOB_GUARD_STORAGE%"` (see `job-guard.md` in the sibling task folder) and exits 1 if the app setting is missing (fail closed). Other args come from the `JOB_ARGS` app setting. Provision guard blobs before activating the WebJob.

## Limitations
- Triggered schedules are not set; take them from each job's existing deployment script when cutting over. The Run/RunOnce mapping was not cross-checked against those schedules.
- A guard "not claimed" exit code of 0 makes a Continuous WebJob restart; WebJobs applies restart back-off.
- Not run on Azure; only packaging was verified (31/31 zips built with VS 18 Canary MSBuild).
