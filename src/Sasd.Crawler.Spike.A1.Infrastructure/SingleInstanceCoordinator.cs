using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace Sasd.Crawler.Spike.A1.Infrastructure;

/// <summary>Coordinates one process per Windows user with a mutex and local named-pipe activation.</summary>
public sealed class SingleInstanceCoordinator : IDisposable
{
    private readonly Mutex mutex;
    private readonly string pipeName;
    private bool ownsMutex;

    public SingleInstanceCoordinator(string applicationId, string? userIdentity = null)
    {
        var identity = userIdentity ?? $"{Environment.UserDomainName}\\{Environment.UserName}";
        var suffix = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..16];
        var safeId = new string(applicationId.Where(char.IsLetterOrDigit).ToArray());
        pipeName = $"{safeId}-{suffix}";
        mutex = new Mutex(false, $"Local\\{pipeName}");
    }

    public string PipeName => pipeName;

    public bool TryAcquirePrimary()
    {
        try { ownsMutex = mutex.WaitOne(0); }
        catch (AbandonedMutexException) { ownsMutex = true; }
        return ownsMutex;
    }

    public async Task SignalPrimaryAsync(CancellationToken cancellationToken)
    {
        await using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
        await client.ConnectAsync(2000, cancellationToken).ConfigureAwait(false);
        await client.WriteAsync(new byte[] { 1 }, cancellationToken).ConfigureAwait(false);
    }

    public async Task ListenOnceAsync(Func<Task> activation, CancellationToken cancellationToken)
    {
        await using var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
        var buffer = new byte[1];
        if (await server.ReadAsync(buffer, cancellationToken).ConfigureAwait(false) > 0)
            await activation().ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (ownsMutex) mutex.ReleaseMutex();
        mutex.Dispose();
    }
}
