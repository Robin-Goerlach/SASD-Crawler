using System.Diagnostics;

public static class TikaRuntimeBuilder
{
    private static readonly string[] Modules =
    {
        "java.base", "java.compiler", "java.desktop", "java.logging", "java.management",
        "java.naming", "java.net.http", "java.prefs", "java.rmi", "java.scripting",
        "java.security.jgss", "java.security.sasl", "java.sql", "java.sql.rowset",
        "java.transaction.xa", "java.xml", "java.xml.crypto", "jdk.charsets",
        "jdk.crypto.cryptoki", "jdk.crypto.ec", "jdk.httpserver", "jdk.localedata",
        "jdk.management", "jdk.unsupported", "jdk.zipfs"
    };

    public static async Task<RuntimeBuildResult> BuildAsync(string javaHome, string outputPath, CancellationToken cancellationToken = default)
    {
        var jlink = Path.GetFullPath(Path.Combine(javaHome, "bin", "jlink.exe"));
        if (!File.Exists(jlink)) throw new FileNotFoundException("jlink.exe was not found below JavaHome.", jlink);
        var output = Path.GetFullPath(outputPath);
        if (Directory.Exists(output) || File.Exists(output))
            throw new IOException($"Output path already exists; refusing to overwrite it: {output}");

        var startInfo = new ProcessStartInfo(jlink)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("--add-modules");
        startInfo.ArgumentList.Add(string.Join(',', Modules));
        startInfo.ArgumentList.Add("--strip-debug");
        startInfo.ArgumentList.Add("--no-man-pages");
        startInfo.ArgumentList.Add("--no-header-files");
        startInfo.ArgumentList.Add("--compress=zip-6");
        startInfo.ArgumentList.Add("--output");
        startInfo.ArgumentList.Add(output);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("jlink did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"jlink failed with code {process.ExitCode}: {await stderr}");

        var files = Directory.EnumerateFiles(output, "*", SearchOption.AllDirectories).Select(path => new FileInfo(path)).ToArray();
        var listInfo = new ProcessStartInfo(Path.Combine(output, "bin", "java.exe"), "--list-modules")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        using var listProcess = Process.Start(listInfo) ?? throw new InvalidOperationException("Reduced java runtime did not start.");
        var moduleOutput = await listProcess.StandardOutput.ReadToEndAsync(cancellationToken);
        await listProcess.WaitForExitAsync(cancellationToken);
        if (listProcess.ExitCode != 0) throw new InvalidOperationException("Reduced java runtime could not list its modules.");
        var moduleCount = moduleOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        return new RuntimeBuildResult(output, moduleCount, files.Length, files.Sum(file => file.Length), await stdout, await stderr);
    }
}

public sealed record RuntimeBuildResult(string OutputPath, int ResolvedModuleCount, int FileCount, long Bytes, string StandardOutput, string StandardError);
