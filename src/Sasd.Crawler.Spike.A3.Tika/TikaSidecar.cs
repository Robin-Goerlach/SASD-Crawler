using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text.Json;

namespace Sasd.Crawler.Spike.A3.Tika;

public sealed class TikaSidecar : IAsyncDisposable
{
    private readonly TikaSidecarOptions options;
    private readonly SemaphoreSlim operationGate = new(1, 1);
    private readonly ConcurrentQueue<string> logs = new();
    private Process? process;
    private HttpClient? client;
    private string? processTemporaryPath;
    private Uri? loopbackEndpoint;
    private bool disposed;

    public TikaSidecar(TikaSidecarOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        options.Validate();
    }

    public bool IsRunning => process is { HasExited: false };
    public int? ProcessId => IsRunning ? process!.Id : null;
    public Uri? LoopbackEndpoint => loopbackEndpoint;

    public IReadOnlyList<string> GetRecentLogs() => logs.ToArray();

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            await EnsureStartedCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            operationGate.Release();
        }
    }

    public async Task<TikaExtractionResult> ExtractAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var file = new FileInfo(filePath);
        if (!file.Exists) throw new FileNotFoundException("Input document does not exist.", file.FullName);
        if (file.Length > options.MaximumInputBytes)
            throw new TikaInputTooLargeException(file.Length, options.MaximumInputBytes);

        await operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            await EnsureStartedCoreAsync(cancellationToken).ConfigureAwait(false);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.RequestTimeout);
            try
            {
                var text = await PutFileAsync("tika", file, "text/plain", timeout.Token).ConfigureAwait(false);
                var metadataJson = await PutFileAsync("meta", file, "application/json", timeout.Token).ConfigureAwait(false);
                return new TikaExtractionResult(text, ParseMetadata(metadataJson));
            }
            catch (OperationCanceledException)
            {
                await StopCoreAsync().ConfigureAwait(false);
                throw;
            }
            catch (HttpRequestException)
            {
                await StopCoreAsync().ConfigureAwait(false);
                throw;
            }
        }
        finally
        {
            operationGate.Release();
        }
    }

    public async Task RestartAsync(CancellationToken cancellationToken = default)
    {
        await operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            await StopCoreAsync().ConfigureAwait(false);
            await EnsureStartedCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            operationGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await operationGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (disposed) return;
            await StopCoreAsync().ConfigureAwait(false);
            disposed = true;
        }
        finally
        {
            operationGate.Release();
            operationGate.Dispose();
        }
    }

    private async Task EnsureStartedCoreAsync(CancellationToken cancellationToken)
    {
        if (IsRunning) return;
        await StopCoreAsync().ConfigureAwait(false);

        var port = ReserveLoopbackPort();
        processTemporaryPath = Path.Combine(options.TemporaryRootPath, $"tika-{Guid.NewGuid():N}");
        Directory.CreateDirectory(processTemporaryPath);
        var startInfo = new ProcessStartInfo(options.JavaExecutablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(options.ServerJarPath))!
        };
        startInfo.ArgumentList.Add($"-Xmx{options.MaximumHeapMegabytes}m");
        startInfo.ArgumentList.Add($"-Djava.io.tmpdir={Path.GetFullPath(processTemporaryPath)}");
        startInfo.ArgumentList.Add("-jar");
        startInfo.ArgumentList.Add(Path.GetFullPath(options.ServerJarPath));
        startInfo.ArgumentList.Add("--host");
        startInfo.ArgumentList.Add(IPAddress.Loopback.ToString());
        startInfo.ArgumentList.Add("--port");
        startInfo.ArgumentList.Add(port.ToString());

        process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += CaptureLog;
        process.ErrorDataReceived += CaptureLog;
        if (!process.Start()) throw new InvalidOperationException("Tika sidecar process did not start.");
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        loopbackEndpoint = new Uri($"http://127.0.0.1:{port}/");
        client = new HttpClient { BaseAddress = loopbackEndpoint };

        using var startup = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        startup.CancelAfter(options.StartupTimeout);
        try
        {
            while (true)
            {
                startup.Token.ThrowIfCancellationRequested();
                if (process.HasExited)
                    throw new TikaSidecarStartException($"Tika exited with code {process.ExitCode}. {string.Join(Environment.NewLine, GetRecentLogs())}");
                try
                {
                    using var response = await client.GetAsync("version", startup.Token).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode) return;
                }
                catch (HttpRequestException)
                {
                    // The loopback listener is not ready yet.
                }
                await Task.Delay(100, startup.Token).ConfigureAwait(false);
            }
        }
        catch
        {
            await StopCoreAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task<string> PutFileAsync(string endpoint, FileInfo file, string accept, CancellationToken cancellationToken)
    {
        using var stream = file.OpenRead();
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.ContentLength = file.Length;
        using var request = new HttpRequestMessage(HttpMethod.Put, endpoint) { Content = content };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
        request.Headers.TryAddWithoutValidation("Content-Disposition", $"attachment; filename=\"{SanitizeFileName(file.Name)}\"");
        using var response = await client!.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new TikaParserException((int)response.StatusCode, body);
        return body;
    }

    private async Task StopCoreAsync()
    {
        client?.Dispose();
        client = null;
        loopbackEndpoint = null;
        if (process is not null)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync().ConfigureAwait(false);
            }
            process.Dispose();
            process = null;
        }
        if (processTemporaryPath is not null)
        {
            var generatedPath = Path.GetFullPath(processTemporaryPath);
            var configuredRoot = Path.GetFullPath(options.TemporaryRootPath)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (generatedPath.StartsWith(configuredRoot, StringComparison.OrdinalIgnoreCase) && Directory.Exists(generatedPath))
            {
                try { Directory.Delete(generatedPath, recursive: true); }
                catch (IOException exception) { logs.Enqueue($"Temporary cleanup failed: {exception.Message}"); }
                catch (UnauthorizedAccessException exception) { logs.Enqueue($"Temporary cleanup failed: {exception.Message}"); }
            }
            processTemporaryPath = null;
        }
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseMetadata(string json)
    {
        using var document = JsonDocument.Parse(json);
        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            result[property.Name] = property.Value.ValueKind == JsonValueKind.Array
                ? property.Value.EnumerateArray().Select(value => value.ToString()).ToArray()
                : new[] { property.Value.ToString() };
        }
        return result;
    }

    private void CaptureLog(object sender, DataReceivedEventArgs eventArgs)
    {
        if (string.IsNullOrWhiteSpace(eventArgs.Data)) return;
        logs.Enqueue(eventArgs.Data);
        while (logs.Count > 200) logs.TryDequeue(out _);
    }

    private static int ReserveLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string SanitizeFileName(string value) =>
        string.Concat(value.Select(character => char.IsLetterOrDigit(character) || character is '.' or '-' or '_' ? character : '_'));

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);
}

public sealed class TikaInputTooLargeException(long actualBytes, long maximumBytes)
    : Exception($"Input contains {actualBytes} bytes; maximum is {maximumBytes} bytes.");

public sealed class TikaParserException(int statusCode, string responseBody)
    : Exception($"Tika returned HTTP {statusCode}: {responseBody}")
{
    public int StatusCode { get; } = statusCode;
}

public sealed class TikaSidecarStartException(string message) : Exception(message);
