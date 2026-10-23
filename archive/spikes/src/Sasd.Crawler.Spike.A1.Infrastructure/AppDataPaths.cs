namespace Sasd.Crawler.Spike.A1.Infrastructure;

/// <summary>Resolves the spike-specific per-user storage boundary.</summary>
public static class AppDataPaths
{
    public static string A1Directory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SASD", "Crawler", "spikes", "a1");
}
