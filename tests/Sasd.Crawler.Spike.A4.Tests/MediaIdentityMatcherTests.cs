using Sasd.Crawler.Spike.A4.Media;

namespace Sasd.Crawler.Spike.A4.Tests;

public sealed class MediaIdentityMatcherTests
{
    private readonly MediaIdentityMatcher matcher = new();

    [Fact]
    public void USB_003_Unique_exact_volume_guid_is_the_only_automatic_match()
    {
        var medium = Registered(Identity(guid: "guid-a"));
        var result = matcher.Match(Identity(guid: "GUID-A"), new[] { medium });
        Assert.Equal(MediaMatchKind.Exact, result.Kind);
        Assert.Equal(medium.MediaId, result.MediaId);
    }

    [Fact]
    public void USB_003_Changed_guid_with_matching_secondary_fingerprint_requires_confirmation()
    {
        var medium = Registered(Identity(guid: "old-guid"));
        var result = matcher.Match(Identity(guid: "new-guid"), new[] { medium });
        Assert.Equal(MediaMatchKind.RequiresConfirmation, result.Kind);
        Assert.Null(result.MediaId);
        Assert.Single(result.CandidateMediaIds);
    }

    [Fact]
    public void USB_003_Cloned_or_duplicate_identity_is_ambiguous_and_never_auto_merged()
    {
        var first = Registered(Identity(guid: "duplicate"));
        var second = Registered(Identity(guid: "duplicate"));
        var result = matcher.Match(Identity(guid: "duplicate"), new[] { first, second });
        Assert.Equal(MediaMatchKind.Ambiguous, result.Kind);
        Assert.Null(result.MediaId);
        Assert.Equal(2, result.CandidateMediaIds.Count);
    }

    [Fact]
    public void USB_003_Unknown_fingerprint_registers_a_new_medium()
    {
        var result = matcher.Match(Identity(guid: "new", serial: 2), new[] { Registered(Identity(guid: "old")) });
        Assert.Equal(MediaMatchKind.NewMedium, result.Kind);
    }

    private static RegisteredMedium Registered(VolumeIdentity identity) => new(Guid.NewGuid(), identity);
    private static VolumeIdentity Identity(string guid, uint serial = 1) => new("E:\\", guid, serial, "exFAT", 1_000_000, "ARCHIVE", DriveType.Removable);
}
