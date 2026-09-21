// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGetGallery.Configuration
{
    /// <summary>
    /// Options bound from the "Session" configuration section (appsettings.json).
    ///
    /// Replaces the legacy Web.config &lt;system.web&gt;&lt;sessionState mode="Off"/&gt; setting.
    /// ASP.NET Core does not add session support to the pipeline unless
    /// <c>app.UseSession()</c> is explicitly called, so "off" is the default behavior with no
    /// action required; <see cref="Enabled"/> exists only so a future deployment can opt back in
    /// without a further Web.config-style code change.
    /// </summary>
    public class GallerySessionOptions
    {
        public const string SectionName = "Session";

        public bool Enabled { get; set; }
    }

    /// <summary>
    /// Options bound from the "DataProtection" configuration section (appsettings.json).
    ///
    /// Replaces the legacy Web.config &lt;machineKey configProtectionProvider="GalleryMachineKeyConfigurationProvider"&gt;
    /// section and its backing <c>NuGetGallery.GalleryMachineKeyConfigurationProvider</c> (App_Start), whose job was
    /// to hand ASP.NET a fixed decryption/validation key pair so that cookies encrypted/signed by one instance of
    /// the gallery could be read by another (multiple role instances, or a slot swap), instead of each instance
    /// generating its own ephemeral machine key. See https://github.com/NuGet/Engineering/issues/1329.
    ///
    /// The Data Protection API is the ASP.NET Core-native equivalent of that cross-instance key sharing concern:
    /// by default each instance/process gets its own auto-generated, ephemeral key ring, which does not survive
    /// a restart and is not shared across instances -- the same problem the legacy machine key configuration
    /// existed to solve. When <see cref="EnableKeyPersistence"/> is true and <see cref="KeyRingDirectory"/> is
    /// set, the key ring is persisted to a shared directory (e.g. a mounted network/file share reachable from
    /// every instance) so all instances of the gallery use the same keys, and keys survive a restart.
    /// </summary>
    public class GalleryDataProtectionOptions
    {
        public const string SectionName = "DataProtection";

        /// <summary>
        /// The Data Protection "application name" isolation discriminator. Must be identical across every
        /// instance that needs to share protected payloads (cookies, etc.) -- analogous to all instances
        /// needing the same &lt;machineKey&gt; values in the legacy configuration.
        /// </summary>
        public string ApplicationName { get; set; } = "NuGetGallery";

        /// <summary>
        /// Mirrors the legacy "Gallery.EnableMachineKeyConfiguration" appSetting: when false (the default,
        /// matching the shipped Web.config), each instance keeps its own ephemeral, ASP.NET Core-managed key
        /// ring, and <see cref="KeyRingDirectory"/> is ignored.
        /// </summary>
        public bool EnableKeyPersistence { get; set; }

        /// <summary>
        /// Directory (typically a shared/mounted path reachable by every instance) that the Data Protection
        /// key ring is persisted to when <see cref="EnableKeyPersistence"/> is true. Mirrors the role that
        /// "Gallery.MachineKeyDecryptionKey"/"Gallery.MachineKeyValidationKey" played in the legacy provider,
        /// except the API manages key material itself instead of accepting raw key bytes from configuration.
        /// </summary>
        public string KeyRingDirectory { get; set; }
    }
}

