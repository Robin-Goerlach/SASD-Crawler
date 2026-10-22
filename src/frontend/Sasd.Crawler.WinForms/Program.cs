using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sasd.Crawler.Application.Sources;
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
                // Only the composition root is allowed to select the concrete indexer implementation. Forms and
                // presenters depend on ISourceCatalog and therefore remain isolated from SQLite.
                services.AddSingleton<TimeProvider>(TimeProvider.System);
                services.AddSingleton<ISourceRepository>(
                    _ => new SqliteSourceRepository(AppPaths.ControlDatabasePath));
                services.AddSingleton<ISourceCatalog, SourceCatalogService>();
                services.AddSingleton<MainForm>();
            })
            .Build();

        // WinForms requires the message loop to remain on this STA entry thread. Generic Host exposes an async
        // lifecycle API, so startup/shutdown use a narrow synchronous bridge here at the process boundary. No
        // application or UI I/O is performed synchronously by this bridge.
        host.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
        try
        {
            Application.Run(host.Services.GetRequiredService<MainForm>());
        }
        finally
        {
            using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            host.StopAsync(shutdownTimeout.Token).GetAwaiter().GetResult();
        }
    }
}
