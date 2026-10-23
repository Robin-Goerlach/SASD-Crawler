using Sasd.Crawler.Spike.A1.Core;

namespace Sasd.Crawler.Spike.A1.Tests;

public sealed class MainPresenterTests
{
    [Fact]
    public async Task A1_PresenterRendersPersistedStateWithoutUiDependencies()
    {
        var heartbeat = new Heartbeat(DateTimeOffset.UtcNow, "Recovered", 1);
        var view = new RecordingView();
        using var presenter = new MainPresenter(view, new Store(heartbeat), new HeartbeatState(), new ImmediateContext());

        await presenter.RefreshAsync(CancellationToken.None);

        Assert.Equal("Recovered", view.Status);
        Assert.NotNull(view.Timestamp);
    }

    private sealed class RecordingView : IMainView
    {
        public string? Status { get; private set; }
        public string? Timestamp { get; private set; }
        public void ActivateWindow() { }
        public void ShowStatus(string workerStatus, string lastHeartbeat, bool paused) { Status = workerStatus; Timestamp = lastHeartbeat; }
    }

    private sealed class Store(Heartbeat heartbeat) : IHeartbeatStore
    {
        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Heartbeat?> ReadLatestAsync(CancellationToken cancellationToken) => Task.FromResult<Heartbeat?>(heartbeat);
        public Task WriteAsync(Heartbeat value, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class ImmediateContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state) => d(state);
    }
}
