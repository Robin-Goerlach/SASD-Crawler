using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sasd.Crawler.Application.Crawling;
using Sasd.Crawler.Application.Jobs;
using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Indexer.LocalFileSystem;
using Sasd.Crawler.Indexer.Workers;
using Sasd.Crawler.Persistence.Sqlite;
using Sasd.Crawler.Persistence.Sqlite.Crawling;
using Sasd.Crawler.Persistence.Sqlite.Jobs;
using Sasd.Crawler.Persistence.Sqlite.Sources;

namespace Sasd.Crawler.WinForms;

/// <summary>
/// Windows desktop composition root.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Starts the Generic Host and WinForms message loop.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<TimeProvider>(TimeProvider.System);

                // A single control-store object owns schema migration and connection policy. Repositories share it
                // but still open short-lived connections for each operation.
                services.AddSingleton(_ => new SqliteControlStore(AppPaths.ControlDatabasePath));
                services.AddSingleton<ISourceRepository>(
                    provider => new SqliteSourceRepository(
                        provider.GetRequiredService<SqliteControlStore>()));
                services.AddSingleton<IDocumentRegistry>(
                    provider => new SqliteDocumentRegistry(
                        provider.GetRequiredService<SqliteControlStore>()));
                services.AddSingleton<ICrawlWorkQueue>(
                    provider => new SqliteCrawlWorkQueue(
                        provider.GetRequiredService<SqliteControlStore>()));

                services.AddSingleton<ISourceCatalog, SourceCatalogService>();
                services.AddSingleton<ICrawlJobService, CrawlJobService>();

                // The crawler is registered at the composition root but is never invoked directly from a Form.
                // Forms enqueue durable work; the hosted worker below owns filesystem traversal so synchronous
                // enumeration never becomes UI-thread work.
                services.AddSingleton<ILocalFileSystemConnector, LocalFileSystemConnector>();
                services.AddSingleton<ILocalSourceScanner, LocalSourceScanService>();

                // The hosted worker is the only production path that executes queued crawl work. Keeping it behind
                // Generic Host ensures WinForms remains presentation-only and shutdown cancellation flows through
                // the same host lifetime used by the rest of the application.
                services.AddSingleton(new CrawlWorkerOptions());
                services.AddSingleton<CrawlWorkItemProcessor>();
                services.AddHostedService<CrawlWorkBackgroundService>();

                services.AddSingleton<MainForm>();
            })
            .Build();

        // WinForms requires the message loop to remain on this STA entry thread. Generic Host exposes an async
        // lifecycle API, so startup/shutdown use a narrow synchronous bridge here at the process boundary. No
        // application or UI I/O is performed synchronously by this bridge.
        host.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
        try
        {
            System.Windows.Forms.Application.Run(host.Services.GetRequiredService<MainForm>());
        }
        finally
        {
            using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            host.StopAsync(shutdownTimeout.Token).GetAwaiter().GetResult();
        }
    }
}
