using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Frontend.Main;

namespace Sasd.Crawler.Frontend.Tests.Main;

/// <summary>
/// Verifies presenter behavior without WinForms, SQLite or a Windows message loop.
/// </summary>
public sealed class MainPresenterTests
{
    [Fact]
    public async Task Initialize_LoadsSourcesAndRestoresBusyState()
    {
        var source = new SourceSummary(
            SourceId.New(),
            SourceKind.LocalDirectory,
            "Documents",
            "file:///C:/Documents/",
            @"C:\Documents\",
            IsEnabled: true);
        var catalog = new FakeSourceCatalog([source]);
        var view = new RecordingMainView();
        var presenter = new MainPresenter(view, catalog);

        await presenter.InitializeAsync(CancellationToken.None);

        Assert.True(catalog.InitializeCalled);
        Assert.Equal([true, false], view.BusyStates);
        Assert.Equal(source, Assert.Single(view.Sources));
    }

    [Fact]
    public async Task RegisterExistingSource_ShowsNoticeAndRefreshes()
    {
        var source = new SourceSummary(
            SourceId.New(),
            SourceKind.LocalDirectory,
            "Documents",
            "file:///C:/Documents/",
            @"C:\Documents\",
            IsEnabled: true);
        var catalog = new FakeSourceCatalog([source])
        {
            RegisterResult = new RegisterLocalSourceResult(source, Created: false),
        };
        var view = new RecordingMainView();
        var presenter = new MainPresenter(view, catalog);

        await presenter.RegisterLocalDirectoryAsync(@"C:\Documents", CancellationToken.None);

        Assert.Contains("already registered", view.Notice, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(source, Assert.Single(view.Sources));
        Assert.Equal([true, false], view.BusyStates);
    }

    private sealed class RecordingMainView : IMainView
    {
        public IReadOnlyList<SourceSummary> Sources { get; private set; } = [];
        public List<bool> BusyStates { get; } = [];
        public string Notice { get; private set; } = string.Empty;

        public void ShowSources(IReadOnlyList<SourceSummary> sources) => Sources = sources;
        public void SetBusy(bool isBusy) => BusyStates.Add(isBusy);
        public void ShowNotice(string message) => Notice = message;
    }

    private sealed class FakeSourceCatalog(IReadOnlyList<SourceSummary> initialSources) : ISourceCatalog
    {
        public bool InitializeCalled { get; private set; }
        public RegisterLocalSourceResult? RegisterResult { get; set; }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            InitializeCalled = true;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SourceSummary>> GetSourcesAsync(CancellationToken cancellationToken)
            => Task.FromResult(initialSources);

        public Task<RegisterLocalSourceResult> RegisterLocalDirectoryAsync(
            string directoryPath,
            CancellationToken cancellationToken)
            => Task.FromResult(
                RegisterResult
                ?? throw new InvalidOperationException("The test did not configure a registration result."));

        public Task SetEnabledAsync(
            SourceId sourceId,
            bool isEnabled,
            CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
