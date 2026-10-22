using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Application.Tests.Sources;

/// <summary>
/// Tests source-catalog use cases independently from SQLite and WinForms.
/// </summary>
public sealed class SourceCatalogServiceTests
{
    [Fact]
    public async Task Src001_RegisterLocalDirectory_CreatesSingleCanonicalSource()
    {
        var repository = new InMemorySourceRepository();
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 10, 22, 18, 30, 0, TimeSpan.Zero));
        var service = new SourceCatalogService(repository, clock);
        var path = Path.Combine(Path.GetTempPath(), "SasdCrawler", "Documents");

        var first = await service.RegisterLocalDirectoryAsync(path, CancellationToken.None);
        var second = await service.RegisterLocalDirectoryAsync(
            path + Path.DirectorySeparatorChar,
            CancellationToken.None);

        Assert.True(first.Created);
        Assert.False(second.Created);
        Assert.Equal(first.Source.Id, second.Source.Id);
        Assert.Equal(first.Source.CanonicalLocation, second.Source.CanonicalLocation);
        Assert.Single(await service.GetSourcesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Src003_SetEnabled_PersistsChangedStateThroughRepositoryPort()
    {
        var repository = new InMemorySourceRepository();
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 10, 22, 18, 30, 0, TimeSpan.Zero));
        var service = new SourceCatalogService(repository, clock);
        var registered = await service.RegisterLocalDirectoryAsync(
            Path.Combine(Path.GetTempPath(), "SasdCrawler", "DisableMe"),
            CancellationToken.None);

        clock.UtcNow = clock.UtcNow.AddMinutes(1);
        await service.SetEnabledAsync(registered.Source.Id, isEnabled: false, CancellationToken.None);

        var source = Assert.Single(await service.GetSourcesAsync(CancellationToken.None));
        Assert.False(source.IsEnabled);
        Assert.Equal(1, repository.UpdateCount);
    }

    private sealed class FixedTimeProvider(DateTimeOffset initialUtc) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = initialUtc;

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }

    /// <summary>
    /// Small deterministic test double. Production persistence intentionally lives in the indexer project.
    /// </summary>
    private sealed class InMemorySourceRepository : ISourceRepository
    {
        private readonly List<SourceDefinition> sources = [];

        public int UpdateCount { get; private set; }

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<SourceDefinition>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<SourceDefinition>>(sources.ToArray());

        public Task<SourceDefinition?> GetByIdAsync(
            SourceId sourceId,
            CancellationToken cancellationToken)
            => Task.FromResult(sources.SingleOrDefault(source => source.Id == sourceId));

        public Task<SourceDefinition?> FindByCanonicalLocationAsync(
            string canonicalLocation,
            CancellationToken cancellationToken)
            => Task.FromResult(sources.SingleOrDefault(
                source => string.Equals(
                    source.CanonicalLocation,
                    canonicalLocation,
                    StringComparison.OrdinalIgnoreCase)));

        public Task<bool> TryAddAsync(SourceDefinition source, CancellationToken cancellationToken)
        {
            if (sources.Any(existing => string.Equals(
                existing.CanonicalLocation,
                source.CanonicalLocation,
                StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(false);
            }

            sources.Add(source);
            return Task.FromResult(true);
        }

        public Task UpdateAsync(SourceDefinition source, CancellationToken cancellationToken)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }
    }
}
