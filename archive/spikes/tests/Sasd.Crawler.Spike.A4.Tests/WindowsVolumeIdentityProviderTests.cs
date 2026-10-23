using Sasd.Crawler.Spike.A4.Media;

namespace Sasd.Crawler.Spike.A4.Tests;

public sealed class WindowsVolumeIdentityProviderTests
{
    [Fact]
    public void USB_003_Ready_system_volume_exposes_native_identity_signals()
    {
        var root = Path.GetPathRoot(Environment.SystemDirectory)!;
        var result = new WindowsVolumeIdentityProvider().Read(root);
        Assert.Equal(root, result.MountPath, ignoreCase: true);
        Assert.StartsWith("\\\\?\\Volume{", result.VolumeGuidPath, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(result.FileSystem));
        Assert.True(result.CapacityBytes > 0);
    }
}
