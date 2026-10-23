namespace Sasd.Crawler.Domain.Jobs;

/// <summary>
/// Identifies the processing stage represented by a durable work item.
/// </summary>
/// <remarks>
/// Version 0.1 starts with source discovery as one stage. Later extraction/index stages can be added without
/// changing the job-vs-work-item distinction.
/// </remarks>
public enum WorkItemStage
{
    /// <summary>Enumerate one local source and update the durable document registry.</summary>
    LocalFullScan = 1,
}
