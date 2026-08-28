namespace Sasd.Crawler.Spike.A4.Media;

public sealed class MediaIdentityMatcher
{
    public MediaMatchDecision Match(VolumeIdentity attached, IReadOnlyCollection<RegisteredMedium> registered)
    {
        ArgumentNullException.ThrowIfNull(attached);
        ArgumentNullException.ThrowIfNull(registered);

        var exact = registered
            .Where(item => string.Equals(item.LastKnownIdentity.VolumeGuidPath, attached.VolumeGuidPath, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (exact.Length == 1)
            return new(MediaMatchKind.Exact, exact[0].MediaId, new[] { exact[0].MediaId }, "Unique exact Volume GUID match.");
        if (exact.Length > 1)
            return Ambiguous(exact, "The same Volume GUID is registered to multiple media; automatic merge is unsafe.");

        var fingerprint = registered.Where(item =>
            item.LastKnownIdentity.VolumeSerialNumber == attached.VolumeSerialNumber &&
            string.Equals(item.LastKnownIdentity.FileSystem, attached.FileSystem, StringComparison.OrdinalIgnoreCase) &&
            item.LastKnownIdentity.CapacityBytes == attached.CapacityBytes).ToArray();
        if (fingerprint.Length == 1)
            return new(MediaMatchKind.RequiresConfirmation, null, new[] { fingerprint[0].MediaId },
                "Serial, filesystem and capacity match, but the stable Volume GUID changed; user confirmation is required.");
        if (fingerprint.Length > 1)
            return Ambiguous(fingerprint, "Multiple media share the same secondary fingerprint; automatic merge is unsafe.");

        return new(MediaMatchKind.NewMedium, null, Array.Empty<Guid>(), "No registered stable identity or secondary fingerprint matches.");
    }

    private static MediaMatchDecision Ambiguous(IEnumerable<RegisteredMedium> candidates, string reason) =>
        new(MediaMatchKind.Ambiguous, null, candidates.Select(candidate => candidate.MediaId).ToArray(), reason);
}
