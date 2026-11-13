// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace NuGetGallery.Infrastructure.JobCoordination
{
    /// <summary>
    /// Azure blob lease implementation of <see cref="IJobScheduleStore"/>. One blob per job at
    /// <c>background-jobs/{job}</c>; the lease is the claim and metadata carries the schedule
    /// (<c>intervalseconds</c>, <c>nextdueutc</c>, <c>generation</c>, optional <c>retryafterutc</c>).
    /// Provision the blob and metadata at deployment, with <c>nextdueutc</c> far in the future until activation.
    /// Times come from the storage service's Date header, never the host clock.
    /// </summary>
    public sealed class BlobJobScheduleStore : IJobScheduleStore
    {
        private const string BlobPrefix = "background-jobs/";
        private readonly BlobContainerClient _container;

        public BlobJobScheduleStore(BlobContainerClient container, TimeSpan leaseDuration)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            // The service accepts finite leases of 15 to 60 seconds.
            if (leaseDuration < TimeSpan.FromSeconds(15) || leaseDuration > TimeSpan.FromSeconds(60))
            {
                throw new ArgumentOutOfRangeException(nameof(leaseDuration), "Blob leases last 15 to 60 seconds.");
            }

            _container = container;
            LeaseDuration = leaseDuration;
        }

        public TimeSpan LeaseDuration { get; }

        public async Task<IJobClaim> TryClaimDueAsync(string jobName, CancellationToken cancellationToken)
        {
            var blob = _container.GetBlobClient(BlobPrefix + jobName);
            var lease = blob.GetBlobLeaseClient();
            try
            {
                await lease.AcquireAsync(LeaseDuration, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
            {
                return null;
            }

            try
            {
                var leased = new BlobRequestConditions { LeaseId = lease.LeaseId };
                var properties = await blob.GetPropertiesAsync(leased, cancellationToken);
                var metadata = new Dictionary<string, string>(properties.Value.Metadata);
                var now = ServiceNow(properties);
                var due = DateTimeOffset.Parse(metadata["nextdueutc"], CultureInfo.InvariantCulture);
                if (due > now
                    || (metadata.TryGetValue("retryafterutc", out var retry)
                        && DateTimeOffset.Parse(retry, CultureInfo.InvariantCulture) > now))
                {
                    await lease.ReleaseAsync(cancellationToken: cancellationToken);
                    return null;
                }

                var generation = long.Parse(metadata["generation"], CultureInfo.InvariantCulture) + 1;
                metadata["generation"] = generation.ToString(CultureInfo.InvariantCulture);
                await blob.SetMetadataAsync(metadata, leased, cancellationToken);
                return new Claim(blob, lease, new JobOccurrence(jobName, due.UtcDateTime, generation));
            }
            catch
            {
                try
                {
                    await lease.ReleaseAsync(cancellationToken: CancellationToken.None);
                }
                catch (RequestFailedException)
                {
                    // Expiry releases the lease regardless.
                }

                throw;
            }
        }

        private static DateTimeOffset ServiceNow(Response<BlobProperties> response)
        {
            var date = response.GetRawResponse().Headers.Date;
            if (date == null)
            {
                throw new InvalidOperationException("The storage response carried no Date header.");
            }

            return date.Value;
        }

        private sealed class Claim : IJobClaim
        {
            private readonly BlobClient _blob;
            private readonly BlobLeaseClient _lease;
            private bool _recordAttempted;

            public Claim(BlobClient blob, BlobLeaseClient lease, JobOccurrence occurrence)
            {
                _blob = blob;
                _lease = lease;
                Occurrence = occurrence;
            }

            public JobOccurrence Occurrence { get; }

            public async Task<bool> RenewAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await _lease.RenewAsync(cancellationToken: cancellationToken);
                    return true;
                }
                catch (RequestFailedException)
                {
                    return false;
                }
            }

            public Task<bool> CompleteAsync(CancellationToken cancellationToken)
            {
                return RecordAsync((metadata, now) =>
                {
                    // The Date header has one-second resolution; the extra second keeps a short interval from collapsing.
                    var interval = TimeSpan.FromSeconds(int.Parse(metadata["intervalseconds"], CultureInfo.InvariantCulture));
                    metadata["nextdueutc"] = (now + interval + TimeSpan.FromSeconds(1)).ToString("O", CultureInfo.InvariantCulture);
                    metadata.Remove("retryafterutc");
                }, cancellationToken);
            }

            public Task<bool> FailAsync(TimeSpan retryAfter, CancellationToken cancellationToken)
            {
                return RecordAsync(
                    (metadata, now) => metadata["retryafterutc"] = (now + retryAfter).ToString("O", CultureInfo.InvariantCulture),
                    cancellationToken);
            }

            public async Task ReleaseAsync()
            {
                if (_recordAttempted)
                {
                    return;
                }

                try
                {
                    await _lease.ReleaseAsync(cancellationToken: CancellationToken.None);
                }
                catch (RequestFailedException)
                {
                    // Expiry releases the lease regardless.
                }
            }

            private async Task<bool> RecordAsync(Action<IDictionary<string, string>, DateTimeOffset> update, CancellationToken cancellationToken)
            {
                // Never release after a record attempt, even a failed one: the lease expires on its own.
                _recordAttempted = true;
                try
                {
                    var leased = new BlobRequestConditions { LeaseId = _lease.LeaseId };
                    var properties = await _blob.GetPropertiesAsync(leased, cancellationToken);
                    var metadata = new Dictionary<string, string>(properties.Value.Metadata);
                    update(metadata, ServiceNow(properties));
                    await _blob.SetMetadataAsync(metadata, leased, cancellationToken);
                }
                catch (RequestFailedException)
                {
                    return false;
                }

                try
                {
                    await _lease.ReleaseAsync(cancellationToken: cancellationToken);
                }
                catch (RequestFailedException)
                {
                    // Recorded already; expiry releases the lease regardless.
                }

                return true;
            }
        }
    }
}
