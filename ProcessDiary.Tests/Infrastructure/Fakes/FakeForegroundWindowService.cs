using ProcessDiary.Domain.Abstractions;

namespace ProcessDiary.Tests.Infrastructure.Fakes;

/// <summary>Foreground window service whose result is set by the test.</summary>
internal sealed class FakeForegroundWindowService : IForegroundWindowService
{
    /// <summary>Gets or sets the window returned by <see cref="GetCurrent"/>.</summary>
    public ForegroundWindowInfo? Current { get; set; }

    /// <inheritdoc />
    public ForegroundWindowInfo? GetCurrent() => Current;
}
