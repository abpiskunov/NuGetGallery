# SDK-style conversion (phase 1)

- Converted src/NuGetGallery/NuGetGallery.csproj to SDK-style (TargetFramework net472; Microsoft.NET.Sdk.Web).
- Preserved Directory.Packages.props; no per-project versions added.
- Removed legacy root packages.config (MicroBuild.Core, xunit.runner.console) as unused.
- NuGetCDNRedirect.csproj remains legacy pending VS Web targets; converter could not load project in this environment.

Build guidance:
- Build web apps with msbuild.exe (requires ASP.NET/Web workload). Tests via dotnet test.

Rollback: revert csproj and restore packages.config from history.

## Validation (2026-11-18)`n- Repo scan: no packages.config present; all projects SDK-style.`n- No PackageReference Version= entries found; central versions honored.`n- dotnet build NuGetGallery.sln succeeded.`n
