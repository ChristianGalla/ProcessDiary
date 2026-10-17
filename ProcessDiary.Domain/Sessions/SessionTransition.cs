using ProcessDiary.Domain.Entities;

namespace ProcessDiary.Domain.Sessions;

/// <summary>Result of evaluating a foreground change. Both members null means "no change".</summary>
/// <param name="Ended">The session that ended, if any.</param>
/// <param name="Started">The session that started, if any.</param>
public sealed record SessionTransition(ProcessSession? Ended, ProcessSession? Started)
{
    /// <summary>A transition that represents no change.</summary>
    public static readonly SessionTransition None = new(null, null);

    /// <summary>Gets a value indicating whether a session ended or started.</summary>
    public bool HasChange => Ended is not null || Started is not null;
}
