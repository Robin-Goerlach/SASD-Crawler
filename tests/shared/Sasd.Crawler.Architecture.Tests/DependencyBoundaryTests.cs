using Sasd.Crawler.Application.Sources;
using Sasd.Crawler.Domain.Sources;
using Sasd.Crawler.Frontend.Main;
using Sasd.Crawler.Indexer.LocalFileSystem;
using Sasd.Crawler.Persistence.Sqlite.Sources;

namespace Sasd.Crawler.Architecture.Tests;

/// <summary>
/// Guards the first production dependency boundaries from ADR-0017.
/// </summary>
public sealed class DependencyBoundaryTests
{
    [Fact]
    public void Domain_HasNoDependencyOnOtherSasdCrawlerAssemblies()
    {
        var references = GetReferenceNames(typeof(SourceDefinition).Assembly);

        Assert.DoesNotContain(references, name => name.StartsWith("Sasd.Crawler.", StringComparison.Ordinal));
    }

    [Fact]
    public void Application_DoesNotDependOnFrontendOrIndexerImplementation()
    {
        var references = GetReferenceNames(typeof(SourceCatalogService).Assembly);

        Assert.DoesNotContain("Sasd.Crawler.Frontend", references);
        Assert.DoesNotContain("Sasd.Crawler.Persistence.Sqlite", references);
        Assert.DoesNotContain("Microsoft.Data.Sqlite", references);
    }

    [Fact]
    public void FrontendPresenterAssembly_DoesNotDependOnSqliteOrWinForms()
    {
        var references = GetReferenceNames(typeof(MainPresenter).Assembly);

        Assert.DoesNotContain("Microsoft.Data.Sqlite", references);
        Assert.DoesNotContain("System.Windows.Forms", references);
        Assert.DoesNotContain("Sasd.Crawler.Persistence.Sqlite", references);
    }

    [Fact]
    public void IndexerPersistence_DoesNotDependOnWinFormsFrontend()
    {
        var references = GetReferenceNames(typeof(SqliteSourceRepository).Assembly);

        Assert.DoesNotContain("System.Windows.Forms", references);
        Assert.DoesNotContain("Sasd.Crawler.Frontend", references);
    }

    [Fact]
    public void LocalFileSystemIndexer_DoesNotDependOnFrontendOrSqlite()
    {
        var references = GetReferenceNames(typeof(LocalFileSystemConnector).Assembly);

        Assert.DoesNotContain("System.Windows.Forms", references);
        Assert.DoesNotContain("Sasd.Crawler.Frontend", references);
        Assert.DoesNotContain("Sasd.Crawler.Persistence.Sqlite", references);
        Assert.DoesNotContain("Microsoft.Data.Sqlite", references);
    }

    private static string[] GetReferenceNames(System.Reflection.Assembly assembly)
        => assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
}
