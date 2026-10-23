namespace Sasd.Crawler.Domain.Jobs;

/// <summary>
/// Identifies one durable execution item belonging to a crawl job.
/// </summary>
public readonly record struct WorkItemId
{
    /// <summary>Creates a work-item identifier from an existing persisted value.</summary>
    public WorkItemId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A work-item identifier must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Gets the underlying globally unique identifier.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new work-item identifier.</summary>
    public static WorkItemId New() => new(Guid.NewGuid());

    /// <summary>Parses a persisted work-item identifier.</summary>
    public static WorkItemId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}
