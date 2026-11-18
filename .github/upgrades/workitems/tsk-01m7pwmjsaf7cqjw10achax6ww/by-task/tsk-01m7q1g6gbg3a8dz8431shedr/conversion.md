# SDK-style conversion and PackageReference migration (no behavior change)

Summary
- All legacy projects converted to SDK-style; NuGetGallery (System.Web) and NuGetCDNRedirect remain on net472.
- Removed packages.config across repo; all packages use PackageReference with central versions in Directory.Packages.props.
- Web app content/resx/razor explicitly included as needed; ApplicationInsights.config and transforms preserved; WPP copy enabled.

Build
- Solution: dotnet build NuGetGallery.sln (validated OK) or Visual Studio msbuild.exe /restore.
- Tests: dotnet test; note LocalDB required for DatabaseMigration.Facts.

Notes
- Keep global.json unchanged.
- No functional changes intended.

Rollback
- Revert this folder’s commit range; project files are self-contained.
