# NuGetGallery .NET upgrade investigation

Requested outcome: Upgrade the NuGetGallery solution to .NET 10 while keeping builds and tests green.

Scope inspected: abpiskunov/NuGetGallery (5 solutions, 143 projects).

Summary of findings
- Central package management: Directory.Packages.props with ManagePackageVersionsCentrally=true.
- Mixed project types:
  - Web frontend NuGetGallery.csproj: legacy non–SDK-style, TargetFrameworkVersion=v4.7.2, System.Web, OWIN (MVC5/WebApi2).
  - Many libraries: SDK-style, multi-target net472;netstandard2.1; several already net6.0/net8.0/net10.0.
- OWIN packages present (Microsoft.Owin.*), EF6 (EntityFramework 6.5.1), System.Web references across net472 TFMs.
- global.json pins SDK 8.0 with rollForward=latestMajor.

Evidence boundary
- Declaration-only reads of csproj/props/targets; evaluated fact tool (get_dotnet_upgrade_options) reports “every project can upgrade to net11.0 (recommended)”. Given System.Web/OWIN presence, that output likely over-approximates feasibility for the web app. Treat as advisory; requires evaluated build + compatibility checks.

Implications
- Upgrading the ASP.NET MVC5 (System.Web) app to .NET 10 requires migrating to ASP.NET Core. This is a substantial rewrite: pipeline (OWIN→ASP.NET Core middleware), auth (Owin cookie/OpenID → ASP.NET Core auth), MVC routing/filters, WebApi2 controllers, config (web.config → appsettings), bundling, and hosting (IIS in-process/Kestrel). EF6 code-first usages should be migrated to EF Core or isolated behind adapters.
- Many libraries can move to net10.0 independently once consumers are ready or with multi-targeting.

Proposed staged plan (delivery partitions)
1. Baseline the repo: ensure current default build and tests pass (msbuild + dotnet) and record.
2. Convert remaining legacy projects (where feasible) to SDK-style while staying on net472; keep behavior identical. Add Directory.Build.props/targets as needed.
3. Lift library TFMs:
   - Multi-target libraries to include net8.0/net10.0 where code is portable.
   - Remove System.Web-only code from netstandard/net10 targets via conditional compilation.
4. EF modernization:
   - Identify EF6 usage surfaces; migrate to EF Core in libraries first where possible; otherwise abstract EF6 behind interfaces.
5. OWIN to ASP.NET Core bridge:
   - Introduce an ASP.NET Core host (YARP strangler optional) alongside the existing app; begin moving endpoints to Core.
   - Alternatively, a big-bang port of NuGetGallery to ASP.NET Core MVC, preserving routes and behaviors.
6. Replace System.Web-only packages with ASP.NET Core equivalents; move configuration to appsettings.* and user secrets.
7. CI/CD updates: add net10 SDK matrix, ensure global.json and SDK availability; add ARM64 if desired.
8. Remove net472 TFMs once Core port is complete and consumers are on net10.

Next bounded investigations
- Evaluate build graph per solution with msbuild/dotnet to enumerate non-SDK projects and System.Web consumers (configurations AnyCPU/Debug/Release) and collect precise TFMs.
- Identify projects already on net6+/net10 and their dependencies.
- Enumerate OWIN and EF6 consumers and migration complexity (auth, middleware, filters, WebApi2).
- Decide strangler vs direct port strategy with stakeholders.

Open questions
- Preferred migration strategy for the web app (strangler proxy vs direct port)?
- EF6 to EF Core tolerance for behavior changes; acceptable phased schema changes?
- Minimum supported runtime (net8 vs net10) for interim phases?

Next action
- Run a targeted evaluated assessment (dotnet-tools assess_dotnet_projects) on the NuGetGallery.sln and core libraries to confirm TFMs and blockers, then propose the first PR: SDK-style conversion and library TFM lift where safe.
