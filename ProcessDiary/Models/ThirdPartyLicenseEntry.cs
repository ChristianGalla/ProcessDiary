// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.UI.Xaml;

namespace ProcessDiary.Models;

/// <summary>Describes the license information for a third-party package.</summary>
public sealed class ThirdPartyLicenseEntry
{
    public string? PackageId { get; set; }

    public string? PackageVersion { get; set; }

    public string? Authors { get; set; }

    public string? PackageProjectUrl { get; set; }

    public string? License { get; set; }

    public string? LicenseFile { get; set; }

    [JsonIgnore]
    public string? LicenseFilePath { get; set; }

    [JsonIgnore]
    public Uri? PackageProjectUri => Uri.TryCreate(PackageProjectUrl, UriKind.Absolute, out Uri? uri) ? uri : null;

    [JsonIgnore]
    public bool HasLicenseFile => !string.IsNullOrEmpty(LicenseFilePath);

    [JsonIgnore]
    public Visibility LicenseFileVisibility => HasLicenseFile ? Visibility.Visible : Visibility.Collapsed;

    [JsonIgnore]
    public bool HasPackageProjectUrl => !string.IsNullOrEmpty(PackageProjectUrl);

    [JsonIgnore]
    public Visibility PackageProjectVisibility => HasPackageProjectUrl ? Visibility.Visible : Visibility.Collapsed;
}
