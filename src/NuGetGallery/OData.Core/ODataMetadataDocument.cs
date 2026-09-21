// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGetGallery.OData.Core
{
    /// <summary>
    /// Static CSDL (the OData v1/v2 "$metadata" document) mirroring the schema the legacy
    /// <c>NuGetODataV1FeedConfig.GetEdmModel()</c> / <c>NuGetODataV2FeedConfig.GetEdmModel()</c>
    /// built dynamically via <c>ODataConventionModelBuilder</c> at startup (see
    /// <c>..\App_Start\NuGetODataV1FeedConfig.cs</c>, <c>..\App_Start\NuGetODataV2FeedConfig.cs</c>).
    /// Hand-written here because there is no OData v1/v2 model builder available on net10 (see
    /// <c>OData.Core\README.md</c>).
    /// </summary>
    public static class ODataMetadataDocument
    {
        public const string V1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""1.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2007/06/edmx"">
  <edmx:DataServices xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" m:DataServiceVersion=""1.0"">
    <Schema Namespace=""NuGetGallery"" xmlns=""http://schemas.microsoft.com/ado/2006/04/edm"">
      <EntityType Name=""V1FeedPackage"" m:HasStream=""true"">
        <Key><PropertyRef Name=""Id"" /><PropertyRef Name=""Version"" /></Key>
        <Property Name=""Id"" Type=""Edm.String"" Nullable=""false"" />
        <Property Name=""Version"" Type=""Edm.String"" Nullable=""false"" />
        <Property Name=""NormalizedVersion"" Type=""Edm.String"" />
        <Property Name=""Title"" Type=""Edm.String"" />
        <Property Name=""Summary"" Type=""Edm.String"" />
        <Property Name=""Description"" Type=""Edm.String"" />
        <Property Name=""Authors"" Type=""Edm.String"" />
        <Property Name=""Copyright"" Type=""Edm.String"" />
        <Property Name=""Tags"" Type=""Edm.String"" />
        <Property Name=""ProjectUrl"" Type=""Edm.String"" />
        <Property Name=""IconUrl"" Type=""Edm.String"" />
        <Property Name=""LicenseUrl"" Type=""Edm.String"" />
        <Property Name=""RequireLicenseAcceptance"" Type=""Edm.Boolean"" Nullable=""false"" />
        <Property Name=""Dependencies"" Type=""Edm.String"" />
        <Property Name=""Created"" Type=""Edm.DateTime"" Nullable=""false"" />
        <Property Name=""Published"" Type=""Edm.DateTime"" Nullable=""false"" />
        <Property Name=""LastUpdated"" Type=""Edm.DateTime"" Nullable=""false"" />
        <Property Name=""PackageSize"" Type=""Edm.Int64"" Nullable=""false"" />
        <Property Name=""PackageHash"" Type=""Edm.String"" />
        <Property Name=""PackageHashAlgorithm"" Type=""Edm.String"" />
        <Property Name=""DownloadCount"" Type=""Edm.Int64"" Nullable=""false"" />
        <Property Name=""VersionDownloadCount"" Type=""Edm.Int64"" Nullable=""false"" />
        <Property Name=""IsLatestVersion"" Type=""Edm.Boolean"" Nullable=""false"" />
        <Property Name=""IsAbsoluteLatestVersion"" Type=""Edm.Boolean"" Nullable=""false"" />
        <Property Name=""IsPrerelease"" Type=""Edm.Boolean"" Nullable=""false"" />
        <Property Name=""GalleryDetailsUrl"" Type=""Edm.String"" />
        <Property Name=""ReportAbuseUrl"" Type=""Edm.String"" />
        <Property Name=""MinClientVersion"" Type=""Edm.String"" />
      </EntityType>
      <EntityContainer Name=""V1FeedContext"" m:IsDefaultEntityContainer=""true"">
        <EntitySet Name=""Packages"" EntityType=""NuGetGallery.V1FeedPackage"" />
        <FunctionImport Name=""FindPackagesById"" ReturnType=""Collection(NuGetGallery.V1FeedPackage)"" EntitySet=""Packages"">
          <Parameter Name=""id"" Type=""Edm.String"" Mode=""In"" />
        </FunctionImport>
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";

        public const string V2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""1.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2007/06/edmx"">
  <edmx:DataServices xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" m:DataServiceVersion=""2.0"">
    <Schema Namespace=""NuGetGallery"" xmlns=""http://schemas.microsoft.com/ado/2006/04/edm"">
      <EntityType Name=""V2FeedPackage"" m:HasStream=""true"">
        <Key><PropertyRef Name=""Id"" /><PropertyRef Name=""Version"" /></Key>
        <Property Name=""Id"" Type=""Edm.String"" Nullable=""false"" />
        <Property Name=""Version"" Type=""Edm.String"" Nullable=""false"" />
        <Property Name=""NormalizedVersion"" Type=""Edm.String"" />
        <Property Name=""Authors"" Type=""Edm.String"" />
        <Property Name=""Copyright"" Type=""Edm.String"" />
        <Property Name=""Created"" Type=""Edm.DateTime"" Nullable=""false"" />
        <Property Name=""Dependencies"" Type=""Edm.String"" />
        <Property Name=""Description"" Type=""Edm.String"" />
        <Property Name=""DownloadCount"" Type=""Edm.Int64"" Nullable=""false"" />
        <Property Name=""GalleryDetailsUrl"" Type=""Edm.String"" />
        <Property Name=""IconUrl"" Type=""Edm.String"" />
        <Property Name=""IsLatestVersion"" Type=""Edm.Boolean"" Nullable=""false"" />
        <Property Name=""IsAbsoluteLatestVersion"" Type=""Edm.Boolean"" Nullable=""false"" />
        <Property Name=""IsPrerelease"" Type=""Edm.Boolean"" Nullable=""false"" />
        <Property Name=""Language"" Type=""Edm.String"" />
        <Property Name=""LastUpdated"" Type=""Edm.DateTime"" Nullable=""false"" />
        <Property Name=""Published"" Type=""Edm.DateTime"" Nullable=""false"" />
        <Property Name=""PackageHash"" Type=""Edm.String"" />
        <Property Name=""PackageHashAlgorithm"" Type=""Edm.String"" />
        <Property Name=""PackageSize"" Type=""Edm.Int64"" Nullable=""false"" />
        <Property Name=""ProjectUrl"" Type=""Edm.String"" />
        <Property Name=""ReportAbuseUrl"" Type=""Edm.String"" />
        <Property Name=""ReleaseNotes"" Type=""Edm.String"" />
        <Property Name=""RequireLicenseAcceptance"" Type=""Edm.Boolean"" Nullable=""false"" />
        <Property Name=""Summary"" Type=""Edm.String"" />
        <Property Name=""Tags"" Type=""Edm.String"" />
        <Property Name=""Title"" Type=""Edm.String"" />
        <Property Name=""VersionDownloadCount"" Type=""Edm.Int64"" Nullable=""false"" />
        <Property Name=""MinClientVersion"" Type=""Edm.String"" />
        <Property Name=""LastEdited"" Type=""Edm.DateTime"" />
        <Property Name=""LicenseUrl"" Type=""Edm.String"" />
        <Property Name=""LicenseNames"" Type=""Edm.String"" />
        <Property Name=""LicenseReportUrl"" Type=""Edm.String"" />
      </EntityType>
      <EntityContainer Name=""V2FeedContext"" m:IsDefaultEntityContainer=""true"">
        <EntitySet Name=""Packages"" EntityType=""NuGetGallery.V2FeedPackage"" />
        <FunctionImport Name=""FindPackagesById"" ReturnType=""Collection(NuGetGallery.V2FeedPackage)"" EntitySet=""Packages"">
          <Parameter Name=""id"" Type=""Edm.String"" Mode=""In"" />
        </FunctionImport>
        <FunctionImport Name=""Search"" ReturnType=""Collection(NuGetGallery.V2FeedPackage)"" EntitySet=""Packages"">
          <Parameter Name=""searchTerm"" Type=""Edm.String"" Mode=""In"" />
          <Parameter Name=""targetFramework"" Type=""Edm.String"" Mode=""In"" />
          <Parameter Name=""includePrerelease"" Type=""Edm.Boolean"" Mode=""In"" />
        </FunctionImport>
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
    }
}
