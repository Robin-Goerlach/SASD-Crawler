using Microsoft.Data.Sqlite;
using Sasd.Crawler.Spike.A1.Core;
using Sasd.Crawler.Spike.A1.Infrastructure;

namespace Sasd.Crawler.Spike.A1.Tests;

public sealed class SqliteHeartbeatStoreTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "sasd-a1-tests", Guid.NewGuid().ToString("N"));
    private string DatabasePath => Path.Combine(directory, "heartbeat.db");

    [Fact]
    public async Task A1_PersistsAndReadsHeartbeatStatus()
    {
        var store = new SqliteHeartbeatStore(DatabasePath);
        var expected = new Heartbeat(DateTimeOffset.UtcNow, "Test", 42);
        await store.InitializeAsync(CancellationToken.None);
        await store.WriteAsync(expected, CancellationToken.None);

        var actual = await store.ReadLatestAsync(CancellationToken.None);

        Assert.NotNull(actual);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.ProcessId, actual.ProcessId);
        Assert.Equal(expected.TimestampUtc, actual.TimestampUtc);
    }

    [Fact]
    public async Task A1_OrderedShutdownLeavesSqliteReopenable()
    {
        var first = new SqliteHeartbeatStore(DatabasePath);
        await first.InitializeAsync(CancellationToken.None);
        await first.WriteAsync(new Heartbeat(DateTimeOffset.UtcNow, "Before shutdown", 7), CancellationToken.None);

        // A new store simulates the next process and must obtain an exclusive write transaction.
        var reopened = new SqliteHeartbeatStore(DatabasePath);
        await reopened.InitializeAsync(CancellationToken.None);
        await using var connection = new SqliteConnection($"Data Source={DatabasePath};Pooling=False");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "BEGIN EXCLUSIVE; COMMIT;";
        await command.ExecuteNonQueryAsync();
        Assert.NotNull(await reopened.ReadLatestAsync(CancellationToken.None));
    }

    [Fact]
    public async Task A1_PreviousRunStateIsRecoverable()
    {
        var store = new SqliteHeartbeatStore(DatabasePath);
        await store.InitializeAsync(CancellationToken.None);
        await store.WriteAsync(new Heartbeat(DateTimeOffset.UtcNow.AddMinutes(-5), "Running", 999), CancellationToken.None);

        var nextRun = new SqliteHeartbeatStore(DatabasePath);
        await nextRun.InitializeAsync(CancellationToken.None);

        Assert.Equal(999, (await nextRun.ReadLatestAsync(CancellationToken.None))?.ProcessId);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
    }
}
