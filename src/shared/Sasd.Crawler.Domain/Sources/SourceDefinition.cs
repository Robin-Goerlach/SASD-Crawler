namespace Sasd.Crawler.Domain.Sources;

/// <summary>
/// Represents one configured source whose content may be crawled and indexed.
/// </summary>
/// <remarks>
/// This type contains only durable source identity and lifecycle state. It deliberately has no knowledge of
/// SQLite, WinForms, Lucene or a concrete connector. Keeping the aggregate infrastructure-neutral is the first
/// production enforcement of the shared/frontend/indexer boundary from ADR-0017.
/// </remarks>
public sealed class SourceDefinition
{
    private SourceDefinition(
        SourceId id,
        SourceKind kind,
        string displayName,
        string canonicalLocation,
        bool isEnabled,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("A source display name is required.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(canonicalLocation))
        {
            throw new ArgumentException("A canonical source location is required.", nameof(canonicalLocation));
        }

        if (updatedAtUtc < createdAtUtc)
        {
            throw new ArgumentException("UpdatedAtUtc must not be earlier than CreatedAtUtc.", nameof(updatedAtUtc));
        }

        Id = id;
        Kind = kind;
        DisplayName = displayName.Trim();
        CanonicalLocation = canonicalLocation.Trim();
        IsEnabled = isEnabled;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>Gets the stable internal source identifier.</summary>
    public SourceId Id { get; }

    /// <summary>Gets the logical source kind used to select an appropriate connector.</summary>
    public SourceKind Kind { get; }

    /// <summary>Gets the human-readable source name presented to the user.</summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the canonical location used to distinguish the source from another configured source.
    /// </summary>
    /// <remarks>
    /// For local directories the application layer stores an absolute file URI. The domain intentionally treats
    /// this as an opaque canonical value because normalization rules are source-type and platform specific.
    /// </remarks>
    public string CanonicalLocation { get; }

    /// <summary>Gets whether scheduled and manual crawl operations may process this source.</summary>
    public bool IsEnabled { get; private set; }

    /// <summary>Gets when this source definition was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>Gets when mutable source state was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Creates a newly registered local-directory source.
    /// </summary>
    /// <param name="displayName">Human-readable source name.</param>
    /// <param name="canonicalLocation">Canonical absolute file URI produced by the application layer.</param>
    /// <param name="createdAtUtc">Creation timestamp.</param>
    /// <returns>A new enabled local-directory source.</returns>
    public static SourceDefinition CreateLocalDirectory(
        string displayName,
        string canonicalLocation,
        DateTimeOffset createdAtUtc)
        => new(
            SourceId.New(),
            SourceKind.LocalDirectory,
            displayName,
            canonicalLocation,
            isEnabled: true,
            createdAtUtc,
            createdAtUtc);

    /// <summary>
    /// Rehydrates a source from durable storage without inventing new identity or timestamps.
    /// </summary>
    /// <remarks>
    /// Persistence adapters use this factory so reconstructing a source does not accidentally run "new source"
    /// behavior. Validation still runs to keep corrupt persisted state from silently entering the domain model.
    /// </remarks>
    public static SourceDefinition Restore(
        SourceId id,
        SourceKind kind,
        string displayName,
        string canonicalLocation,
        bool isEnabled,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
        => new(id, kind, displayName, canonicalLocation, isEnabled, createdAtUtc, updatedAtUtc);

    /// <summary>
    /// Changes whether the source may be crawled.
    /// </summary>
    /// <param name="isEnabled">The requested enabled state.</param>
    /// <param name="changedAtUtc">Timestamp of the state change.</param>
    /// <returns><see langword="true"/> when the state changed; otherwise <see langword="false"/>.</returns>
    public bool SetEnabled(bool isEnabled, DateTimeOffset changedAtUtc)
    {
        if (IsEnabled == isEnabled)
        {
            // An idempotent request must not manufacture a new UpdatedAt timestamp. This keeps repeated UI or
            // scheduler commands deterministic and avoids unnecessary writes.
            return false;
        }

        if (changedAtUtc < CreatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(changedAtUtc),
                "A source state change cannot predate source creation.");
        }

        IsEnabled = isEnabled;
        UpdatedAtUtc = changedAtUtc;
        return true;
    }
}
