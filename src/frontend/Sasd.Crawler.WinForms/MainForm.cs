using Microsoft.Extensions.Logging;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Frontend.Main;

namespace Sasd.Crawler.WinForms;

/// <summary>
/// Initial production WinForms shell for viewing and registering crawl sources.
/// </summary>
/// <remarks>
/// This form owns controls and Windows dialogs only. Source rules, canonicalization and persistence stay behind
/// <see cref="MainPresenter"/> and <see cref="ISourceCatalog"/>.
/// </remarks>
public sealed class MainForm : Form, IMainView
{
    private readonly ListView sourceList = new();
    private readonly Button addFolderButton = new();
    private readonly Button toggleEnabledButton = new();
    private readonly Button scanNowButton = new();
    private readonly Button refreshButton = new();
    private readonly Label statusLabel = new();
    private readonly MainPresenter presenter;
    private readonly ILogger<MainForm> logger;
    private readonly CancellationTokenSource lifetimeCancellation = new();
    private bool isBusy;

    /// <summary>Creates the production source-catalog window.</summary>
    public MainForm(
        ISourceCatalog sourceCatalog,
        ICrawlJobService crawlJobService,
        ILogger<MainForm> logger)
    {
        ArgumentNullException.ThrowIfNull(sourceCatalog);
        ArgumentNullException.ThrowIfNull(crawlJobService);
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Text = "SASD Crawler";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 480);

        ConfigureSourceList();
        ConfigureButtons();
        ConfigureLayout();

        presenter = new MainPresenter(this, sourceCatalog, crawlJobService);

        Shown += async (_, _) => await RunUiOperationAsync(
            cancellationToken => presenter.InitializeAsync(cancellationToken));
        FormClosed += (_, _) =>
        {
            lifetimeCancellation.Cancel();
            lifetimeCancellation.Dispose();
        };

        addFolderButton.Click += async (_, _) => await AddFolderAsync();
        refreshButton.Click += async (_, _) => await RunUiOperationAsync(
            cancellationToken => presenter.RefreshAsync(cancellationToken));
        toggleEnabledButton.Click += async (_, _) => await ToggleSelectedSourceAsync();
        scanNowButton.Click += async (_, _) => await QueueSelectedSourceScanAsync();
        sourceList.SelectedIndexChanged += (_, _) => UpdateSelectionCommands();
    }

    /// <inheritdoc />
    public void ShowSources(IReadOnlyList<SourceSummary> sources)
    {
        var selectedId = SelectedSource?.Id;

        sourceList.BeginUpdate();
        try
        {
            sourceList.Items.Clear();
            foreach (var source in sources)
            {
                var item = new ListViewItem(source.DisplayName)
                {
                    Tag = source,
                };
                item.SubItems.Add(source.Kind.ToString());
                item.SubItems.Add(source.DisplayLocation);
                item.SubItems.Add(source.IsEnabled ? "Enabled" : "Disabled");
                sourceList.Items.Add(item);

                if (selectedId.HasValue && source.Id == selectedId.Value)
                {
                    item.Selected = true;
                }
            }
        }
        finally
        {
            sourceList.EndUpdate();
        }

        UpdateSelectionCommands();
    }

    /// <inheritdoc />
    public void SetBusy(bool isBusy)
    {
        this.isBusy = isBusy;
        UseWaitCursor = isBusy;
        UpdateSelectionCommands();
    }

    /// <inheritdoc />
    public void ShowNotice(string message)
    {
        statusLabel.Text = message;
    }

    private SourceSummary? SelectedSource
        => sourceList.SelectedItems.Count == 1
            ? sourceList.SelectedItems[0].Tag as SourceSummary
            : null;

    private void ConfigureSourceList()
    {
        sourceList.Dock = DockStyle.Fill;
        sourceList.FullRowSelect = true;
        sourceList.HideSelection = false;
        sourceList.MultiSelect = false;
        sourceList.View = View.Details;

        sourceList.Columns.Add("Name", 180);
        sourceList.Columns.Add("Type", 120);
        sourceList.Columns.Add("Location", 390);
        sourceList.Columns.Add("State", 90);
    }

    private void ConfigureButtons()
    {
        addFolderButton.Text = "Add local folder...";
        addFolderButton.AutoSize = true;

        toggleEnabledButton.Text = "Disable";
        toggleEnabledButton.AutoSize = true;
        toggleEnabledButton.Enabled = false;

        scanNowButton.Text = "Scan now";
        scanNowButton.AutoSize = true;
        scanNowButton.Enabled = false;

        refreshButton.Text = "Refresh";
        refreshButton.AutoSize = true;
    }

    private void ConfigureLayout()
    {
        var title = new Label
        {
            AutoSize = true,
            Font = new Font(this.Font, FontStyle.Bold),
            Text = "Configured crawl sources",
        };

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
        };
        buttons.Controls.AddRange([addFolderButton, toggleEnabledButton, scanNowButton, refreshButton]);

        statusLabel.AutoEllipsis = true;
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.Text = "Starting...";

        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            RowCount = 4,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(sourceList, 0, 1);
        layout.Controls.Add(buttons, 0, 2);
        layout.Controls.Add(statusLabel, 0, 3);

        Controls.Add(layout);
    }

    private async Task AddFolderAsync()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select a local folder that SASD Crawler should index.",
            ShowNewFolderButton = false,
            UseDescriptionForTitle = true,
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await RunUiOperationAsync(
            cancellationToken => presenter.RegisterLocalDirectoryAsync(
                dialog.SelectedPath,
                cancellationToken));
    }

    private async Task ToggleSelectedSourceAsync()
    {
        var selected = SelectedSource;
        if (selected is null)
        {
            return;
        }

        await RunUiOperationAsync(
            cancellationToken => presenter.SetSourceEnabledAsync(
                selected.Id,
                !selected.IsEnabled,
                cancellationToken));
    }

    private async Task QueueSelectedSourceScanAsync()
    {
        var selected = SelectedSource;
        if (selected?.IsEnabled != true)
        {
            return;
        }

        // The presenter only persists a job request. The Generic Host background worker performs filesystem I/O,
        // so this event handler never crawls the selected directory on the WinForms UI thread.
        await RunUiOperationAsync(
            cancellationToken => presenter.QueueFullScanAsync(
                selected.Id,
                cancellationToken));
    }

    private void UpdateSelectionCommands()
    {
        var selected = SelectedSource;

        // ShowSources can run while a presenter operation still owns the busy state. Centralizing all enable/disable
        // decisions here prevents a list refresh from accidentally re-enabling commands before that operation ends.
        sourceList.Enabled = !isBusy;
        addFolderButton.Enabled = !isBusy;
        refreshButton.Enabled = !isBusy;
        toggleEnabledButton.Enabled = !isBusy && selected is not null;
        toggleEnabledButton.Text = selected?.IsEnabled == true ? "Disable" : "Enable";
        scanNowButton.Enabled = !isBusy && selected?.IsEnabled == true;
    }

    private async Task RunUiOperationAsync(Func<CancellationToken, Task> operation)
    {
        try
        {
            await operation(lifetimeCancellation.Token);
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested)
        {
            // Normal window shutdown: no user-facing error is needed.
        }
        catch (Exception exception)
        {
            // The UI boundary is the last safe place to prevent an expected storage/validation problem from
            // terminating the whole desktop process. We log the complete exception but show a compact message.
            logger.LogError(exception, "Source catalog operation failed.");
            MessageBox.Show(
                this,
                exception.Message,
                "SASD Crawler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
