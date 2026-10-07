namespace Sasd.Crawler.Spike.A3.Tika;

public sealed record TikaExtractionResult(
    string Text,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Metadata);
