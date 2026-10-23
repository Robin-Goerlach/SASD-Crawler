namespace Sasd.Crawler.Spike.A4.Media;

public sealed record VolumeIdentity(
    string MountPath,
    string VolumeGuidPath,
    uint VolumeSerialNumber,
    string FileSystem,
    long CapacityBytes,
    string Label,
    DriveType DriveType);

public sealed record RegisteredMedium(Guid MediaId, VolumeIdentity LastKnownIdentity);

public enum MediaMatchKind
{
    Exact,
    RequiresConfirmation,
    Ambiguous,
    NewMedium
}

public sealed record MediaMatchDecision(MediaMatchKind Kind, Guid? MediaId, IReadOnlyList<Guid> CandidateMediaIds, string Reason);
