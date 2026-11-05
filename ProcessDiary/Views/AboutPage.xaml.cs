using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace ProcessDiary.Views;

/// <summary>Shows information about ProcessDiary.</summary>
public sealed partial class AboutPage : Page
{
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
