namespace Sasd.Crawler.Domain.Sources;

/// <summary>
/// Identifies a configured crawl source independently of its current path, drive letter or display name.
/// </summary>
/// <remarks>
/// A dedicated value type prevents accidental interchange of source identifiers with document, media or job
/// identifiers. The identifier is generated once and remains stable while mutable source metadata changes.
/// </remarks>
public readonly record struct SourceId
{
    /// <summary>
    /// Initializes a source identifier from an existing persisted <see cref="Guid"/>.
    /// </summary>
    /// <param name="value">The non-empty identifier value.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
    public SourceId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A source identifier must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Gets the underlying globally unique identifier.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new source identifier.
    /// </summary>
    /// <returns>A new non-empty identifier.</returns>
    public static SourceId New() => new(Guid.NewGuid());

    /// <summary>
    /// Parses the canonical persisted representation of a source identifier.
    /// </summary>
    /// <param name="value">A GUID string in any format accepted by <see cref="Guid.Parse(string)"/>.</param>
    /// <returns>The parsed source identifier.</returns>
    public static SourceId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc />
    public override string ToString() => Value.ToString("D");
}
