using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

public static class TikaJarInventoryBuilder
{
    public static async Task<TikaJarInventoryResult> BuildAsync(
        string jarPath,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var jar = Path.GetFullPath(jarPath);
        if (!File.Exists(jar)) throw new FileNotFoundException("Tika server JAR does not exist.", jar);
        var output = Path.GetFullPath(outputPath);
        if (File.Exists(output) || Directory.Exists(output))
            throw new IOException($"Output path already exists; refusing to overwrite it: {output}");

        var components = new Dictionary<string, MavenComponent>(StringComparer.Ordinal);
        var licenseEntries = new List<string>();
        using (var archive = ZipFile.OpenRead(jar))
        {
            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (entry.FullName.StartsWith("META-INF/", StringComparison.OrdinalIgnoreCase) &&
                    (entry.Name.StartsWith("LICENSE", StringComparison.OrdinalIgnoreCase) ||
                     entry.Name.StartsWith("NOTICE", StringComparison.OrdinalIgnoreCase)))
                {
                    licenseEntries.Add(entry.FullName);
                }

                if (!entry.FullName.StartsWith("META-INF/maven/", StringComparison.Ordinal) ||
                    !entry.FullName.EndsWith("/pom.properties", StringComparison.Ordinal)) continue;
                using var reader = new StreamReader(entry.Open());
                var properties = ParseProperties(await reader.ReadToEndAsync(cancellationToken));
                if (!properties.TryGetValue("groupId", out var group) ||
                    !properties.TryGetValue("artifactId", out var name) ||
                    !properties.TryGetValue("version", out var version)) continue;
                var key = $"{group}:{name}:{version}";
                components[key] = new MavenComponent(group, name, version);
            }
        }

        await using var hashStream = File.OpenRead(jar);
        var sha512 = Convert.ToHexString(await SHA512.HashDataAsync(hashStream, cancellationToken)).ToLowerInvariant();
        var ordered = components.Values.OrderBy(component => component.Group).ThenBy(component => component.Name).ThenBy(component => component.Version).ToArray();
        var bom = new
        {
            bomFormat = "CycloneDX",
            specVersion = "1.5",
            serialNumber = $"urn:uuid:{Guid.NewGuid()}",
            version = 1,
            metadata = new
            {
                timestamp = DateTimeOffset.UtcNow,
                tools = new[] { new { vendor = "SASD", name = "TikaJarInventoryBuilder", version = "0.0.3-spike" } },
                properties = new[]
                {
                    new { name = "sasd.inventory.completeness", value = "partial-derived-from-embedded-maven-metadata" },
                    new { name = "sasd.inventory.source.sha512", value = sha512 }
                }
            },
            components = ordered.Select(component => new
            {
                type = "library",
                group = component.Group,
                name = component.Name,
                version = component.Version,
                purl = $"pkg:maven/{Uri.EscapeDataString(component.Group)}/{Uri.EscapeDataString(component.Name)}@{Uri.EscapeDataString(component.Version)}"
            })
        };
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        await File.WriteAllTextAsync(output, JsonSerializer.Serialize(bom, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
        return new TikaJarInventoryResult(output, ordered.Length, licenseEntries.Count, sha512, new FileInfo(output).Length);
    }

    private static Dictionary<string, string> ParseProperties(string content)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith('#')) continue;
            var separator = line.IndexOf('=');
            if (separator <= 0) continue;
            result[line[..separator].Trim()] = line[(separator + 1)..].Trim();
        }
        return result;
    }

    private sealed record MavenComponent(string Group, string Name, string Version);
}

public sealed record TikaJarInventoryResult(string OutputPath, int ComponentCount, int LicenseOrNoticeEntryCount, string JarSha512, long InventoryBytes);
