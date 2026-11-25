# .NET upgrade assessment (synthesis): abpiskunov/NuGetGallery → net10.0

Target: **net10.0 (.NET 10 LTS)**, user-confirmed. Scope: **all 143 .NET projects**, no exclusions. Synthesis only: no scan was re-run and no code was changed.
Scanned commit = checkout HEAD = `8415738427c9768a275e8e32c9c583fd08c668fe` (the repository revision in the graph is the same), so no scanned project is stale. This document replaces an earlier assessment.md that cited a different commit and different scan records.

## Coverage

All 143 projects in the graph are covered, each by exactly one completed scan assessment (`assessment_by_unit`: `complete = true` for every unit; no overlaps, no gaps).

| Scan | Scan task | Covering assessment record | Projects |
|---|---|---|---|
| 1 | tsk_01M879ZVGN40H4APYD53V16P3P | `assessment_01M87A0VEHHHCMDQVX30HJWQZT` | 25 |
| 2 | tsk_01M879ZVGN3XY511BW33VXBFVY | `assessment_01M87A0TXE70W48MKXDZ5SVSFP` | 25 |
| 3 | tsk_01M879ZVGNA03T9ABV342AKJCN | `assessment_01M87VT9702NDGBSMYHREVXYA2` | 25 |
| 4 | tsk_01M879ZVGN1SKXRQFCN5RW17GZ | `assessment_01M87A0JJP66YFYBFF99N0C8Q5` | 25 |
| 5 | tsk_01M879ZVGN1YZDQ5RXRW0P5XZW | `assessment_01M87A0NBH608RQRRPHKZDKFG5` | 25 |
| 6 | tsk_01M879ZVGNCMNSZKDY48FEGZDF | `assessment_01M87A70CW7EBX2420273YDSQ5` | 18 |

Total: 143.

Limitations:

- Scan 3 has an earlier failed record, `assessment_01M87A6A1YWQ4VARJM7KTQJ0MC` (nothing published). It covers nothing; the rerun `…VT97…` supersedes it.
- Scan 6 has **no receipt note** (`scan <taskId>:`), so its scanned commit is not recorded. Its record finished from the same graph revision and the repository revision equals HEAD, so it is treated as covered and not stale. Validation should confirm this.
- Each record carries the analyzer's "incomplete inventory/findings coverage; absence is not resolution" warning. That is the normal partial claim, not a gap, but the absence of a finding is not proof of compatibility.
- Only the project-level analyzer ran. Package-upgrade and ARM64 assessments were not run.
- Reused assessments: none. Excluded projects: none.

## Project inventory (143)

| Target framework today | Projects |
|---|---|
| net472 (SDK-style) | 106 |
| net472 + netstandard2.0 | 16 |
| net472 + netstandard2.1 | 11 |
| net472, legacy non-SDK csproj (NuGetGallery, NuGetCDNRedirect) | 2 |
| net10.0 | 5 (NuGetGallery.AppHost, .AppHost.Config, .AppHost.Tools, NuGetGallery.FunctionalTests, .FunctionalTests.Core) |
| net8.0 | 2 (NuGet.Services.SearchService.Core, its Tests) |
| net6.0 | 1 (Microsoft.PackageManagement.Search.Web) |

81 application projects (39 executables) and 62 test projects. 308 findings in total (163 required, 144 potential, 1 optional). 138 projects carry `Project.0002` (target framework must change); the five projects already on net10.0 carry none.

## Resolved projects

`resolved: [{project, reason}]`: **every one of the 143 projects has reason `picked`** (scope is the whole repository). Dependencies were resolved in both directions from the project reference edges (each project's `ProjectReference` items, the source of the graph's `references` edges). Every reference target is itself one of the 143 projects; **no referenced project lies outside the picked set**, so nothing is added from outside. The "Uses" (uses Y) and "Dependency of" (dependency of X) columns record the relationships that drive ordering, truncated to two names plus a count.

Columns: Req/Pot = required/potential findings; Scan = covering scan (1-6).

|Project|Area|TFM today|Req/Pot|Scan|Uses|Dependency of|
|---|---|---|---|---|---|---|
|AccountDeleter|src|net472|1/1|1|NuGetGallery.Services, Validation.Common.Job|AccountDeleter.Facts|
|ArchivePackages|src|net472|1/1|1|NuGet.Jobs.Common|-|
|NuGet.Services.Metadata.Catalog|src|net472;netstandard2.1|1/2|1|NuGet.Protocol.Catalog, NuGet.Services.Configuration +4|NuGet.Jobs.Catalog2AzureSearch, NuGet.Services.Metadata.Catalog.Monitoring +2|
|CopyAzureContainer|src|net472|2/1|1|NuGet.Jobs.Common|-|
|DatabaseMigrationTools|src|net472|1/2|1|NuGet.Services.CatalogValidation, NuGet.Services.DatabaseMigration +2|NuGet.Services.DatabaseMigration.Facts|
|Gallery.CredentialExpiration|src|net472|1/1|1|NuGet.Jobs.Common, NuGet.Services.Storage +1|Tests.CredentialExpiration|
|Gallery.Maintenance|src|net472|1/1|1|NuGet.Jobs.Common|Tests.Gallery.Maintenance|
|GalleryTools|src|net472|1/2|1|GitHubVulnerabilities2Db, NuGetGallery|-|
|GitHubVulnerabilities2Db|src|net472|1/1|1|NuGet.Jobs.Common, NuGet.Services.Cursor +2|GalleryTools, GitHubVulnerabilities2Db.Facts +1|
|GitHubVulnerabilities2V3|src|net472|1/1|1|NuGet.Jobs.Common, NuGet.Services.Cursor +2|GitHubVulnerabilities2v3.Facts|
|Microsoft.PackageManagement.Search.Web|src|net6.0|1/1|1|NuGet.Services.AzureSearch|NuGet.Services.SearchService.Core|
|Ng|src|net472|1/2|1|NuGet.Jobs.Catalog2Registration, NuGet.Services.Metadata.Catalog.Monitoring +1|NgTests, NuGetGallery.AppHost|
|NuGet.Jobs.Auxiliary2AzureSearch|src|net472|1/0|1|NuGet.Jobs.Common, NuGet.Services.AzureSearch|NuGetGallery.AppHost|
|NuGet.Jobs.Catalog2AzureSearch|src|net472|1/0|1|NuGet.Services.AzureSearch, NuGet.Services.Metadata.Catalog|NuGetGallery.AppHost|
|NuGet.Jobs.Catalog2Registration|src|net472|1/2|1|NuGet.Jobs.Common, NuGet.Services.V3|Ng, NuGet.Jobs.Catalog2Registration.Tests +1|
|NuGet.Jobs.Common|src|net472;netstandard2.1|1/3|1|NuGet.Services.Configuration, NuGet.Services.FeatureFlags +5|ArchivePackages, CopyAzureContainer +21|
|NuGet.Jobs.Db2AzureSearch|src|net472|1/0|1|NuGet.Jobs.Common, NuGet.Services.AzureSearch|NuGetGallery.AppHost|
|NuGet.Jobs.GitHubIndexer|src|net472|2/0|1|NuGet.Jobs.Common|NuGet.Jobs.GitHubIndexer.Tests|
|NuGet.Protocol.Catalog|src|net472;netstandard2.0|1/1|1|-|Monitoring.PackageLag, NuGet.Protocol.Catalog.Tests +1|
|NuGet.Services.AzureSearch|src|net472;netstandard2.1|1/3|1|NuGet.Services.V3|Microsoft.PackageManagement.Search.Web, NuGet.Jobs.Auxiliary2AzureSearch +4|
|NuGet.Services.Build|src|net472|2/1|1|-|NuGet.Services.Build.Tests|
|NuGet.Services.CatalogValidation|src|net472;netstandard2.1|1/1|1|NuGet.Services.Validation|DatabaseMigrationTools|
|NuGet.Services.Configuration|src|net472;netstandard2.0|1/1|1|NuGet.Services.KeyVault|NuGet.Jobs.Common, NuGet.Services.AzureSearch.FunctionalTests +5|
|NuGet.Services.Contracts|src|net472;netstandard2.0|1/0|1|-|NuGet.Services.Contracts.Tests, NuGet.Services.Logging +10|
|NuGet.Services.Cursor|src|net472|1/2|1|NuGet.Services.Storage|GitHubVulnerabilities2Db, GitHubVulnerabilities2V3 +4|
|NuGet.Services.DatabaseMigration|src|net472|1/2|2|NuGet.Jobs.Common|DatabaseMigrationTools|
|NuGet.Services.Entities|src|net472;netstandard2.1|2/2|2|-|NuGet.Services.Entities.Tests, NuGetGallery +1|
|NuGet.Services.FeatureFlags|src|net472;netstandard2.0|1/1|2|-|NuGet.Jobs.Common, NuGet.Services.FeatureFlags.Tests +2|
|NuGet.Services.GitHub|src|net472|1/1|2|NuGet.Services.Cursor, NuGetGallery.Services|GitHubVulnerabilities2Db, GitHubVulnerabilities2V3|
|NuGet.Services.Incidents|src|net472;netstandard2.0|1/3|2|-|StatusAggregator|
|NuGet.Services.KeyVault|src|net472;netstandard2.0|1/2|2|-|AzureSqlConnectionTest, NuGet.Services.Configuration +6|
|NuGet.Services.Licenses|src|net472;netstandard2.0|1/1|2|-|NuGet.Services.Licenses.Tests, NuGetGallery|
|NuGet.Services.Logging|src|net472;netstandard2.0|1/3|2|NuGet.Services.Contracts|NuGet.Jobs.Common, NuGet.Services.Logging.Tests +3|
|NuGet.Services.Messaging.Email|src|net472;netstandard2.0|1/0|2|NuGet.Services.Messaging|NuGet.Jobs.Common, NuGet.Services.Messaging.Email.Tests +2|
|NuGet.Services.Messaging|src|net472;netstandard2.0|1/1|2|NuGet.Services.Contracts, NuGet.Services.ServiceBus|NuGet.Services.Messaging.Email, NuGet.Services.Messaging.Email.Tests +2|
|NuGet.Services.Metadata.Catalog.Monitoring|src|net472|1/2|2|NuGet.Services.Metadata.Catalog, NuGet.Services.Storage|CatalogMetadataTests, Ng|
|NuGet.Services.Owin|src|net472|2/1|2|-|NuGet.Services.Owin.Tests, NuGetGallery|
|NuGet.Services.Revalidate|src|net472|1/2|2|NuGet.Services.Status, Validation.Common.Job +1|NuGet.Services.Revalidate.Tests|
|NuGet.Services.SearchService.Core|src|net8.0|2/0|2|Microsoft.PackageManagement.Search.Web|NuGet.Services.SearchService.Core.Tests, NuGetGallery.AppHost|
|NuGet.Services.ServiceBus|src|net472;netstandard2.0|1/2|2|NuGet.Services.Contracts|NuGet.Services.Messaging, NuGet.Services.Messaging.Tests +6|
|NuGet.Services.Sql|src|net472;netstandard2.0|1/2|2|NuGet.Services.KeyVault|AzureSqlConnectionTest, NuGet.Jobs.Common +3|
|NuGet.Services.Status.Table|src|net472;netstandard2.0|1/1|2|NuGet.Services.Status|StatusAggregator|
|NuGet.Services.Status|src|net472;netstandard2.0|1/1|2|-|NuGet.Services.Revalidate, NuGet.Services.Status.Table +1|
|NuGet.Services.Storage|src|net472;netstandard2.1|1/2|2|-|Gallery.CredentialExpiration, Ng +9|
|NuGet.Services.Testing.Entities|src|net472|1/0|2|NuGet.Services.Validation|Tests.ContextHelpers|
|NuGet.Services.V3|src|net472;netstandard2.1|1/1|2|NuGet.Services.Metadata.Catalog, Validation.Common.Job|NuGet.Jobs.Catalog2Registration, NuGet.Services.AzureSearch +1|
|NuGet.Services.Validation.Issues|src|net472;netstandard2.0|1/1|2|NuGet.Services.Contracts|NuGet.Services.Validation.Issues.Tests, NuGetGallery +1|
|NuGet.Services.Validation.Orchestrator|src|net472|1/2|2|Validation.Common.Job, Validation.ContentScan.Core +3|NuGet.Services.Validation.Orchestrator.Tests, Validation.ContentScan.Tests +3|
|NuGet.Services.Validation|src|net472;netstandard2.1|1/2|2|NuGet.Services.Contracts, NuGet.Services.ServiceBus|DatabaseMigrationTools, NuGet.Services.CatalogValidation +5|
|NuGet.SupportRequests.Notifications|src|net472|1/1|2|NuGet.Jobs.Common|-|
|NuGetCDNRedirect|src|net472 (legacy csproj)|8/3|3|-|-|
|NuGetGallery.AppHost.Config|src|net10.0|0/0|3|-|NuGetGallery.AppHost, NuGetGallery.AppHost.Tools|
|NuGetGallery.AppHost.Tools|src|net10.0|0/0|3|NuGetGallery.AppHost.Config|NuGetGallery.AppHost|
|NuGetGallery.AppHost|src|net10.0|0/0|3|Ng, NuGet.Jobs.Auxiliary2AzureSearch +6|NuGetGallery.FunctionalTests|
|NuGetGallery.Core|src|net472;netstandard2.1|1/3|3|NuGet.Services.Entities, NuGet.Services.FeatureFlags +3|Gallery.CredentialExpiration, GitHubVulnerabilities2V3 +5|
|NuGetGallery.Services|src|net472;netstandard2.1|2/3|3|NuGet.Services.Configuration, NuGet.Services.Logging +1|AccountDeleter, GitHubVulnerabilities2Db +3|
|NuGetGallery|src|net472 (legacy csproj)|7/4|3|NuGet.Services.Configuration, NuGet.Services.Contracts +14|DatabaseMigrationTools, GalleryTools +1|
|PackageHash|src|net472|1/1|3|NuGet.Services.Cursor, Validation.Common.Job|-|
|Monitoring.PackageLag|src|net472|1/1|3|NuGet.Jobs.Common, NuGet.Protocol.Catalog|Monitoring.PackageLag.Tests|
|SnapshotAzureBlob|src|net472|2/0|3|NuGet.Jobs.Common|-|
|SplitLargeFiles|src|net472|1/1|3|NuGet.Services.KeyVault|SplitLargeFiles.Tests|
|Stats.AggregateCdnDownloadsInGallery|src|net472|1/1|3|NuGet.Jobs.Common, NuGet.Services.AzureSearch +1|Tests.Stats.AggregateCdnDownloadsInGallery|
|Stats.AzureCdnLogs.Common|src|net472|1/2|3|NuGet.Services.Storage|Stats.CDNLogsSanitizer, Stats.CollectAzureChinaCDNLogs +2|
|Stats.CDNLogsSanitizer|src|net472|1/1|3|NuGet.Jobs.Common, Stats.AzureCdnLogs.Common|Tests.Stats.CDNLogsSanitizer|
|Stats.CollectAzureChinaCDNLogs|src|net472|1/1|3|NuGet.Jobs.Common, Stats.AzureCdnLogs.Common|Tests.Stats.CollectAzureChinaCDNLogs|
|Stats.CreateAzureCdnWarehouseReports|src|net472|1/1|3|NuGet.Jobs.Common, Stats.AzureCdnLogs.Common|-|
|Stats.PostProcessReports|src|net472|1/2|3|NuGet.Jobs.Common, NuGet.Services.Storage|Stats.PostProcessReports.Tests|
|StatusAggregator|src|net472|1/2|3|NuGet.Jobs.Common, NuGet.Services.Incidents +2|StatusAggregator.Tests|
|UpdateBlobProperties|src|net472|1/1|3|NuGet.Services.Cursor, Validation.Common.Job|-|
|Validation.Common.Job|src|net472;netstandard2.1|2/3|3|NuGet.Jobs.Common, NuGet.Services.ServiceBus +1|AccountDeleter, NuGet.Services.Revalidate +8|
|Validation.ContentScan.Core|src|net472|1/1|3|NuGet.Services.ServiceBus|NuGet.Services.Validation.Orchestrator|
|Validation.PackageSigning.Core|src|net472|1/2|3|Validation.Common.Job|NuGet.Services.Revalidate, NuGet.Services.Validation.Orchestrator +4|
|Validation.PackageSigning.ProcessSignature|src|net472|1/1|3|NuGet.Services.Validation.Orchestrator|Validation.PackageSigning.ProcessSignature.Tests|
|Validation.PackageSigning.RevalidateCertificate|src|net472|1/0|3|Validation.Common.Job, Validation.PackageSigning.Core|Validation.PackageSigning.RevalidateCertificate.Tests|
|Validation.PackageSigning.ValidateCertificate|src|net472|1/0|3|Validation.Common.Job, Validation.PackageSigning.Core|Validation.PackageSigning.ValidateCertificate.Tests|
|Validation.ScanAndSign.Core|src|net472;netstandard2.0|1/2|4|NuGet.Services.ServiceBus|NuGet.Services.Validation.Orchestrator|
|Validation.Symbols.Core|src|net472|1/0|4|Validation.Common.Job|NuGet.Services.Validation.Orchestrator, Validation.Symbols.Core.Tests +1|
|Validation.Symbols.Job|src|net472|1/2|4|NuGet.Services.Validation.Orchestrator, Validation.Symbols.Core|Validation.Symbols.Tests|
|VerifyGitHubVulnerabilities|src|net472|1/1|4|GitHubVulnerabilities2Db, NuGet.Jobs.Common|-|
|VerifyMicrosoftPackage|src|net472|1/1|4|NuGetGallery.Services|VerifyMicrosoftPackage.Facts|
|AccountDeleter.Facts|tests|net472|1/0|4|AccountDeleter, NuGetGallery.Core.Facts|-|
|BasicSearchTests.FunctionalTests.Core|tests|net472|1/3|4|-|NuGet.Services.AzureSearch.FunctionalTests|
|CatalogMetadataTests|tests|net472|1/1|4|NuGet.Services.Metadata.Catalog.Monitoring|-|
|CatalogTests|tests|net472|1/1|4|NgTests, NuGet.Services.V3.Tests|-|
|GitHubVulnerabilities2Db.Facts|tests|net472|1/0|4|GitHubVulnerabilities2Db, NuGetGallery.Core.Facts +1|-|
|GitHubVulnerabilities2v3.Facts|tests|net472|1/1|4|GitHubVulnerabilities2V3|-|
|Monitoring.PackageLag.Tests|tests|net472|1/0|4|Monitoring.PackageLag, TestUtil|-|
|NgTests|tests|net472|1/1|4|Ng|CatalogTests|
|NuGet.Jobs.Catalog2Registration.Tests|tests|net472|1/1|4|NuGet.Jobs.Catalog2Registration, NuGet.Services.V3.Tests|-|
|NuGet.Jobs.Common.Tests|tests|net472|1/0|4|NuGet.Jobs.Common, TestUtil|-|
|NuGet.Jobs.GitHubIndexer.Tests|tests|net472|1/0|4|NuGet.Jobs.GitHubIndexer|-|
|NuGet.Protocol.Catalog.Tests|tests|net472|1/0|4|NuGet.Protocol.Catalog|NuGet.Services.V3.Tests|
|NuGet.Services.AzureSearch.FunctionalTests|tests|net472|1/1|4|BasicSearchTests.FunctionalTests.Core, NuGet.Services.Configuration|-|
|NuGet.Services.AzureSearch.Tests|tests|net472|1/1|4|NuGet.Services.AzureSearch, NuGet.Services.V3.Tests|-|
|NuGet.Services.Build.Tests|tests|net472|1/0|4|NuGet.Services.Build|-|
|NuGet.Services.Configuration.Tests|tests|net472|2/1|4|NuGet.Services.Configuration, NuGet.Services.KeyVault|-|
|NuGet.Services.Contracts.Tests|tests|net472|1/0|4|NuGet.Services.Contracts|-|
|NuGet.Services.Cursor.Tests|tests|net472|1/1|4|NuGet.Services.Cursor, NuGet.Services.Storage|-|
|NuGet.Services.DatabaseMigration.Facts|tests|net472|1/1|4|DatabaseMigrationTools|-|
|NuGet.Services.Entities.Tests|tests|net472|1/0|4|NuGet.Services.Entities|-|
|NuGet.Services.FeatureFlags.Tests|tests|net472|1/0|5|NuGet.Services.FeatureFlags|-|
|NuGet.Services.KeyVault.Tests|tests|net472|1/1|5|NuGet.Services.KeyVault|-|
|NuGet.Services.Licenses.Tests|tests|net472|1/0|5|NuGet.Services.Licenses|-|
|NuGet.Services.Logging.Tests|tests|net472|1/1|5|NuGet.Services.Contracts, NuGet.Services.Logging|-|
|NuGet.Services.Messaging.Email.Tests|tests|net472|1/0|5|NuGet.Services.Messaging, NuGet.Services.Messaging.Email|-|
|NuGet.Services.Messaging.Tests|tests|net472|1/0|5|NuGet.Services.Contracts, NuGet.Services.Messaging +1|-|
|NuGet.Services.Owin.Tests|tests|net472|1/1|5|NuGet.Services.Owin|-|
|NuGet.Services.Revalidate.Tests|tests|net472|1/0|5|NuGet.Services.Revalidate, Tests.ContextHelpers|-|
|NuGet.Services.SearchService.Core.Tests|tests|net8.0|1/0|5|NuGet.Services.SearchService.Core|-|
|NuGet.Services.ServiceBus.Tests|tests|net472|1/0|5|NuGet.Services.Contracts, NuGet.Services.ServiceBus|-|
|NuGet.Services.Sql.Tests|tests|net472|1/1|5|NuGet.Services.KeyVault, NuGet.Services.Sql|-|
|NuGet.Services.Status.Tests|tests|net472|1/0|5|NuGet.Services.Status|-|
|NuGet.Services.Storage.Tests|tests|net472|1/1|5|NuGet.Services.Storage|-|
|NuGet.Services.V3.Tests|tests|net472|1/1|5|NuGet.Protocol.Catalog.Tests, NuGet.Services.V3|CatalogTests, NuGet.Jobs.Catalog2Registration.Tests +1|
|NuGet.Services.Validation.Issues.Tests|tests|net472|1/0|5|NuGet.Services.Contracts, NuGet.Services.Validation +1|-|
|NuGet.Services.Validation.Orchestrator.Tests|tests|net472|1/1|5|NuGet.Services.Validation.Orchestrator, Tests.ContextHelpers +1|-|
|NuGet.Services.Validation.Tests|tests|net472|1/2|5|NuGet.Services.Contracts, NuGet.Services.Validation|-|
|NuGetGallery.Core.Facts|tests|net472|1/2|5|NuGetGallery.Core|AccountDeleter.Facts, GitHubVulnerabilities2Db.Facts +2|
|NuGetGallery.Facts|tests|net472|2/2|5|NuGetGallery, NuGetGallery.Core.Facts|GitHubVulnerabilities2Db.Facts|
|NuGetGallery.FunctionalTests.Core|tests|net10.0|0/0|5|NuGet.Services.Configuration|NuGetGallery.FunctionalTests|
|NuGetGallery.FunctionalTests|tests|net10.0|0/0|5|NuGetGallery.AppHost, NuGetGallery.FunctionalTests.Core|-|
|SplitLargeFiles.Tests|tests|net472|1/0|5|SplitLargeFiles, Validation.Symbols.Tests|-|
|Stats.PostProcessReports.Tests|tests|net472|1/1|5|Stats.PostProcessReports|-|
|StatusAggregator.Tests|tests|net472|1/0|5|StatusAggregator|-|
|Tests.CredentialExpiration|tests|net472|1/0|5|Gallery.CredentialExpiration|-|
|Tests.Gallery.Maintenance|tests|net472|1/0|6|Gallery.Maintenance|-|
|Tests.Stats.AggregateCdnDownloadsInGallery|tests|net472|1/0|6|Stats.AggregateCdnDownloadsInGallery|-|
|Tests.Stats.AzureCdnLogs.Common|tests|net472|1/1|6|Stats.AzureCdnLogs.Common|-|
|Tests.Stats.CDNLogsSanitizer|tests|net472|1/0|6|Stats.CDNLogsSanitizer|-|
|Tests.Stats.CollectAzureChinaCDNLogs|tests|net472|1/1|6|Stats.CollectAzureChinaCDNLogs|-|
|TestUtil|tests|net472|1/0|6|Tests.ContextHelpers|Monitoring.PackageLag.Tests, NuGet.Jobs.Common.Tests +7|
|Validation.Common.Job.Tests|tests|net472|2/1|6|TestUtil|-|
|Validation.ContentScan.Tests|tests|net472|1/1|6|NuGet.Services.Validation.Orchestrator, Tests.ContextHelpers|-|
|Validation.PackageSigning.Core.Tests|tests|net472|1/3|6|Tests.ContextHelpers, TestUtil|Validation.PackageSigning.ProcessSignature.Tests, Validation.PackageSigning.ValidateCertificate.Tests|
|Tests.ContextHelpers|tests|net472|1/0|6|NuGet.Services.Testing.Entities, Validation.PackageSigning.Core|NuGet.Services.Revalidate.Tests, NuGet.Services.Validation.Orchestrator.Tests +7|
|Validation.PackageSigning.ProcessSignature.Tests|tests|net472|1/1|6|Tests.ContextHelpers, TestUtil +2|-|
|Validation.PackageSigning.RevalidateCertificate.Tests|tests|net472|1/0|6|Tests.ContextHelpers, Validation.PackageSigning.RevalidateCertificate|-|
|Validation.PackageSigning.ScanAndSign.Tests|tests|net472|1/1|6|NuGet.Services.Validation.Orchestrator, Tests.ContextHelpers|-|
|Validation.PackageSigning.ValidateCertificate.Tests|tests|net472|1/0|6|Tests.ContextHelpers, TestUtil +2|-|
|Validation.Symbols.Core.Tests|tests|net472|1/0|6|TestUtil, Validation.Symbols.Core|-|
|Validation.Symbols.Tests|tests|net472|1/0|6|TestUtil, Validation.Symbols.Job|SplitLargeFiles.Tests|
|VerifyMicrosoftPackage.Facts|tests|net472|1/1|6|NuGetGallery.Core.Facts, VerifyMicrosoftPackage|-|
|AzureSqlConnectionTest|tools|net472|1/1|6|NuGet.Services.KeyVault, NuGet.Services.Sql|-|

Edge-source caveat: the 258 project-to-project edges above come from the `ProjectReference` items in the project files; the graph reports 293 `project → project` references. The 35-edge difference was not reconciled (graph neighbour reads are capped and the lens byte budget refused a full dump). Treat the "Uses" and "Dependency of" columns as a lower bound; planning should re-read the graph edges for final wave ordering.

## Blockers (required findings beyond the retarget itself)

Every project except the five already on net10.0 has the required `Project.0002` finding (the retarget). Beyond it:

| Project | Finding | Evidence |
|---|---|---|
| NuGetGallery (ASP.NET MVC 5 host, legacy csproj) | `Api.0001` binary incompatible (750 places), `Feature.0001` System.Web.Optimization bundling unsupported, `NuGet.0001` incompatible package, `NuGet.0003` package to remove, `Binding.0006`; also 217 `Api.0002` places pointing at `Microsoft.AspNetCore.SystemWebAdapters` 1.4.0 | Scan 3, `assessment_01M87VT9702NDGBSMYHREVXYA2`. `Directory.Packages.props` pins Microsoft.AspNet.Mvc 5.2.9, Microsoft.Owin.* 4.2.2, Autofac.Mvc5/Owin/WebApi2, Microsoft.Web.Infrastructure |
| NuGetCDNRedirect (legacy csproj) | `NuGet.0001`, `NuGet.0003`, `Binding.0006`, `Api.0001` (2 places): 8 required findings | Scan 3 |
| NuGet.Services.Build, NuGet.Services.Owin | `NuGet.0001` incompatible package | Scans 1 and 2. Owin is also used by the NuGetGallery host |
| NuGetGallery.Services (3), Validation.Common.Job (1), NuGet.Jobs.GitHubIndexer (1) | `Api.0001` binary incompatible API | Scans 3 and 1. Validation.Common.Job has 10 dependents and gates the validation jobs |
| NuGet.Services.SearchService.Core | `Api.0001` (3 places): DynamicallyAccessedMembers annotation removed | Scan 2 (already net8.0) |
| CopyAzureContainer, NuGet.Services.Entities, SnapshotAzureBlob | `NuGet.0003`: package is now in the framework and must be removed | Scans 1, 2, 3 |
| Tests: NuGetGallery.Facts (2), NuGet.Services.Configuration.Tests (1), Validation.Common.Job.Tests (2) | `Api.0001` | Scans 5, 4, 6 |

Environment blocker: `global.json` pins SDK `8.0.318` (`rollForward: latestMajor`, no prerelease). A .NET 10 SDK is needed to build net10.0; planning should use `check_dotnet_global_json` and `validate_dotnet_sdk`.

## Risks

- `Api.0003` (118 places on the host): URI length limits removed; behaviour to test.
- `Api.0002` source-incompatible suggestions: `System.Data.SqlClient` 4.8.6 (or `Microsoft.Data.SqlClient`) and `SystemWebAdapters` 1.4.0 (host only).
- `NuGet.0002`: recommended package version bumps.
- 27 libraries already multi-target netstandard2.0/2.1 next to net472, so adding `net10.0` is mechanical for them. The 106 net472-only SDK-style projects need a full retarget.
- Test projects mostly carry only `Project.0002`, but they gate each wave's verification.
- Central package management (`Directory.Packages.props`), shared build props and four solutions mean one version change reaches many projects.
- EntityFramework 6.5.1 and WindowsAzure.Storage 9.3.3 are central pins. The scans did not flag them as incompatible, but their net10.0 behaviour was not assessed (to verify).

## Decisions for the upgrade planning Stage

| ID | Decision | Evidence | Recommended |
|---|---|---|---|
| D1 | How to move the NuGetGallery MVC 5 / OWIN host | ~750 API places, `Feature.0001`, `Binding.0006`; SystemWebAdapters suggested by `Api.0002` | Spike first; incremental port with SystemWebAdapters (or YARP strangler), not a big-bang rewrite; its own wave |
| D2 | Library strategy: multi-target `net472;net10.0` or retarget in place | 27 libraries multi-target; hosts and ~39 executables consume them | Multi-target shared libraries first, retarget consumers, drop net472 last |
| D3 | NuGetCDNRedirect: port or retire | Legacy csproj, 8 required findings, no dependents | Ask the owner via a spike |
| D4 | OWIN / legacy auth packages (`NuGet.Services.Owin`) | `NuGet.0001`; used by the host | Fold into D1 |
| D5 | SQL client: `System.Data.SqlClient` or `Microsoft.Data.SqlClient` | `Api.0002` findings | Microsoft.Data.SqlClient, applied centrally |
| D6 | SDK/CI: `global.json` and CI YAML to .NET 10; align the net6.0/net8.0 projects | `global.json` pins 8.0.318 | Update `global.json`; align net6.0/net8.0 to net10.0 |

Not assessed: runtime hosting of the 39 executables (Windows services, Azure deployment config) and deployment pipelines. The scans cover code and packages only.

The soft-checkpoint signals fire (more than 50 projects, an ASP.NET host, blockers), so the planning Stage should ask "Proceed with this plan?".

## Proposed wave outline (outcomes only)

0. **Foundation:** .NET 10 SDK in `global.json` and CI, central package versions that work for net472 and net10.0, a build baseline; decision spikes D1, D3, D5.
1. **Leaf and shared libraries:** Contracts, KeyVault, Storage, FeatureFlags, Status, Incidents, Build, Owin, Licenses, Protocol.Catalog, Entities with their unit tests build for net10.0.
2. **Mid-tier services and common job host:** Configuration, Logging, Messaging, ServiceBus, Sql, Validation, V3, Metadata.Catalog, AzureSearch, NuGet.Jobs.Common, Validation.Common.Job, NuGetGallery.Core/.Services, plus shared test helpers (TestUtil, Tests.ContextHelpers).
3. **Job executables and their tests:** the 39 executables (Stats, Validation, Ng, catalog and search jobs, GitHub vulnerability jobs, tools) and their test projects, grouped by shared dependency.
4. **Web host:** NuGetGallery and its Facts/functional tests per D1; NuGetCDNRedirect per D3.
5. **Final validation:** full build and test of all solutions on net10.0, no leftover net472 or legacy-csproj references, the five existing net10.0 projects still build.

## Evidence

- Coverage: `assessment_query` lens `assessment_by_unit` on each record; 143 of 143 units `complete = true`, 0 duplicates.
- Findings: graph `finding:<rule>/<project path>` nodes (308).
- Receipts: `scan <taskId>:` memory notes for scans 1-5; none for scan 6.
