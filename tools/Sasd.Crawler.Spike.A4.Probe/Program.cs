using System.Text.Json;
using Sasd.Crawler.Spike.A4.Media;

var provider = new WindowsVolumeIdentityProvider();
var results = new List<object>();
foreach (var drive in DriveInfo.GetDrives())
{
    if (!drive.IsReady)
    {
        results.Add(new { drive.Name, drive.DriveType, Ready = false });
        continue;
    }
    try
    {
        var identity = provider.Read(drive.RootDirectory.FullName);
        results.Add(new
        {
            drive.Name,
            drive.DriveType,
            Ready = true,
            identity.VolumeGuidPath,
            Serial = identity.VolumeSerialNumber.ToString("X8"),
            identity.FileSystem,
            identity.CapacityBytes,
            identity.Label
        });
    }
    catch (Exception exception)
    {
        results.Add(new { drive.Name, drive.DriveType, Ready = true, Error = exception.Message });
    }
}

await using (var monitor = new WindowsDeviceChangeMonitor())
{
    await monitor.StartAsync();
}

Console.WriteLine(JsonSerializer.Serialize(new
{
    Volumes = results,
    ReadyVolumeCount = DriveInfo.GetDrives().Count(drive => drive.IsReady),
    DeviceMonitorStartedAndStopped = true
}, new JsonSerializerOptions { WriteIndented = true }));
