using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sasd.Crawler.Spike.A4.Media;

public sealed class WindowsDeviceChangeMonitor : IAsyncDisposable
{
    private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Thread thread;
    private DeviceMessageWindow? window;
    private bool started;

    public WindowsDeviceChangeMonitor()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        thread = new Thread(RunMessageLoop) { IsBackground = true, Name = "SASD volume device-change monitor" };
        thread.SetApartmentState(ApartmentState.STA);
    }

    public event EventHandler<VolumeTopologyChangedEventArgs>? TopologyChanged;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (started) return;
        started = true;
        thread.Start();
        await ready.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (!started) return;
        var handle = window?.Handle ?? IntPtr.Zero;
        if (handle != IntPtr.Zero) PostMessage(handle, WmClose, IntPtr.Zero, IntPtr.Zero);
        await Task.Run(thread.Join).ConfigureAwait(false);
        started = false;
    }

    private void RunMessageLoop()
    {
        window = new DeviceMessageWindow(OnTopologyChanged);
        window.CreateHandle(new CreateParams { Caption = "SASD Device Monitor", Parent = HwndMessage });
        ready.TrySetResult();
        Application.Run();
        window.DestroyHandle();
    }

    private void OnTopologyChanged(DeviceChangeKind kind) => TopologyChanged?.Invoke(this, new(kind));

    internal static DeviceChangeKind Classify(int eventCode) => eventCode switch
    {
        DbtDeviceArrival => DeviceChangeKind.Arrival,
        DbtDeviceRemoveComplete => DeviceChangeKind.Removal,
        DbtDevNodesChanged => DeviceChangeKind.TopologyChanged,
        _ => DeviceChangeKind.Ignored
    };

    private sealed class DeviceMessageWindow(Action<DeviceChangeKind> callback) : NativeWindow
    {
        protected override void WndProc(ref Message message)
        {
            if (message.Msg == WmDeviceChange)
            {
                var kind = Classify(message.WParam.ToInt32());
                if (kind != DeviceChangeKind.Ignored) callback(kind);
            }
            if (message.Msg == WmClose) Application.ExitThread();
            base.WndProc(ref message);
        }
    }

    private static readonly IntPtr HwndMessage = new(-3);
    private const int WmDeviceChange = 0x0219;
    private const int WmClose = 0x0010;
    private const int DbtDevNodesChanged = 0x0007;
    private const int DbtDeviceArrival = 0x8000;
    private const int DbtDeviceRemoveComplete = 0x8004;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
}

public enum DeviceChangeKind { Ignored, Arrival, Removal, TopologyChanged }
public sealed class VolumeTopologyChangedEventArgs(DeviceChangeKind kind) : EventArgs { public DeviceChangeKind Kind { get; } = kind; }
