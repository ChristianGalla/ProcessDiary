using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace ProcessDiary.Views;

/// <summary>A thin vertical grip that shows a horizontal resize cursor.</summary>
public sealed partial class ResizeGrip : Grid
{
    /// <summary>Initializes the grip.</summary>
    public ResizeGrip() => ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);
}
