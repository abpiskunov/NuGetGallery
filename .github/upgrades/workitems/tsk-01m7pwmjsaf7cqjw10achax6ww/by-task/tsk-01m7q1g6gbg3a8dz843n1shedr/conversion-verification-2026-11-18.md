Conversion verification (2026-11-18)

Summary
- All projects are SDK-style (<Project Sdk=...> found in 143 project files)
- No legacy MSBuild markers (no ToolsVersion, no Microsoft.CSharp.targets, no 2003 XML namespace)
- No packages.config files present
- Central package management: Directory.Packages.props present at repo root
- Build: dotnet build NuGetGallery.sln succeeded (exit code 0); NU1507 warning noted

Commands executed
- dotnet build abpiskunov-NuGetGallery/NuGetGallery.sln -v:m

Notes
- Classic System.Web app remains on net472 as required; behavior validation to be covered by smoke tests
