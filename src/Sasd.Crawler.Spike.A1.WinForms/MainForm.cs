using Sasd.Crawler.Spike.A1.Core;
using Sasd.Crawler.Spike.A1.Infrastructure;

namespace Sasd.Crawler.Spike.A1.WinForms;

/// <summary>Minimal lifecycle view. Persistence, worker behavior and IPC remain outside the Form.</summary>
public sealed class MainForm : Form, IMainView
{
    private readonly Label statusValue = new() { AutoSize = true };
    private readonly Label heartbeatValue = new() { AutoSize = true };
    private readonly Button pauseButton = new() { Text = "Pause worker", AutoSize = true };
    private readonly NotifyIcon trayIcon;
    private readonly MainPresenter presenter;
    private bool exitRequested;

    public MainForm(IHeartbeatStore store, IHeartbeatState state, ActivationListener activationListener)
    {
        Text = "SASD Crawler - A1 Lifecycle Spike";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(520, 260);

        var minimizeButton = new Button { Text = "Minimize to tray", AutoSize = true };
        var readWriteButton = new Button { Text = "Write / read SQLite", AutoSize = true };
        var exitButton = new Button { Text = "Exit", AutoSize = true };
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        buttons.Controls.AddRange([readWriteButton, pauseButton, minimizeButton, exitButton]);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label { Text = "Application", AutoSize = true }, 0, 0);
        layout.Controls.Add(new Label { Text = "SASD Crawler A1", AutoSize = true }, 1, 0);
        layout.Controls.Add(new Label { Text = "Worker status", AutoSize = true }, 0, 1);
        layout.Controls.Add(statusValue, 1, 1);
        layout.Controls.Add(new Label { Text = "Latest heartbeat", AutoSize = true }, 0, 2);
        layout.Controls.Add(heartbeatValue, 1, 2);
        layout.Controls.Add(buttons, 0, 3);
        layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout);

        presenter = new MainPresenter(this, store, state, SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext());

        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Open", null, (_, _) => ActivateWindow());
        trayMenu.Items.Add("Pause / resume", null, (_, _) => presenter.TogglePause());
        trayMenu.Items.Add("Exit", null, (_, _) => ExitApplication());
        trayIcon = new NotifyIcon { Icon = SystemIcons.Application, Text = "SASD Crawler A1", Visible = true, ContextMenuStrip = trayMenu };
        trayIcon.DoubleClick += (_, _) => ActivateWindow();

        activationListener.ActivationRequested += (_, _) => presenter.Activate();
        Shown += async (_, _) => await presenter.RefreshAsync(CancellationToken.None);
        FormClosing += OnFormClosing;
        minimizeButton.Click += (_, _) => MinimizeToTray();
        pauseButton.Click += (_, _) => presenter.TogglePause();
        readWriteButton.Click += async (_, _) => await presenter.RefreshAsync(CancellationToken.None);
        exitButton.Click += (_, _) => ExitApplication();
    }

    public void ShowStatus(string workerStatus, string lastHeartbeat, bool paused)
    {
        statusValue.Text = workerStatus;
        heartbeatValue.Text = lastHeartbeat;
        pauseButton.Text = paused ? "Resume worker" : "Pause worker";
    }

    public void ActivateWindow() { Show(); WindowState = FormWindowState.Normal; Activate(); }
    private void MinimizeToTray() { Hide(); trayIcon.ShowBalloonTip(1000, "SASD Crawler", "A1 worker remains active.", ToolTipIcon.Info); }
    private void ExitApplication() { exitRequested = true; Close(); }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!exitRequested && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            MinimizeToTray();
            return;
        }
        trayIcon.Visible = false;
        trayIcon.Dispose();
        presenter.Dispose();
    }
}
