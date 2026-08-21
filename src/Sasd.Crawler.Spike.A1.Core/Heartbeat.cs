namespace Sasd.Crawler.Spike.A1.Core;

/// <summary>Immutable status persisted by the A1 lifecycle worker.</summary>
public sealed record Heartbeat(DateTimeOffset TimestampUtc, string Status, int ProcessId);

/// <summary>Persistence boundary used by the worker and presentation service.</summary>
public interface IHeartbeatStore
{
    Task InitializeAsync(CancellationToken cancellationToken);
    Task WriteAsync(Heartbeat heartbeat, CancellationToken cancellationToken);
    Task<Heartbeat?> ReadLatestAsync(CancellationToken cancellationToken);
}

/// <summary>Publishes UI-neutral lifecycle state. Subscribers choose their synchronization context.</summary>
public interface IHeartbeatState
{
    Heartbeat? Current { get; }
    bool IsPaused { get; }
    event EventHandler? Changed;
    void Publish(Heartbeat heartbeat);
    void SetPaused(bool paused);
}

/// <summary>Thread-safe in-memory state shared by the host worker and presenter.</summary>
public sealed class HeartbeatState : IHeartbeatState
{
    private readonly object sync = new();
    private Heartbeat? current;
    private bool paused;

    public Heartbeat? Current { get { lock (sync) return current; } }
    public bool IsPaused { get { lock (sync) return paused; } }
    public event EventHandler? Changed;

    public void Publish(Heartbeat heartbeat)
    {
        lock (sync) current = heartbeat;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetPaused(bool value)
    {
        lock (sync) paused = value;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
