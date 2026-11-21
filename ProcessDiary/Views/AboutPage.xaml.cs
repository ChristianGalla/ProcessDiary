// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.UI.Xaml.Controls;
using ProcessDiary.Models;
using Windows.ApplicationModel;

namespace ProcessDiary.Views;

/// <summary>Shows information about ProcessDiary.</summary>
public sealed partial class AboutPage : Page
{
    private string? _license;
    private IReadOnlyList<ThirdPartyLicenseEntry>? _thirdPartyLicenses;

    /// <summary>Gets the ProcessDiary license text.</summary>
    public string License
    {
        get
        {
            if (_license is null)
            {
                string path = Path.Combine(AppContext.BaseDirectory, "LICENSE.txt");
                _license = File.Exists(path)
                    ? File.ReadAllText(path)
                    : "The ProcessDiary license file is not available.";
            }

            return _license;
        }
    }

    /// <summary>Gets third-party package license information included with ProcessDiary.</summary>
    public IReadOnlyList<ThirdPartyLicenseEntry> ThirdPartyLicenses
    {
        get
        {
            if (_thirdPartyLicenses is null)
            {
                var entries = new List<ThirdPartyLicenseEntry>();
                string licensesDirectory = Path.Combine(AppContext.BaseDirectory, "Licenses");
                string path = Path.Combine(licensesDirectory, "Licenses.json");

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var parsedEntries = JsonSerializer.Deserialize<List<ThirdPartyLicenseEntry>>(json);
                    if (parsedEntries is not null)
                    {
                        foreach (ThirdPartyLicenseEntry entry in parsedEntries)
                        {
                            if (!string.IsNullOrEmpty(entry.LicenseFile))
                            {
                                string licenseFilePath = Path.Combine(licensesDirectory, Path.GetFileName(entry.LicenseFile));
                                if (File.Exists(licenseFilePath))
                                {
                                    entry.LicenseFilePath = licenseFilePath;
                                }
                            }

                            entries.Add(entry);
                        }
                    }
                }

                _thirdPartyLicenses = entries;
            }

            return _thirdPartyLicenses;
        }
    }

    /// <summary>Initializes the page.</summary>
    public AboutPage()
    {
        string? version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        InitializeComponent();
        VersionTextBlock.Text = $"Version {version?.Split('+')[0] ?? "unknown"}";
        PackagingTextBlock.Text = $"Packaging: {(IsPackaged() ? "Packaged (MSIX)" : "Unpackaged")}";
        RuntimeTextBlock.Text = $"Runtime: {RuntimeInformation.FrameworkDescription}, " +
            (IsSelfContained() ? "self-contained" : "framework-dependent");
    }

    private async void LicenseFileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button { Tag: string licenseFilePath } && File.Exists(licenseFilePath))
        {
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(Path.GetFullPath(licenseFilePath));
            await Windows.System.Launcher.LaunchFileAsync(file);
        }
    }

    private static bool IsPackaged()
    {
        try
        {
            _ = Package.Current.Id;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    // A self-contained app ships its own copy of the runtime next to the executable.
    private static bool IsSelfContained() =>
        File.Exists(Path.Combine(AppContext.BaseDirectory, "coreclr.dll"));
}
