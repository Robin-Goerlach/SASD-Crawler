namespace Sasd.Crawler.Spike.A3.Tika;

public sealed record TikaSidecarOptions
{
    public required string JavaExecutablePath { get; init; }
    public required string ServerJarPath { get; init; }
    public required string TemporaryRootPath { get; init; }
    public long MaximumInputBytes { get; init; } = 50 * 1024 * 1024;
    public int MaximumHeapMegabytes { get; init; } = 512;
    public TimeSpan StartupTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

    internal void Validate()
    {
        if (!File.Exists(JavaExecutablePath))
            throw new FileNotFoundException("The configured Java executable does not exist.", JavaExecutablePath);
        if (!File.Exists(ServerJarPath))
            throw new FileNotFoundException("The configured Tika server JAR does not exist.", ServerJarPath);
        if (string.IsNullOrWhiteSpace(TemporaryRootPath))
            throw new ArgumentException("A temporary root is required.", nameof(TemporaryRootPath));
        if (MaximumInputBytes < 1) throw new ArgumentOutOfRangeException(nameof(MaximumInputBytes));
        if (MaximumHeapMegabytes is < 128 or > 4096) throw new ArgumentOutOfRangeException(nameof(MaximumHeapMegabytes));
        if (StartupTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(StartupTimeout));
        if (RequestTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(RequestTimeout));
    }
}
