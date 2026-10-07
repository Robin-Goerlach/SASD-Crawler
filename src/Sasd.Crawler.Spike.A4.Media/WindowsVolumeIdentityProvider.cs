using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Sasd.Crawler.Spike.A4.Media;

public sealed class WindowsVolumeIdentityProvider
{
    public VolumeIdentity Read(string mountPath)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows volume identity requires Windows.");
        ArgumentException.ThrowIfNullOrWhiteSpace(mountPath);
        var root = Path.GetPathRoot(Path.GetFullPath(mountPath));
        if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("A rooted volume path is required.", nameof(mountPath));
        root = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;

        var drive = new DriveInfo(root);
        if (!drive.IsReady) throw new IOException($"Volume {root} is not ready.");
        var volumeGuid = new StringBuilder(64);
        if (!GetVolumeNameForVolumeMountPoint(root, volumeGuid, volumeGuid.Capacity))
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot resolve volume GUID for {root}.");
        var label = new StringBuilder(261);
        var fileSystem = new StringBuilder(261);
        if (!GetVolumeInformation(root, label, label.Capacity, out var serial, out _, out _, fileSystem, fileSystem.Capacity))
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Cannot read volume information for {root}.");

        return new VolumeIdentity(root, volumeGuid.ToString(), serial, fileSystem.ToString(), drive.TotalSize, label.ToString(), drive.DriveType);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumeNameForVolumeMountPoint(string mountPoint, StringBuilder volumeName, int bufferLength);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumeInformation(
        string rootPathName,
        StringBuilder volumeNameBuffer,
        int volumeNameSize,
        out uint volumeSerialNumber,
        out uint maximumComponentLength,
        out uint fileSystemFlags,
        StringBuilder fileSystemNameBuffer,
        int fileSystemNameSize);
}
