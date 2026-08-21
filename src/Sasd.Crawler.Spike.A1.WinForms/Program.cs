using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sasd.Crawler.Spike.A1.Core;
using Sasd.Crawler.Spike.A1.Infrastructure;

namespace Sasd.Crawler.Spike.A1.WinForms;

internal static class Program
{
    /// <summary>Starts the single-instance guard before any worker or SQLite writer is created.</summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var coordinator = new SingleInstanceCoordinator("SasdCrawlerSpikeA1");
        if (!coordinator.TryAcquirePrimary())
        {
            try { coordinator.SignalPrimaryAsync(CancellationToken.None).GetAwaiter().GetResult(); }
            catch (IOException) { /* The primary may be inside its short startup/shutdown window. */ }
            return;
        }

        var databasePath = Path.Combine(AppDataPaths.A1Directory, "lifecycle.db");
        using var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging => logging.AddConsole())
            .ConfigureServices(services =>
            {
                services.AddSingleton(coordinator);
                services.AddSingleton<IHeartbeatState, HeartbeatState>();
                services.AddSingleton<IHeartbeatStore>(_ => new SqliteHeartbeatStore(databasePath));
                services.AddSingleton<ActivationListener>();
                services.AddHostedService(provider => provider.GetRequiredService<ActivationListener>());
                services.AddSingleton<HeartbeatWorker>(provider => new HeartbeatWorker(
                    provider.GetRequiredService<IHeartbeatStore>(),
                    provider.GetRequiredService<IHeartbeatState>(),
                    provider.GetRequiredService<ILogger<HeartbeatWorker>>(),
                    TimeSpan.FromSeconds(3)));
                services.AddHostedService(provider => provider.GetRequiredService<HeartbeatWorker>());
                services.AddSingleton<MainForm>();
            })
            .Build();

        // WinForms owns this STA thread. Keeping mutex acquisition and release on it is required by Win32 mutex semantics.
        host.StartAsync().GetAwaiter().GetResult();
        Application.Run(host.Services.GetRequiredService<MainForm>());
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        host.StopAsync(timeout.Token).GetAwaiter().GetResult();
    }
}
