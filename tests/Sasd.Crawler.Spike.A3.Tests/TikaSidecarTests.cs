using Sasd.Crawler.Spike.A3.Tika;

namespace Sasd.Crawler.Spike.A3.Tests;

public sealed class TikaSidecarTests : IDisposable
{
    private readonly string temporaryPath = Path.Combine(Path.GetTempPath(), "sasd-a3-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void EXT_001_Missing_java_executable_fails_before_process_start()
    {
        var options = ValidOptions() with { JavaExecutablePath = Path.Combine(temporaryPath, "missing-java.exe") };
        Assert.Throws<FileNotFoundException>(() => new TikaSidecar(options));
    }

    [Fact]
    public void EXT_001_Missing_server_jar_fails_before_process_start()
    {
        var options = ValidOptions() with { ServerJarPath = Path.Combine(temporaryPath, "missing-tika.jar") };
        Assert.Throws<FileNotFoundException>(() => new TikaSidecar(options));
    }

    [Fact]
    public async Task SEC_004_Oversized_input_is_rejected_without_starting_sidecar()
    {
        var options = ValidOptions() with { MaximumInputBytes = 4 };
        await using var sidecar = new TikaSidecar(options);
        var input = Path.Combine(temporaryPath, "input.bin");
        await File.WriteAllBytesAsync(input, new byte[5]);

        await Assert.ThrowsAsync<TikaInputTooLargeException>(() => sidecar.ExtractAsync(input));
        Assert.False(sidecar.IsRunning);
    }

    [Fact]
    public void SEC_004_Invalid_resource_limits_fail_explicitly()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TikaSidecar(ValidOptions() with { MaximumHeapMegabytes = 64 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TikaSidecar(ValidOptions() with { RequestTimeout = TimeSpan.Zero }));
    }

    public void Dispose()
    {
        if (Directory.Exists(temporaryPath)) Directory.Delete(temporaryPath, recursive: true);
    }

    private TikaSidecarOptions ValidOptions()
    {
        Directory.CreateDirectory(temporaryPath);
        var java = Path.Combine(temporaryPath, "java.exe");
        var jar = Path.Combine(temporaryPath, "tika.jar");
        File.WriteAllText(java, string.Empty);
        File.WriteAllText(jar, string.Empty);
        return new TikaSidecarOptions
        {
            JavaExecutablePath = java,
            ServerJarPath = jar,
            TemporaryRootPath = Path.Combine(temporaryPath, "isolated")
        };
    }
}
