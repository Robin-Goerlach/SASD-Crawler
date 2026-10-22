namespace Sasd.Crawler.Domain.Documents;

/// <summary>
/// Identifies one logical document independently of its current metadata and processing state.
/// </summary>
/// <remarks>
/// Using a dedicated value type prevents accidental interchange with <c>SourceId</c>, <c>ScanRunId</c> or
/// future media/job identifiers. The value is created once and survives metadata refreshes at the same canonical
/// location.
/// </remarks>
public readonly record struct DocumentId
{
    /// <summary>Creates a document identifier from an existing persisted value.</summary>
    /// <param name="value">The non-empty GUID value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
    public DocumentId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A document identifier must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Gets the underlying globally unique identifier.</summary>
    public Guid Value { get; }

    /// <summary>Creates a new document identifier.</summary>
    public static DocumentId New() => new(Guid.NewGuid());

    /// <summary>Parses a persisted document identifier.</summary>
    public static DocumentId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}
