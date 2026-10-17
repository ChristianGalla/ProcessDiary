using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace ProcessDiary.Infrastructure.Logging;

/// <summary>Configures NLog as the logging provider, driven by an external configuration file.</summary>
public static class LoggingBuilderExtensions
{
    /// <summary>The name of the NLog configuration file.</summary>
    public const string ConfigFileName = "nlog.config";

    /// <summary>
    /// Uses NLog with settings read from a configuration file. The file is searched in this order:
    /// the explicit path, <c>%LOCALAPPDATA%\ProcessDiary\nlog.config</c>, then the application folder.
    /// The file is reloaded when it changes, so no rebuild is needed.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="configFilePath">Optional explicit path of the configuration file.</param>
    /// <returns>The same builder.</returns>
    public static ILoggingBuilder AddProcessDiaryLogging(this ILoggingBuilder builder, string? configFilePath = null)
    {
        var path = ResolveConfigFile(configFilePath);
        if (path is not null)
        {
            LogManager.Setup().LoadConfigurationFromFile(path);
        }

        // Without a configuration file NLog has no targets, so nothing is written anywhere.
        builder.ClearProviders();
        builder.AddNLog();
        return builder;
    }

    /// <summary>Finds the configuration file that applies.</summary>
    /// <param name="configFilePath">Optional explicit path.</param>
    /// <returns>The full path of the file, or null when none exists.</returns>
    internal static string? ResolveConfigFile(string? configFilePath)
    {
        var candidates = new[]
        {
            configFilePath,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProcessDiary", ConfigFileName),
            Path.Combine(AppContext.BaseDirectory, ConfigFileName)
        };

        return candidates.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p));
    }
}
