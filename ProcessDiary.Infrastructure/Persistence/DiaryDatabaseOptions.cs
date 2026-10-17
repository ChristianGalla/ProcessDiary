namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>Location of the diary database.</summary>
/// <param name="DatabasePath">Full path of the SQLite database file.</param>
public sealed record DiaryDatabaseOptions(string DatabasePath)
{
    /// <summary>Gets the default options, with the database in the local application data folder.</summary>
    public static DiaryDatabaseOptions Default { get; } = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ProcessDiary",
        "processdiary.db"));
}
