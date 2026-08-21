using Microsoft.Extensions.Logging.Abstractions;
using Sasd.Crawler.Spike.A1.Core;

namespace Sasd.Crawler.Spike.A1.Tests;

public sealed class HeartbeatWorkerTests
{
    [Fact]
    public async Task A1_HostCancellationStopsWorker()
    {
        var store = new RecordingStore();
        var worker = new HeartbeatWorker(store, new HeartbeatState(), NullLogger<HeartbeatWorker>.Instance, TimeSpan.FromMilliseconds(20));
        await worker.StartAsync(CancellationToken.None);
        await store.Written.Task.WaitAsync(TimeSpan.FromSeconds(2));

        await worker.StopAsync(new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token);

        Assert.True(worker.ExecuteTask?.IsCompleted);
    }

    [Fact]
    public async Task A1_PausePreventsNewPeriodicWrites()
    {
        var store = new RecordingStore();
        var state = new HeartbeatState();
        state.SetPaused(true);
        var worker = new HeartbeatWorker(store, state, NullLogger<HeartbeatWorker>.Instance, TimeSpan.FromMilliseconds(20));
        await worker.StartAsync(CancellationToken.None);
        await store.Written.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var initialCount = store.Count;
        await Task.Delay(100);
        await worker.StopAsync(CancellationToken.None);

        Assert.Equal(initialCount, store.Count);
    }

    private sealed class RecordingStore : IHeartbeatStore
    {
        public int Count { get; private set; }
        public TaskCompletionSource Written { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Heartbeat?> ReadLatestAsync(CancellationToken cancellationToken) => Task.FromResult<Heartbeat?>(null);
        public Task WriteAsync(Heartbeat heartbeat, CancellationToken cancellationToken)
        {
            Count++;
            Written.TrySetResult();
            return Task.CompletedTask;
        }
    }
}
