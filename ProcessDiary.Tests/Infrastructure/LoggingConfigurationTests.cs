using ProcessDiary.Infrastructure.Logging;

namespace ProcessDiary.Tests.Infrastructure;

/// <summary>Tests for locating the NLog configuration file.</summary>
public sealed class LoggingConfigurationTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "ProcessDiaryTests", Guid.NewGuid().ToString("N"));

    /// <summary>Creates a temporary folder.</summary>
    public LoggingConfigurationTests() => Directory.CreateDirectory(_folder);

    /// <inheritdoc />
    public void Dispose() => Directory.Delete(_folder, recursive: true);

    /// <summary>An existing explicit path wins over the default locations.</summary>
    [Fact]
    public void ResolveConfigFile_ExplicitPathExists_ReturnsIt()
    {
        var path = Path.Combine(_folder, "custom.config");
        File.WriteAllText(path, "<nlog />");

        Assert.Equal(path, LoggingBuilderExtensions.ResolveConfigFile(path));
    }

    /// <summary>A missing explicit path falls back to the shipped file in the application folder.</summary>
    [Fact]
    public void ResolveConfigFile_ExplicitPathMissing_FallsBackToShippedFile()
    {
        var result = LoggingBuilderExtensions.ResolveConfigFile(Path.Combine(_folder, "missing.config"));

        Assert.NotNull(result);
        Assert.EndsWith(LoggingBuilderExtensions.ConfigFileName, result);
    }
}
