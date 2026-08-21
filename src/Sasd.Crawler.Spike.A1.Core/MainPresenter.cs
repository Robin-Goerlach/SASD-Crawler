namespace Sasd.Crawler.Spike.A1.Core;

/// <summary>View contract that keeps WinForms controls out of lifecycle and persistence logic.</summary>
public interface IMainView
{
    void ShowStatus(string workerStatus, string lastHeartbeat, bool paused);
    void ActivateWindow();
}

/// <summary>Coordinates presentation-safe state reads and marshals notifications to the UI context.</summary>
public sealed class MainPresenter : IDisposable
{
    private readonly IMainView view;
    private readonly IHeartbeatStore store;
    private readonly IHeartbeatState state;
    private readonly SynchronizationContext uiContext;

    public MainPresenter(IMainView view, IHeartbeatStore store, IHeartbeatState state, SynchronizationContext uiContext)
    {
        this.view = view;
        this.store = store;
        this.state = state;
        this.uiContext = uiContext;
        state.Changed += StateChanged;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var heartbeat = state.Current ?? await store.ReadLatestAsync(cancellationToken).ConfigureAwait(false);
        Render(heartbeat);
    }

    public void TogglePause() => state.SetPaused(!state.IsPaused);
    public void Activate() => uiContext.Post(_ => view.ActivateWindow(), null);

    private void StateChanged(object? sender, EventArgs e) => Render(state.Current);

    private void Render(Heartbeat? heartbeat) => uiContext.Post(_ =>
        view.ShowStatus(
            state.IsPaused ? "Paused" : heartbeat?.Status ?? "Starting",
            heartbeat?.TimestampUtc.ToLocalTime().ToString("G") ?? "No heartbeat yet",
            state.IsPaused), null);

    public void Dispose() => state.Changed -= StateChanged;
}
