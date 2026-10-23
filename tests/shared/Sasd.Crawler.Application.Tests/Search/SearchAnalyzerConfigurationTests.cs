using Sasd.Crawler.Application.Search;

namespace Sasd.Crawler.Application.Tests.Search;

/// <summary>
/// Guards the backend-independent analyzer configuration contract.
/// </summary>
public sealed class SearchAnalyzerConfigurationTests
{
    [Fact]
    public void DefaultConfiguration_IsVersionedAndConservative()
    {
        var configuration = SearchAnalyzerConfiguration.CreateDefault();

        Assert.False(string.IsNullOrWhiteSpace(configuration.Version));
        Assert.Contains("der", configuration.GermanStopWords);
        Assert.Contains("die", configuration.GermanStopWords);
        Assert.Contains("das", configuration.GermanStopWords);
        Assert.DoesNotContain("ja", configuration.GermanStopWords);
        Assert.DoesNotContain("nein", configuration.GermanStopWords);
        Assert.Contains("the", configuration.EnglishStopWords);
    }

    [Fact]
    public void ExplicitConfiguration_NormalizesAndDeduplicatesWords()
    {
        var configuration = new SearchAnalyzerConfiguration(
            " custom-v1 ",
            [" JA ", "ja", "NEIN"],
            [" THE ", "the"]);

        Assert.Equal("custom-v1", configuration.Version);
        Assert.Equal(["ja", "nein"], configuration.GermanStopWords);
        Assert.Equal(["the"], configuration.EnglishStopWords);
    }
}
