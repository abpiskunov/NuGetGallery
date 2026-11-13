// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetGallery.Infrastructure.JobCoordination
{
    /// <summary>
    /// One scheduled occurrence of a job. <see cref="DueUtc"/> is shared by retries of the same occurrence, so
    /// it is the right input for idempotency keys. <see cref="Generation"/> changes on every claim and is the
    /// fencing token for sinks that store and compare it.
    /// </summary>
    public sealed class JobOccurrence
    {
        public JobOccurrence(string jobName, DateTime dueUtc, long generation)
        {
            JobName = jobName;
            DueUtc = dueUtc;
            Generation = generation;
        }

        public string JobName { get; }
        public DateTime DueUtc { get; }
        public long Generation { get; }
    }

    /// <summary>
    /// An exclusive claim on one occurrence of a job. Held by exactly one process at a time across all hosts.
    /// </summary>
    public interface IJobClaim
    {
        JobOccurrence Occurrence { get; }

        /// <summary>False means the claim is lost or cannot be confirmed; the work must stop.</summary>
        Task<bool> RenewAsync(CancellationToken cancellationToken);

        /// <summary>Records the occurrence as done and schedules the next one. False means it was not recorded.</summary>
        Task<bool> CompleteAsync(CancellationToken cancellationToken);

        /// <summary>Keeps the occurrence due but defers a retry by <paramref name="retryAfter"/>.</summary>
        Task<bool> FailAsync(TimeSpan retryAfter, CancellationToken cancellationToken);

        /// <summary>Releases the claim without recording anything. A no-op after Complete or Fail.</summary>
        Task ReleaseAsync();
    }

    /// <summary>
    /// Shared store holding each job's next-due time and claim. All hosts (legacy and App Service) must use
    /// the same store, job names and interval, and decide with the store's clock.
    /// </summary>
    public interface IJobScheduleStore
    {
        TimeSpan LeaseDuration { get; }

        /// <summary>Returns null when the job is not due or is held by another process.</summary>
        Task<IJobClaim> TryClaimDueAsync(string jobName, CancellationToken cancellationToken);
    }
}
