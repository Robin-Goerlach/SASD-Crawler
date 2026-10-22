using Sasd.Crawler.Domain.Sources;

namespace Sasd.Crawler.Domain.Tests.Sources;

/// <summary>
/// Verifies the durable identity and lifecycle invariants of <see cref="SourceDefinition"/>.
/// </summary>
public sealed class SourceDefinitionTests
{
    [Fact]
    public void Id002_NewSource_HasStableNonEmptySourceId()
    {
        var now = new DateTimeOffset(2026, 10, 22, 18, 0, 0, TimeSpan.Zero);

        var source = SourceDefinition.CreateLocalDirectory(
            "Documents",
            "file:///C:/Documents/",
            now);

        Assert.NotEqual(Guid.Empty, source.Id.Value);
        Assert.Equal(SourceKind.LocalDirectory, source.Kind);
        Assert.True(source.IsEnabled);
        Assert.Equal(now, source.CreatedAtUtc);
        Assert.Equal(now, source.UpdatedAtUtc);
    }

    [Fact]
    public void Src003_SetEnabled_IsIdempotentAndOnlyAdvancesTimestampOnChange()
    {
        var created = new DateTimeOffset(2026, 10, 22, 18, 0, 0, TimeSpan.Zero);
        var changed = created.AddMinutes(5);
        var source = SourceDefinition.CreateLocalDirectory(
            "Documents",
            "file:///C:/Documents/",
            created);

        var unchanged = source.SetEnabled(isEnabled: true, changed);
        var disabled = source.SetEnabled(isEnabled: false, changed);

        Assert.False(unchanged);
        Assert.True(disabled);
        Assert.False(source.IsEnabled);
        Assert.Equal(changed, source.UpdatedAtUtc);
    }

    [Fact]
    public void Restore_RejectsPersistedStateWhoseUpdatePredatesCreation()
    {
        var created = new DateTimeOffset(2026, 10, 22, 18, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() => SourceDefinition.Restore(
            SourceId.New(),
            SourceKind.LocalDirectory,
            "Documents",
            "file:///C:/Documents/",
            isEnabled: true,
            created,
            created.AddSeconds(-1)));
    }
}
