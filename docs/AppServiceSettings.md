# NuGetGallery on Azure App Service (Windows) – settings

The web app stays net472 / System.Web (no retarget). It reads settings through `ConfigurationManager`,
so App Service **Application settings** override `<appSettings>` keys with the same name, and
**Connection strings** override `<connectionStrings>` entries by name. Keys with `:` or `.` are used as-is.
Never commit values; use Key Vault references (`@Microsoft.KeyVault(...)`) for secrets.

## Connection strings (type: SQLAzure)

| Name | Purpose |
|---|---|
| `Gallery.SqlServer` | Main gallery DB (EF migrations; schema changes must be additive while legacy host is live) |
| `Gallery.SupportRequestSqlServer` | Support request DB |
| `Gallery.ValidationSqlServer` | Validation DB |

## Required app settings

| Setting | Notes |
|---|---|
| `Gallery.Environment` | Environment name (default `Development`) |
| `Gallery.SiteRoot`, `Gallery.SupportEmailSiteRoot` | Public https URLs |
| `Gallery.StorageType` | `AzureStorage` (set by `Web.AppService.config`) |
| `Gallery.AzureStorage.{Auditing,UserCertificates,Content,Errors,Packages,FlatContainer,Statistics,Uploads,Revalidation}.ConnectionString` | Required when StorageType is AzureStorage; also `Statistics.ConnectionString.Alternate` |
| `Gallery.AppInsightsInstrumentationKey` | Application Insights |
| `Gallery.SearchServiceUriPrimary` / `Secondary` | Remote search; otherwise Lucene under `App_Data` |
| `Gallery.EnableMachineKeyConfiguration`, `Gallery.MachineKeyDecryption`, `Gallery.MachineKeyDecryptionKey`, `Gallery.MachineKeyValidationAlgorithm`, `Gallery.MachineKeyValidationKey` | Shared machine key so cookies survive slot swaps and both hosts |

## Conditional app settings

- Service Bus (required if `Gallery.AsynchronousPackageValidationEnabled`): `AzureServiceBus.ManagedIdentityClientId`, `AzureServiceBus.{AccountDeleter,Validation,SymbolsValidation,EmailPublisher}.{ConnectionString,TopicName}`.
- Key Vault: `KeyVault.VaultName`, `KeyVault.UseManagedIdentity`, `KeyVault.ClientId`, `KeyVault.CertificateThumbprint`.
- Auth: `Auth.*` (`MicrosoftAccount`, `AzureActiveDirectory`, `AzureActiveDirectoryV2`), `Gallery.EnforcedAuthProviderForAdmin`, `Gallery.EnforcedTenantIdForAdmin`.
- Admin/Federated: `Gallery.AdminApi*`, `FederatedCredential.*`.
- Other: `Gallery.AzureCdnHost`, `Gallery.SmtpUri`, `Gallery.ReCaptcha{Private,Public}Key`, `Gallery.CspReportUri`, `Gallery.AlternateSiteRootList`, `Gallery.FeatureFlagsRefreshInterval`.

Full list with defaults: `src/NuGetGallery/Web.config` `<appSettings>`.

## Health and warmup

- Health check path: `/api/health-probe` (also `/api/status`); both are excluded from HTTPS redirect via `Gallery.ForceSslExclusion`.
- Warmup: `<applicationInitialization>` in Web.config already requests `/` and `/api/health-probe`. Set App Service **Always On** and, for slots, `WEBSITE_SWAP_WARMUP_PING_PATH=/api/health-probe`.

## Transforms

`Web.Release.config` is unchanged. `Web.AppService.config` is an optional extra transform
(AzureStorage, no dev banner, placeholder connection strings so a missing App Service connection string fails fast).
Apply after `Web.Release.config`.

## Duplicate execution

In-process background jobs (WebBackgrounder in `AppActivator`) run in every instance; a guard is handled by the separate duplicate-execution task.
