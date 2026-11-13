# .NET 10 upgrade assessment (synthesis)

Repository: abpiskunov/NuGetGallery, commit `29a88885c07a2b30ef71ce9f010a0913a04e9d9d`.
Target: .NET 10 (LTS). Scope: all .NET projects in the repository, no exclusions (user-approved).
No code was changed by this synthesis.

## 1. Coverage check

The graph holds 143 distinct project nodes, all `evaluation=evaluated`, `platform=dotnet`, `buildSystem=msbuild`. Sorting them by relative path and splitting at the scan boundaries gives exactly the planned partition, with no overlap and no gap:

| Scan | Task | Range (sorted) | Projects | Current assessment | Notes |
|---|---|---|---|---|---|
| A | tsk_01M79K7FNNJM33J87Y97EB77Z4 | `src/AccountDeleter` .. `src/NuGet.Services.Validation.Orchestrator` | 48 | assessment_01M79QPPR4SNR060ABP0KCC0NA (completed); 46 projects were published earlier under superseded assessment_01M79MN5XAKF55RNP9KSM1F86C, DatabaseMigrationTools and GalleryTools last | all 48 published |
| B | tsk_01M79K7FNNG2Y677YWQAEQFSYT | `src/NuGet.Services.Validation` .. `tests/NuGet.Services.Configuration.Tests` | 48 | assessment_01M79MG0CP4Y51GNQ4Q1JAQZXY (completed) | 45 analyzer-published; 3 read directly (section 4) |
| C | tsk_01M79K7FNN54SHR61WZ1DWYRBA | `tests/NuGet.Services.Contracts.Tests` .. `tools/AzureSqlConnectionTest` | 47 | assessment_01M7A0EK8QM26S3CWNKVG6AMA7 (completed, 47 analyzed in 6 batches, 0 failed) | includes `tools/AzureSqlConnectionTest` |

Total 48 + 48 + 47 = 143 = graph project count (src 80, tests 62, tools 1). **Uncovered projects: none.** No project is assigned to two scans. No rescan was run.

Caveats:
- Each completed assessment still carries the analyzer's generic "incomplete inventory/findings coverage; absence is not resolution" per-project warnings. They are advisory, not failure statuses.
- Earlier attempts (timeouts, MCP -32001, on 47-48 project lists) are failed or superseded. Only the assessments above count.
- `src/Stats.Warehouse/Stats.Warehouse.sqlproj` is an SSDT SQL project. It is not a csproj, is not among the 143 and needs no retarget.

## 2. Findings summary

Graph findings (all severity `potential`, target `net10.0`; counts include rows from superseded assessment_01M79MN5XAKF55RNP9KSM1F86C, so treat them as upper bounds):

- 135 x Project.0002 "target framework(s) needs to be changed" (analyzer severity mandatory), one per net472-targeting project.
- 64 x Api.0003 behavioral change (mostly "URI length limits removed" x61, .NET 9 obsoletions x5).
- 39 x Api.0002 source incompatible (mainly `System.Data.SqlClient` 4.8.6 reference needed x21; `Microsoft.AspNetCore.SystemWebAdapters` 1.4.0 x5; obsolete X509Certificate/X509Certificate2 constructors x4).
- 7 x Api.0001 binary incompatible.
- 34 x NuGet.0002 package upgrade recommended; 3 x NuGet.0003 package functionality in framework; 2 x NuGet.0001 package incompatible.

## 3. Target frameworks today (from project files)

| TargetFramework(s) | Projects |
|---|---|
| `net472` | 106 |
| `net472;netstandard2.0` | 16 |
| `net472;netstandard2.1` | 11 |
| `net10.0` | 5 (already on target; includes the Aspire AppHost projects) |
| `net8.0` | 2 |
| `net6.0` | 1 |
| no TFM property | 2 (the two non-SDK web projects, net472 via `TargetFrameworkVersion` v4.7.2) |

Roles: 81 application and 62 test projects.

## 4. Evaluated build facts

- `global.json` pins SDK `8.0.318`, `rollForward: latestMajor`, no prerelease. Update it for a .NET 10 SDK.
- No `Directory.Build.props`. Root `Directory.Build.targets` carries package-ownership metadata (authors, license, symbols). Root `Directory.Packages.props` enables central package management (`ManagePackageVersionsCentrally`, `EnablePackageVersionOverride`), so package bumps are made in one file.
- Conditional `ItemGroup`s on `$(TargetFramework)` (`net472` vs `netstandard2.x`) are used in the multi-target libraries (for example `NuGetGallery.Services` references `Microsoft.AspNet.Mvc` only for `net472`).
- Project format: 141 of 143 are SDK-style. The two non-SDK projects are `src/NuGetGallery/NuGetGallery.csproj` and `src/NuGetCDNRedirect/NuGetCDNRedirect.csproj`; both use PackageReference. The only `packages.config` is at the repository root and belongs to no project.
- No cloud service (`.ccproj`, `.cscfg`) projects.
- ASP.NET / web hosts: `NuGetGallery` (classic ASP.NET MVC on System.Web, IIS Express), `NuGetCDNRedirect` (classic ASP.NET). Other web-flavoured projects (`Microsoft.PackageManagement.Search.Web` net6.0, one `Microsoft.NET.Sdk.Web` net8.0 project) are already ASP.NET Core. `NuGetGallery.AppHost` uses `Aspire.AppHost.Sdk/13.5.3` and is already net10.0.
- The analyzer could not load three scan-B projects (assessed from direct reads, recorded in tsk_01M79K7FNNG2Y677YWQAEQFSYT test_report v4): `NuGetGallery`, `NuGetCDNRedirect`, `tests/GitHubVulnerabilities2Db.Facts` (blocked on NuGetGallery). Their graph nodes exist and are evaluated.

## 5. Dependency structure

Project-reference layering among the 143 projects has 13 levels (L0..L12); counts per level: 14/20/13/6/2/4/18/18/15/16/10/6/1. Most depended on: `NuGet.Jobs.Common` (23 dependents), `NuGet.Services.Contracts` (12), `NuGet.Services.Storage` (11), `Validation.Common.Job` (10), `tests/TestUtil` (9), `tests/Validation.PackageSigning.Helpers` (9), `NuGet.Services.KeyVault` (8), `NuGet.Services.ServiceBus` (8).

`NuGetGallery` references 16 projects (`NuGet.Services.{Configuration,Contracts,Entities,FeatureFlags,KeyVault,Licenses,Logging,Messaging.Email,Messaging,Owin,ServiceBus,Sql,Validation.Issues,Validation}`, `NuGetGallery.Core`, `NuGetGallery.Services`). `GalleryTools`, `DatabaseMigrationTools`, `tests/NuGetGallery.Facts` and (transitively) `tests/GitHubVulnerabilities2Db.Facts` depend on it and cannot leave net472 before the host does.

## 6. Blockers and unresolved decisions

1. **NuGetGallery host** (ASP.NET MVC / System.Web / OWIN, non-SDK). It needs SDK-style conversion (`convert_dotnet_project_to_sdk_style`) first and a strategy decision: port to ASP.NET Core (with `Microsoft.AspNetCore.SystemWebAdapters` as a bridge) versus keeping the host on net472. Its dependents (section 5) are blocked on it.
2. **NuGetCDNRedirect**: non-SDK classic ASP.NET, no references, no dependents. Independent, but needs the same conversion and hosting decision.
3. **Data access**: `System.Data.SqlClient` is required in 21 places. Decide between keeping it and moving to `Microsoft.Data.SqlClient`; check Entity Framework package compatibility for `NuGet.Services.Entities` and dependents.
4. **Legacy OWIN/MVC packages** in the central package list (Autofac.Mvc5, Autofac.Owin and similar) need net10 version research.
5. **Library targeting policy**: 27 libraries multi-target `net472` plus `netstandard2.0/2.1`. Decide whether to add `net10.0`, replace net472, or keep the netstandard asset.
6. **SDK pin** in `global.json` and any pipeline SDK settings must follow .NET 10.
7. Analyzer coverage is advisory (3 manually assessed projects, generic coverage warnings); a restore and build of the first wave is the real proof.

## 7. Proposed delivery waves

Clusters are not delivery waves. These waves follow dependency layering and risk; each wave Stage refines its own membership.

1. **Wave 0 - decision spikes (findings only)**: NuGetGallery hosting strategy, SqlClient/EF package direction, library multi-target policy.
2. **Wave 1 - already-modern and leaf libraries (L0-L1)**: the 5 net10.0, 2 net8.0 and 1 net6.0 projects plus leaf libraries (`NuGet.Services.Contracts`, `Storage`, `KeyVault`, `Entities`, `Configuration`, `Logging`, `Sql`, `Cursor`, `ServiceBus`) and their tests. Update `global.json` and central package versions here.
3. **Wave 2 - shared libraries and test helpers (L2-L5)**: `NuGetGallery.Core`, prerequisites of `NuGet.Jobs.Common`, `tests/TestUtil`, `Validation.PackageSigning.Helpers`.
4. **Wave 3 - job and validation executables (L6-L11)**: the `src` Exe projects and their tests (Jobs, Stats.*, Validation.*, Catalog, AzureSearch, StatusAggregator), grouped by shared dependency (`NuGet.Jobs.Common`, `Validation.Common.Job`).
5. **Wave 4 - NuGetGallery host and dependents**: SDK conversion, host retarget per the spike result, then `GalleryTools`, `DatabaseMigrationTools`, `tests/NuGetGallery.Facts`, `tests/GitHubVulnerabilities2Db.Facts`, `NuGetCDNRedirect` and the functional tests.
6. **Final validation Stage**: depends on every code-changing wave; restore, build and test all 143 projects.

Soft-checkpoint signals present for planning: more than one wave, more than 50 projects, ASP.NET host in scope, blockers reported.
