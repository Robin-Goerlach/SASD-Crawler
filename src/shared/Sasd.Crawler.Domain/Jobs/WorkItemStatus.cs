namespace Sasd.Crawler.Domain.Jobs;

/// <summary>
/// Describes the durable state of an executable queue item.
/// </summary>
public enum WorkItemStatus
{
    /// <summary>The item is ready to be leased when its NextAttempt time is reached.</summary>
    Pending = 1,

    /// <summary>The item is owned by one worker until explicitly completed, failed or released.</summary>
    Leased = 2,

    /// <summary>The item completed its execution stage.</summary>
    Succeeded = 3,

    /// <summary>The item failed and is not automatically retried by the current milestone.</summary>
    Failed = 4,
}
