namespace Sasd.Crawler.Domain.Jobs;

/// <summary>
/// Identifies the processing stage represented by a durable work item.
/// </summary>
public enum WorkItemStage
{
    /// <summary>Enumerate one local source and update the durable document registry.</summary>
    LocalFullScan = 1,

    /// <summary>Extract one current document and upsert it into the search index.</summary>
    IndexDocument = 2,

    /// <summary>Remove one confirmed-missing document from the current search index.</summary>
    DeleteFromSearchIndex = 3,
}
