namespace Sasd.Crawler.WinForms;

/// <summary>
/// Resolves per-user storage paths for the Windows desktop application.
/// </summary>
internal static class AppPaths
{
    /// <summary>Gets the per-user directory used for durable SASD-Crawler application state.</summary>
    public static string DataDirectory
    {
        get
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "SASD-GmbH", "SASD-Crawler");
        }
    }

    /// <summary>Gets the SQLite control-store path.</summary>
    public static string ControlDatabasePath => Path.Combine(DataDirectory, "crawler.db");

    /// <summary>Gets the rebuildable per-user Lucene search-index directory.</summary>
    public static string SearchIndexDirectory => Path.Combine(DataDirectory, "search-index");
}
