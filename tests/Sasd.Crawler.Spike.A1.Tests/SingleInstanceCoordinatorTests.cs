using Sasd.Crawler.Spike.A1.Infrastructure;

namespace Sasd.Crawler.Spike.A1.Tests;

public sealed class SingleInstanceCoordinatorTests
{
    [Fact]
    public void A1_PipeIdentityIsStablePerUserAndDistinctAcrossUsers()
    {
        using var first = new SingleInstanceCoordinator("TestApp", "DOMAIN\\alice");
        using var sameUser = new SingleInstanceCoordinator("TestApp", "DOMAIN\\alice");
        using var otherUser = new SingleInstanceCoordinator("TestApp", "DOMAIN\\bob");

        Assert.Equal(first.PipeName, sameUser.PipeName);
        Assert.NotEqual(first.PipeName, otherUser.PipeName);
    }

    [Fact]
    public async Task A1_NamedPipeActivatesPrimaryInstance()
    {
        using var primary = new SingleInstanceCoordinator($"TestApp{Guid.NewGuid():N}", "DOMAIN\\alice");
        using var secondary = new SingleInstanceCoordinator(primary.PipeName, "DOMAIN\\alice");
        // Use the primary pipe identity directly to isolate and verify the IPC primitive.
        var activated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var listen = primary.ListenOnceAsync(() => { activated.TrySetResult(); return Task.CompletedTask; }, CancellationToken.None);
        await Task.Delay(50);

        await SignalAsync(primary.PipeName);

        await activated.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await listen;
    }

    [Fact]
    public async Task A1_SecondInstanceCannotAcquirePerUserMutex()
    {
        var appId = $"TestApp{Guid.NewGuid():N}";
        var acquired = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var release = new ManualResetEventSlim();
        var primaryThread = Task.Factory.StartNew(() =>
        {
            using var primary = new SingleInstanceCoordinator(appId, "DOMAIN\\alice");
            Assert.True(primary.TryAcquirePrimary());
            acquired.TrySetResult();
            release.Wait(TimeSpan.FromSeconds(5));
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        await acquired.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var secondaryResult = await Task.Factory.StartNew(() =>
        {
            using var secondary = new SingleInstanceCoordinator(appId, "DOMAIN\\alice");
            return secondary.TryAcquirePrimary();
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        release.Set();
        await primaryThread;
        Assert.False(secondaryResult);
    }

    private static async Task SignalAsync(string pipeName)
    {
        await using var client = new System.IO.Pipes.NamedPipeClientStream(".", pipeName, System.IO.Pipes.PipeDirection.Out, System.IO.Pipes.PipeOptions.Asynchronous);
        await client.ConnectAsync(2000);
        await client.WriteAsync(new byte[] { 1 });
    }
}
