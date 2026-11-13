// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetGallery.Infrastructure.JobCoordination
{
    public enum JobGuardOutcome
    {
        /// <summary>Not due, or another host holds the claim. The work did not run.</summary>
        NotClaimed,

        /// <summary>The store could not be reached. Fails closed: the work did not run.</summary>
        StoreUnavailable,

        /// <summary>The work ran and the occurrence was recorded.</summary>
        Completed,

        /// <summary>The work threw; a retry was deferred in the store.</summary>
        Failed,

        /// <summary>The claim was lost or could not be confirmed, or recording failed. The occurrence stays due.</summary>
        ClaimLost,
    }

    public sealed class JobGuardResult
    {
        public JobGuardResult(JobGuardOutcome outcome, JobOccurrence occurrence = null, Exception exception = null)
        {
            Outcome = outcome;
            Occurrence = occurrence;
            Exception = exception;
        }

        public JobGuardOutcome Outcome { get; }
        public JobOccurrence Occurrence { get; }
        public Exception Exception { get; }
    }

    /// <summary>
    /// Runs work only while holding the shared claim for one due occurrence, so the same job never runs on two
    /// hosts at once and a recorded occurrence is not run again. Delivery is at-least-once: make effects
    /// idempotent on <see cref="JobOccurrence.DueUtc"/> or fence them on <see cref="JobOccurrence.Generation"/>.
    /// </summary>
    public sealed class JobGuard
    {
        private readonly IJobScheduleStore _store;
        private readonly TimeSpan _retryDelay;

        public JobGuard(IJobScheduleStore store, TimeSpan retryDelay)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _retryDelay = retryDelay;
        }

        public async Task<JobGuardResult> RunAsync(
            string jobName,
            Func<JobOccurrence, CancellationToken, Task> work,
            CancellationToken cancellationToken)
        {
            IJobClaim claim;
            try
            {
                claim = await _store.TryClaimDueAsync(jobName, cancellationToken);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                return new JobGuardResult(JobGuardOutcome.StoreUnavailable, exception: ex);
            }

            if (claim == null)
            {
                return new JobGuardResult(JobGuardOutcome.NotClaimed);
            }

            try
            {
                using (var workCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    var lost = 0;
                    var renewer = RenewLoopAsync(claim, workCts, () => Interlocked.Exchange(ref lost, 1));

                    Exception failure = null;
                    try
                    {
                        await work(claim.Occurrence, workCts.Token);
                    }
                    catch (Exception ex)
                    {
                        failure = ex;
                    }

                    workCts.Cancel();
                    await renewer;

                    // A run that outlived its lease or lost it is never recorded, even if it finished.
                    if (Volatile.Read(ref lost) == 1)
                    {
                        return new JobGuardResult(JobGuardOutcome.ClaimLost, claim.Occurrence, failure);
                    }

                    if (failure != null)
                    {
                        await SafeAsync(() => claim.FailAsync(_retryDelay, CancellationToken.None));
                        return new JobGuardResult(JobGuardOutcome.Failed, claim.Occurrence, failure);
                    }

                    var recorded = await SafeAsync(() => claim.CompleteAsync(CancellationToken.None));
                    return new JobGuardResult(recorded ? JobGuardOutcome.Completed : JobGuardOutcome.ClaimLost, claim.Occurrence);
                }
            }
            finally
            {
                await claim.ReleaseAsync();
            }
        }

        private static async Task<bool> SafeAsync(Func<Task<bool>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task RenewLoopAsync(IJobClaim claim, CancellationTokenSource workCts, Action markLost)
        {
            var interval = TimeSpan.FromTicks(_store.LeaseDuration.Ticks / 3);
            var sinceConfirmed = Stopwatch.StartNew();
            var token = workCts.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(interval, token);

                    // A stalled renewal must not keep the work alive past the lease.
                    var remaining = _store.LeaseDuration - sinceConfirmed.Elapsed;
                    if (remaining <= TimeSpan.Zero)
                    {
                        markLost();
                        workCts.Cancel();
                        return;
                    }

                    var renewTask = SafeRenewAsync(claim);
                    var finished = await Task.WhenAny(renewTask, Task.Delay(remaining, token));
                    if (finished == renewTask && await renewTask)
                    {
                        sinceConfirmed.Restart();
                        continue;
                    }

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    markLost();
                    workCts.Cancel();
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                // The work finished or the host is stopping.
            }
        }

        private static async Task<bool> SafeRenewAsync(IJobClaim claim)
        {
            try
            {
                return await claim.RenewAsync(CancellationToken.None);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
