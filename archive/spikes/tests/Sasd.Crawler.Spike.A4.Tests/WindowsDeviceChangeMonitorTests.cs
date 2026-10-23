using Sasd.Crawler.Spike.A4.Media;

namespace Sasd.Crawler.Spike.A4.Tests;

public sealed class WindowsDeviceChangeMonitorTests
{
    [Theory]
    [InlineData(0x8000, DeviceChangeKind.Arrival)]
    [InlineData(0x8004, DeviceChangeKind.Removal)]
    [InlineData(0x0007, DeviceChangeKind.TopologyChanged)]
    [InlineData(1234, DeviceChangeKind.Ignored)]
    public void USB_002_Device_messages_are_classified_without_form_code(int code, DeviceChangeKind expected) =>
        Assert.Equal(expected, WindowsDeviceChangeMonitor.Classify(code));

    [Fact]
    public async Task USB_002_Independent_message_window_starts_and_stops_cleanly()
    {
        await using var monitor = new WindowsDeviceChangeMonitor();
        await monitor.StartAsync();
    }
}
