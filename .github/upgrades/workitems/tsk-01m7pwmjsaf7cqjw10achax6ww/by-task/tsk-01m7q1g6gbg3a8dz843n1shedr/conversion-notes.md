# SDK-style conversion (phase 1)

- Converted src/NuGetGallery/NuGetGallery.csproj to SDK-style (TargetFramework net472; Microsoft.NET.Sdk.Web).
- Preserved Directory.Packages.props; no per-project versions added.
- Removed legacy root packages.config (MicroBuild.Core, xunit.runner.console) as unused.
- NuGetCDNRedirect.csproj remains legacy pending VS Web targets; converter could not load project in this environment.

Build guidance:
- Build web apps with msbuild.exe (requires ASP.NET/Web workload). Tests via dotnet test.

Rollback: revert csproj and restore packages.config from history.
