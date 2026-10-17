using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProcessDiary.ViewModels;

namespace ProcessDiary.Views;

/// <summary>Lets the user choose the period of a report.</summary>
public sealed partial class FilterBar : UserControl
{
    /// <summary>Identifies the <see cref="Filter"/> property.</summary>
    public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
        nameof(Filter), typeof(FilterViewModel), typeof(FilterBar), new PropertyMetadata(null, OnFilterChanged));

    /// <summary>Initializes the control.</summary>
    public FilterBar()
    {
        InitializeComponent();
    }

    /// <summary>Gets or sets the period selection shown by the control.</summary>
    public FilterViewModel? Filter
    {
        get => (FilterViewModel?)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((FilterBar)d).Bindings.Update();
}
