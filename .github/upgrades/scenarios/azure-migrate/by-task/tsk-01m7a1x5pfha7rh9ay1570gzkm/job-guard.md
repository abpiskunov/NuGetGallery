# Duplicate-execution guard for jobs on both hosts

Code: `src/NuGetGallery.Core/Infrastructure/JobCoordination` (`JobGuard`, `IJobScheduleStore`, `BlobJobScheduleStore`); wired into `JobRunner` (opt-in). Tests: `tests/NuGetGallery.Core.Facts/Infrastructure/JobCoordination/JobGuardFacts.cs`.

## Model
- Azure blob lease, one blob per job at `{container}/background-jobs/{assemblyName}`; metadata `intervalseconds`, `nextdueutc`, `generation`, optional `retryafterutc`. Times come from the storage `Date` header.
- At-least-once delivery, fixed delay (approximate). Fail closed when the store is unreachable. Renewal every lease/3; work is cancelled when renewal fails or the lease runs out unconfirmed. Effects must be idempotent on `JobOccurrence.DueUtc` or fenced on `Generation`.
- Failure policy: a failed run defers retry by `JobGuardRetrySeconds` in the store; the occurrence stays due.

## JobRunner (console jobs)
Enabled by arg `-JobGuardStorageConnectionString` (optional `-JobGuardContainer` default `job-guard`, `-JobGuardLeaseSeconds` 15-60 default 60, `-JobGuardRetrySeconds` default 60). Not claimed exits 0; store down, lost claim or failure exits 1. Provision blobs (inactive: `nextdueutc` far future) before activation; activate only after every legacy copy runs with the guard.

## Gallery WebBackgrounder jobs (AppActivator.BackgroundJobsPostStart)
Classified per the skill: indexing (local Lucene index), `CloudDownloadCountServiceRefreshJob` (in-memory cache) and `PackageVulnerabilitiesCacheRefreshJob` (in-memory cache) are process-local, so they must run on EVERY instance of both hosts and are intentionally NOT guarded (a guard would leave non-holders stale). `NuGetJobCoordinator` is unchanged. Any future shared-write Gallery job should call `JobGuard.RunAsync` from its `IJob.Execute`.

## Limitations
- Framework-side claim reuses `BlobJobScheduleStore` (net472 build); no Azurite two-process test was run.
- The SQL schedule-row variant was not implemented (needs schema-owner migration).
