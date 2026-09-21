# Bespoke net10 OData v1/v2 feed implementation

This folder holds the **net10.0** reimplementation of the legacy OData v1/v2 package feed
endpoints (`api/v1`, `api/v2`) that used to be served by `System.Web.Http.OData` (see the
excluded legacy code under `..\OData`, `..\App_Start\NuGetODataConfig.cs`,
`..\App_Start\NuGetODataV1FeedConfig.cs`, `..\App_Start\NuGetODataV2FeedConfig.cs`, and
`..\Controllers\ODataV1FeedController.cs` / `ODataV2FeedController.cs`).

## Why this is hand-rolled instead of built on `Microsoft.AspNetCore.OData`

The spike for this work item (parent step 0, decision D3) already established that
**`Microsoft.AspNetCore.OData` cannot serve these legacy feeds at all**: it is versioned
against OData v4 and does not support the OData v1/v2 wire format
(`Microsoft.Data.OData`/`System.Data.Services.Common`-era atom+xml, `DataServiceVersion`
1.0/2.0 headers, the specific `m:properties`/`d:*` element shapes legacy NuGet clients
(nuget.exe pre-4.x, older Visual Studio Package Manager Console, some third-party tooling)
parse). There is no drop-in library replacement for this wire format on ASP.NET Core.

Per the parent spec, the remaining option is a **bespoke, non-OData-library
implementation**: plain ASP.NET Core MVC controllers that hand-write the exact atom+xml
shapes the legacy `System.Web.Http.OData` formatter + `NuGetEntityTypeSerializer` +
`V1FeedPackageAnnotationStrategy`/`V2FeedPackageAnnotationStrategy` produced. That is what
this folder does.

## What is implemented here (this session)

- `V1FeedPackage.cs` / `V2FeedPackage.cs`: POCOs mirroring the legacy DTOs
  (`..\OData\V1FeedPackage.cs`, `..\OData\V2FeedPackage.cs`), minus the
  `System.Data.Services.Common` attributes (`DataServiceKey`, `EntityPropertyMapping`,
  `HasStream`), which do not exist outside .NET Framework / WCF Data Services and have no
  bearing on a hand-rolled serializer (those attributes only ever fed the old
  `System.Web.Http.OData` formatter's reflection-based property mapping).
- `IPackageFeedSource.cs` + `InMemorySamplePackageFeedSource`: a placeholder data source.
  The real gallery data access (`IReadOnlyEntityRepository<Package>`, `SearchAdaptor`,
  `SearchHijacker`, EF6-backed `EntitiesContext`) is **not wired up** -- `NuGetGallery.Core`
  / `NuGetGallery.Services` do not have a net10.0 `TargetFramework` yet (tracked by the
  sibling "Shared class libraries: add/verify net10 target" work), and the substantive
  business services (`IPackageService`, search hijacking, feature flags) are excluded from
  even the netstandard2.1 leg those projects do have (see this workitem's pinned memory).
  Swapping in the real data source once that dependency lands is a drop-in replacement of
  this interface's implementation; the controllers below only depend on the interface.
- `ODataAtomFeedWriter.cs`: hand-written atom+xml writer (`System.Xml`) producing the same
  element/namespace shapes as the legacy serializer for a **single entry** and a **feed**:
  `<entry>`/`<feed>` with `xmlns` for `atom`, `m` (metadata) and `d` (data), `<m:properties>`
  with `<d:PropertyName>` children in the same property order/types as `V1FeedPackage` /
  `V2FeedPackage`, `<category term="NuGetGallery.OData.V{1,2}FeedPackage">`, and a `<content
  type="application/zip" src="...">` pointing at the package download URL, matching
  `IFeedPackageAnnotationStrategy.Annotate`'s legacy behavior.
- `ODataV1FeedController.cs` / `ODataV2FeedController.cs` (under `..\Controllers.Core\`):
  plain `Microsoft.AspNetCore.Mvc.ControllerBase` controllers, **not** OData controllers,
  mapped explicitly in `Program.cs` to the legacy route shapes:
  - `GET /api/v1/` and `GET /api/v2/` -- service document
  - `GET /api/v1/$metadata` and `GET /api/v2/$metadata` -- static CSDL (hand-written to
    mirror the shape `NuGetODataV1FeedConfig.GetEdmModel()` / `NuGetODataV2FeedConfig
    .GetEdmModel()` used to generate at runtime)
  - `GET /api/v{1,2}/Packages(Id='{id}',Version='{version}')` -- single entry
  - `GET /api/v{1,2}/FindPackagesById()?id='{id}'` -- feed of all versions of a package id

## What is explicitly NOT implemented (out of scope for this session)

The legacy controllers (729 lines for V2, 391 for V1) layer a large amount of behavior on
top of the above that this bespoke implementation does not attempt to reproduce, because it
amounts to reimplementing an OData v1/v2 `$filter`/`$orderby`/`$top`/`$skip` query engine by
hand:

- Arbitrary `ODataQueryOptions<T>` support (`$filter`, `$orderby`, `$top`, `$skip`,
  `$inlinecount`, `$select`) -- `ODataQueryVerifier`/`ODataQueryFilter` allow-list only a
  specific subset of OData query shapes NuGet clients actually send; reproducing that
  allow-list plus the LINQ-expression translation is a substantial, separable piece of work.
- The `Search()` and `GetUpdates()` OData actions, and the search-service "hijacking"
  behavior (`SearchAdaptor`, `SearchHijacker`, `IHijackSearchServiceFactory`) that redirects
  `$filter`-based package listing/search to the external search service instead of SQL.
  Reproducing this is impossible before search-service integration exists in the net10 app.
  the version-normalization / sort-suppression query interceptors (`NormalizeVersionInterceptor`,
  `ODataRemoveSorter`, `DisregardODataInterceptor`, `CountInterceptor`).
- `$count` sub-resources (`/Packages/$count`, `/FindPackagesById()/$count`).
- Feature-flag-gated behavior differences (`IFeatureFlagService.IsODataV2*Enabled()` family).
- Curated feeds (`api/v2/curated-feeds/{curatedFeedName}`).
- OData batch requests (`ODataServiceVersionHeaderPropagatingBatchHandler`).

These are real, separable pieces of remaining work; a from-scratch OData v1/v2 query engine
reimplementation is estimated at several additional work sessions beyond this one, once real
data access (`Package`/search) is wired into the net10 app. See this task's `test_report`
artifact for the honest assessment of what was verified vs. not.
