using System.IO.Compression;
using System.Text.Json;

namespace Sasd.Crawler.Spike.A3.Tests;

public sealed class TikaJarInventoryBuilderTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "sasd-a3-inventory-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task SEC_006_Embedded_maven_components_are_deduplicated_and_hashed()
    {
        Directory.CreateDirectory(root);
        var jar = Path.Combine(root, "sample.jar");
        using (var archive = ZipFile.Open(jar, ZipArchiveMode.Create))
        {
            Add(archive, "META-INF/maven/example/component/pom.properties", "groupId=example\nartifactId=component\nversion=1.2.3");
            Add(archive, "META-INF/maven/example/component/pom.properties", "groupId=example\nartifactId=component\nversion=1.2.3");
            Add(archive, "META-INF/LICENSE", "license text");
        }
        var output = Path.Combine(root, "bom.json");

        var result = await TikaJarInventoryBuilder.BuildAsync(jar, output);

        Assert.Equal(1, result.ComponentCount);
        Assert.Equal(1, result.LicenseOrNoticeEntryCount);
        Assert.Equal(128, result.JarSha512.Length);
        using var json = JsonDocument.Parse(await File.ReadAllTextAsync(output));
        Assert.Equal("CycloneDX", json.RootElement.GetProperty("bomFormat").GetString());
        Assert.Single(json.RootElement.GetProperty("components").EnumerateArray());
    }

    [Fact]
    public async Task SEC_006_Inventory_builder_never_overwrites_existing_output()
    {
        Directory.CreateDirectory(root);
        var jar = Path.Combine(root, "sample.jar");
        using (ZipFile.Open(jar, ZipArchiveMode.Create)) { }
        var output = Path.Combine(root, "existing.json");
        await File.WriteAllTextAsync(output, "preserve");

        await Assert.ThrowsAsync<IOException>(() => TikaJarInventoryBuilder.BuildAsync(jar, output));
        Assert.Equal("preserve", await File.ReadAllTextAsync(output));
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    private static void Add(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open());
        writer.Write(content);
    }
}
