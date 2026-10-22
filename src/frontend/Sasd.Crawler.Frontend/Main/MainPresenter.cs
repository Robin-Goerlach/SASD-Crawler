using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Frontend.Main;

/// <summary>
/// Coordinates the source-catalog view with UI-neutral application services.
/// </summary>
/// <remarks>
/// The presenter contains presentation flow but no WinForms code. This distinction is important for ADR-0017:
/// the same presenter can be tested without starting a message loop and cannot reach SQLite directly.
/// </remarks>
public sealed class MainPresenter
{
    private readonly IMainView view;
    private readonly ISourceCatalog sourceCatalog;

    /// <summary>Initializes a presenter for the supplied view and source application contract.</summary>
    public MainPresenter(IMainView view, ISourceCatalog sourceCatalog)
    {
        this.view = view ?? throw new ArgumentNullException(nameof(view));
        this.sourceCatalog = sourceCatalog ?? throw new ArgumentNullException(nameof(sourceCatalog));
    }

    /// <summary>Initializes durable source storage and renders the current catalog.</summary>
    public Task InitializeAsync(CancellationToken cancellationToken)
        => ExecuteBusyAsync(
            async () =>
            {
                await sourceCatalog.InitializeAsync(cancellationToken);
                await RefreshCoreAsync(cancellationToken);
            });

    /// <summary>Refreshes the displayed sources from durable application state.</summary>
    public Task RefreshAsync(CancellationToken cancellationToken)
        => ExecuteBusyAsync(() => RefreshCoreAsync(cancellationToken));

    /// <summary>Registers a local directory and refreshes the view.</summary>
    public Task RegisterLocalDirectoryAsync(string directoryPath, CancellationToken cancellationToken)
        => ExecuteBusyAsync(
            async () =>
            {
                var result = await sourceCatalog.RegisterLocalDirectoryAsync(directoryPath, cancellationToken);
                if (!result.Created)
                {
                    view.ShowNotice($"'{result.Source.DisplayLocation}' is already registered.");
                }
                else
                {
                    view.ShowNotice($"Added source '{result.Source.DisplayName}'.");
                }

                await RefreshCoreAsync(cancellationToken);
            });

    /// <summary>Changes source enabled state and refreshes the view.</summary>
    public Task SetSourceEnabledAsync(
        SourceId sourceId,
        bool isEnabled,
        CancellationToken cancellationToken)
        => ExecuteBusyAsync(
            async () =>
            {
                await sourceCatalog.SetEnabledAsync(sourceId, isEnabled, cancellationToken);
                await RefreshCoreAsync(cancellationToken);
            });

    private async Task ExecuteBusyAsync(Func<Task> operation)
    {
        view.SetBusy(true);
        try
        {
            await operation();
        }
        finally
        {
            // Busy state is presentation state, so it must be restored even when storage or validation fails.
            // The WinForms boundary decides how to log/display the exception.
            view.SetBusy(false);
        }
    }

    private async Task RefreshCoreAsync(CancellationToken cancellationToken)
    {
        var sources = await sourceCatalog.GetSourcesAsync(cancellationToken);
        view.ShowSources(sources);
    }
}
