// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using NuGetGallery.Infrastructure.JobCoordination;
using Xunit;

namespace NuGetGallery.Infrastructure.JobCoordination
{
    public class JobGuardFacts
    {
        private const string Job = "job";
        private static readonly TimeSpan Lease = TimeSpan.FromMilliseconds(300);

        private static JobGuard NewGuard(FakeStore store) => new JobGuard(store, TimeSpan.FromMilliseconds(200));

        [Fact]
        public async Task CompletedRunRecordsOccurrenceAndIsNotRunAgainUntilNextDue()
        {
            var store = new FakeStore(Lease, TimeSpan.FromMilliseconds(400));
            var runs = 0;

            var first = await NewGuard(store).RunAsync(Job, (o, t) => { runs++; return Task.CompletedTask; }, CancellationToken.None);
            var second = await NewGuard(store).RunAsync(Job, (o, t) => { runs++; return Task.CompletedTask; }, CancellationToken.None);

            Assert.Equal(JobGuardOutcome.Completed, first.Outcome);
            Assert.Equal(JobGuardOutcome.NotClaimed, second.Outcome);
            Assert.Equal(1, runs);

            await Task.Delay(500);
            var third = await NewGuard(store).RunAsync(Job, (o, t) => { runs++; return Task.CompletedTask; }, CancellationToken.None);
            Assert.Equal(JobGuardOutcome.Completed, third.Outcome);
            Assert.Equal(2, runs);
            Assert.Equal(2, third.Occurrence.Generation);
        }

        [Fact]
        public async Task SecondHostCannotRunWhileFirstHoldsClaim()
        {
            var store = new FakeStore(Lease, TimeSpan.FromHours(1));
            var started = new TaskCompletionSource<bool>();
            var release = new TaskCompletionSource<bool>();

            var hostA = NewGuard(store).RunAsync(Job, async (o, t) => { started.SetResult(true); await release.Task; }, CancellationToken.None);
            await started.Task;

            var hostB = await NewGuard(store).RunAsync(Job, (o, t) => Task.CompletedTask, CancellationToken.None);
            release.SetResult(true);

            Assert.Equal(JobGuardOutcome.NotClaimed, hostB.Outcome);
            Assert.Equal(JobGuardOutcome.Completed, (await hostA).Outcome);
        }

        [Fact]
        public async Task HandoverAfterLostRenewalRetriesSameOccurrenceWithNewGeneration()
        {
            var store = new FakeStore(Lease, TimeSpan.FromHours(1)) { FailRenewals = true };
            var cancelled = new TaskCompletionSource<bool>();

            var hostA = await NewGuard(store).RunAsync(
                Job,
                async (o, t) =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), t);
                    }
                    catch (OperationCanceledException)
                    {
                        cancelled.SetResult(true);
                        throw;
                    }
                },
                CancellationToken.None);

            Assert.True(cancelled.Task.IsCompleted);
            Assert.Equal(JobGuardOutcome.ClaimLost, hostA.Outcome);

            store.FailRenewals = false;
            await Task.Delay(Lease + TimeSpan.FromMilliseconds(100));

            var hostB = await NewGuard(store).RunAsync(Job, (o, t) => Task.CompletedTask, CancellationToken.None);

            Assert.Equal(JobGuardOutcome.Completed, hostB.Outcome);
            Assert.Equal(hostA.Occurrence.DueUtc, hostB.Occurrence.DueUtc);
            Assert.Equal(hostA.Occurrence.Generation + 1, hostB.Occurrence.Generation);
        }

        [Fact]
        public async Task FailedRunDefersRetryInStore()
        {
            var store = new FakeStore(Lease, TimeSpan.FromHours(1));

            var failed = await NewGuard(store).RunAsync(Job, (o, t) => throw new InvalidOperationException("boom"), CancellationToken.None);
            var immediate = await NewGuard(store).RunAsync(Job, (o, t) => Task.CompletedTask, CancellationToken.None);
            await Task.Delay(300);
            var retried = await NewGuard(store).RunAsync(Job, (o, t) => Task.CompletedTask, CancellationToken.None);

            Assert.Equal(JobGuardOutcome.Failed, failed.Outcome);
            Assert.IsType<InvalidOperationException>(failed.Exception);
            Assert.Equal(JobGuardOutcome.NotClaimed, immediate.Outcome);
            Assert.Equal(JobGuardOutcome.Completed, retried.Outcome);
            Assert.Equal(failed.Occurrence.DueUtc, retried.Occurrence.DueUtc);
        }

        [Fact]
        public async Task UnreachableStoreFailsClosed()
        {
            var store = new FakeStore(Lease, TimeSpan.FromHours(1)) { Unreachable = true };
            var ran = false;

            var result = await NewGuard(store).RunAsync(Job, (o, t) => { ran = true; return Task.CompletedTask; }, CancellationToken.None);

            Assert.Equal(JobGuardOutcome.StoreUnavailable, result.Outcome);
            Assert.False(ran);
        }

        [Fact]
        public async Task StalledRenewalCancelsWorkAtLeaseExpiry()
        {
            var store = new FakeStore(Lease, TimeSpan.FromHours(1)) { StallRenewals = true };

            var result = await NewGuard(store).RunAsync(Job, (o, t) => Task.Delay(TimeSpan.FromSeconds(10), t), CancellationToken.None);

            Assert.Equal(JobGuardOutcome.ClaimLost, result.Outcome);
        }

        [Fact]
        public async Task ReleasedClaimAfterNoRecordAllowsImmediateReclaim()
        {
            var store = new FakeStore(Lease, TimeSpan.FromHours(1));
            var claim = await store.TryClaimDueAsync(Job, CancellationToken.None);
            await claim.ReleaseAsync();

            var again = await store.TryClaimDueAsync(Job, CancellationToken.None);

            Assert.NotNull(again);
            Assert.Equal(claim.Occurrence.DueUtc, again.Occurrence.DueUtc);
        }

        private sealed class FakeStore : IJobScheduleStore
        {
            private readonly object _lock = new object();
            private readonly TimeSpan _interval;
            private DateTime _nextDue = DateTime.UtcNow.AddSeconds(-1);
            private DateTime _retryAfter = DateTime.MinValue;
            private DateTime _leaseExpiry = DateTime.MinValue;
            private long _generation;
            private object _holder;

            public FakeStore(TimeSpan leaseDuration, TimeSpan interval)
            {
                LeaseDuration = leaseDuration;
                _interval = interval;
            }

            public TimeSpan LeaseDuration { get; }
            public bool FailRenewals { get; set; }
            public bool StallRenewals { get; set; }
            public bool Unreachable { get; set; }

            public Task<IJobClaim> TryClaimDueAsync(string jobName, CancellationToken cancellationToken)
            {
                if (Unreachable)
                {
                    throw new InvalidOperationException("store down");
                }

                lock (_lock)
                {
                    var now = DateTime.UtcNow;
                    if ((_holder != null && _leaseExpiry > now) || _nextDue > now || _retryAfter > now)
                    {
                        return Task.FromResult<IJobClaim>(null);
                    }

                    var token = new object();
                    _holder = token;
                    _leaseExpiry = now + LeaseDuration;
                    _generation++;
                    return Task.FromResult<IJobClaim>(new Claim(this, token, new JobOccurrence(jobName, _nextDue, _generation)));
                }
            }

            private sealed class Claim : IJobClaim
            {
                private readonly FakeStore _store;
                private readonly object _token;

                public Claim(FakeStore store, object token, JobOccurrence occurrence)
                {
                    _store = store;
                    _token = token;
                    Occurrence = occurrence;
                }

                public JobOccurrence Occurrence { get; }

                public async Task<bool> RenewAsync(CancellationToken cancellationToken)
                {
                    if (_store.StallRenewals)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                    }

                    lock (_store._lock)
                    {
                        if (_store.FailRenewals || _store._holder != _token)
                        {
                            return false;
                        }

                        _store._leaseExpiry = DateTime.UtcNow + _store.LeaseDuration;
                        return true;
                    }
                }

                public Task<bool> CompleteAsync(CancellationToken cancellationToken)
                {
                    return Task.FromResult(Record(() =>
                    {
                        _store._nextDue = DateTime.UtcNow + _store._interval;
                        _store._retryAfter = DateTime.MinValue;
                    }));
                }

                public Task<bool> FailAsync(TimeSpan retryAfter, CancellationToken cancellationToken)
                {
                    return Task.FromResult(Record(() => _store._retryAfter = DateTime.UtcNow + retryAfter));
                }

                public Task ReleaseAsync()
                {
                    lock (_store._lock)
                    {
                        if (_store._holder == _token)
                        {
                            _store._holder = null;
                        }
                    }

                    return Task.CompletedTask;
                }

                private bool Record(Action update)
                {
                    lock (_store._lock)
                    {
                        if (_store._holder != _token)
                        {
                            return false;
                        }

                        update();
                        _store._holder = null;
                        return true;
                    }
                }
            }
        }
    }
}
