using Sasd.Crawler.Domain.Documents;
using Sasd.Crawler.Domain.Jobs;

namespace Sasd.Crawler.Application.Jobs;

/// <summary>
/// Describes one document-processing follow-up created by a completed discovery scan.
/// </summary>
public sealed record DocumentWorkRequest
{
    /// <summary>Creates a document work request for an indexing/deletion stage.</summary>
    public DocumentWorkRequest(DocumentId documentId, WorkItemStage stage)
    {
        if (stage is not (WorkItemStage.IndexDocument or WorkItemStage.DeleteFromSearchIndex))
        {
            throw new ArgumentOutOfRangeException(
                nameof(stage),
                "Document work may only use IndexDocument or DeleteFromSearchIndex.");
        }

        DocumentId = documentId;
        Stage = stage;
    }

    /// <summary>Gets the durable document identity.</summary>
    public DocumentId DocumentId { get; }

    /// <summary>Gets the processing stage.</summary>
    public WorkItemStage Stage { get; }
}
