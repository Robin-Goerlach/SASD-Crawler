using Sasd.Crawler.Application.Sources;

namespace Sasd.Crawler.Frontend.Main;

/// <summary>
/// View contract for the initial source-catalog desktop screen.
/// </summary>
/// <remarks>
/// The presenter speaks only in application models. No WinForms control type appears here, which keeps presenter
/// behavior independently testable and prevents accidental leakage of UI technology into application logic.
/// </remarks>
public interface IMainView
{
    /// <summary>Replaces the displayed source list.</summary>
    void ShowSources(IReadOnlyList<SourceSummary> sources);

    /// <summary>Shows whether a long-running UI operation is currently active.</summary>
    void SetBusy(bool isBusy);

    /// <summary>Shows a non-error informational message to the user.</summary>
    void ShowNotice(string message);
}
