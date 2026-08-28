namespace Sasd.Crawler.Spike.A4.Media;

public static class MediaRelativePath
{
    public static string FromAbsolutePath(string volumeMountPath, string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(volumeMountPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        var root = Path.GetFullPath(volumeMountPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(absolutePath);
        if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The document path is outside the media root.", nameof(absolutePath));
        return Path.GetRelativePath(root, candidate).Replace(Path.DirectorySeparatorChar, '/');
    }

    public static string ToAbsolutePath(string volumeMountPath, string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(volumeMountPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        if (Path.IsPathRooted(relativePath)) throw new ArgumentException("Media path must be relative.", nameof(relativePath));
        if (relativePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Any(segment => segment == ".."))
            throw new ArgumentException("Media path must not contain parent traversal segments.", nameof(relativePath));
        var root = Path.GetFullPath(volumeMountPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var combined = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!combined.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Media path escapes its root.", nameof(relativePath));
        return combined;
    }
}
