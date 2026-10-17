using ProcessDiary.Domain.Entities;
using ProcessDiary.Infrastructure.Persistence;

namespace ProcessDiary.Tests.Infrastructure;

/// <summary>Integration tests for the SQLite repositories.</summary>
public sealed class SqliteRepositoryTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Noon = new(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly TestDatabase _database = new();
    private ProcessSessionRepository _sessions = null!;
    private ScreenshotRepository _screenshots = null!;
    private ApplicationMetadataRepository _metadata = null!;

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        _sessions = new ProcessSessionRepository(_database.Factory);
        _screenshots = new ScreenshotRepository(_database.Factory);
        _metadata = new ApplicationMetadataRepository(_database.Factory);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DisposeAsync() => await _database.DisposeAsync();

    private static ProcessSession Session(string name, int startMinute, int? endMinute) =>
        new(Guid.NewGuid(), name, $@"C:\apps\{name}", "title", Noon.AddMinutes(startMinute),
            endMinute is { } end ? Noon.AddMinutes(end) : null);

    /// <summary>A stored session is returned with its application name and times.</summary>
    [Fact]
    public async Task AddAsync_StoresSessionAndReturnsItInRange()
    {
        var session = Session("code.exe", 0, 10);
        await _sessions.AddAsync(session, CancellationToken.None);

        var result = await _sessions.GetOverlappingAsync(Noon.AddMinutes(-1), Noon.AddHours(1), CancellationToken.None);

        var stored = Assert.Single(result);
        Assert.Equal("code.exe", stored.ProcessName);
        Assert.Equal(session.StartedAt.UtcTicks, stored.StartedAt.UtcTicks);
        Assert.Equal(TimeSpan.FromMinutes(10), stored.Duration);
    }

    /// <summary>Sessions of the same process share one application row, ignoring case.</summary>
    [Fact]
    public async Task AddAsync_SameProcessNameDifferentCase_ReusesApplication()
    {
        await _sessions.AddAsync(Session("Code.exe", 0, 1), CancellationToken.None);
        await _sessions.AddAsync(Session("code.exe", 2, 3), CancellationToken.None);

        var applications = await _metadata.GetAllAsync(CancellationToken.None);

        Assert.Single(applications);
    }

    /// <summary>Open sessions overlap any range that starts after them; ended sessions do not.</summary>
    [Fact]
    public async Task GetOverlappingAsync_IncludesOpenSessionsAndExcludesEndedBeforeRange()
    {
        var ended = Session("a.exe", 0, 5);
        var open = Session("b.exe", 6, null);
        await _sessions.AddAsync(ended, CancellationToken.None);
        await _sessions.AddAsync(open, CancellationToken.None);

        var result = await _sessions.GetOverlappingAsync(Noon.AddMinutes(10), Noon.AddMinutes(20), CancellationToken.None);

        Assert.Equal(open.Id, Assert.Single(result).Id);
    }

    /// <summary>Updating a session stores its end time.</summary>
    [Fact]
    public async Task UpdateAsync_StoresEndTime()
    {
        var session = Session("a.exe", 0, null);
        await _sessions.AddAsync(session, CancellationToken.None);

        await _sessions.UpdateAsync(session.End(Noon.AddMinutes(3)), CancellationToken.None);

        var stored = Assert.Single(await _sessions.GetOverlappingAsync(Noon.AddMinutes(-1), Noon.AddHours(1), CancellationToken.None));
        Assert.Equal(TimeSpan.FromMinutes(3), stored.Duration);
    }

    /// <summary>The earliest start is reported, or null when empty.</summary>
    [Fact]
    public async Task GetEarliestStartAsync_ReturnsMinimumOrNull()
    {
        Assert.Null(await _sessions.GetEarliestStartAsync(CancellationToken.None));

        await _sessions.AddAsync(Session("a.exe", 5, 6), CancellationToken.None);
        await _sessions.AddAsync(Session("b.exe", 1, 2), CancellationToken.None);

        var earliest = await _sessions.GetEarliestStartAsync(CancellationToken.None);
        Assert.Equal(Noon.AddMinutes(1).UtcTicks, earliest!.Value.UtcTicks);
    }

    /// <summary>Deleting a session also deletes its screenshot records.</summary>
    [Fact]
    public async Task DeleteAsync_RemovesScreenshotsOfSession()
    {
        var session = Session("a.exe", 0, 5);
        await _sessions.AddAsync(session, CancellationToken.None);
        await _screenshots.AddAsync(new Screenshot(Guid.NewGuid(), session.Id, @"C:\x.png", Noon, 10, 10), CancellationToken.None);

        await _sessions.DeleteAsync([session.Id], CancellationToken.None);

        Assert.Empty(await _screenshots.GetBySessionAsync(session.Id, CancellationToken.None));
    }

    /// <summary>Retention deletes only sessions that ended before the cutoff.</summary>
    [Fact]
    public async Task DeleteEndedBeforeAsync_KeepsNewerAndOpenSessions()
    {
        var old = Session("old.exe", 0, 5);
        var recent = Session("recent.exe", 30, 35);
        var open = Session("open.exe", 40, null);
        foreach (var s in new[] { old, recent, open })
        {
            await _sessions.AddAsync(s, CancellationToken.None);
        }

        await _sessions.DeleteEndedBeforeAsync(Noon.AddMinutes(10), CancellationToken.None);

        var remaining = await _sessions.GetOverlappingAsync(Noon.AddHours(-1), Noon.AddHours(2), CancellationToken.None);
        Assert.Equal(new[] { recent.Id, open.Id }, remaining.Select(s => s.Id).ToArray());
    }

    /// <summary>Deleting everything removes sessions and screenshots.</summary>
    [Fact]
    public async Task DeleteAllAsync_RemovesEverything()
    {
        var session = Session("a.exe", 0, 5);
        await _sessions.AddAsync(session, CancellationToken.None);
        await _screenshots.AddAsync(new Screenshot(Guid.NewGuid(), session.Id, @"C:\x.png", Noon, 10, 10), CancellationToken.None);

        await _screenshots.DeleteAllAsync(CancellationToken.None);
        await _sessions.DeleteAllAsync(CancellationToken.None);

        Assert.Null(await _sessions.GetEarliestStartAsync(CancellationToken.None));
        Assert.Empty(await _screenshots.GetCapturedBetweenAsync(Noon.AddDays(-1), Noon.AddDays(1), CancellationToken.None));
    }

    /// <summary>Screenshots can be queried by time and by retention cutoff and deleted by id.</summary>
    [Fact]
    public async Task Screenshots_QueryByTimeAndDelete()
    {
        var session = Session("a.exe", 0, 60);
        await _sessions.AddAsync(session, CancellationToken.None);
        var first = new Screenshot(Guid.NewGuid(), session.Id, @"C:\1.png", Noon.AddMinutes(1), 1, 1);
        var second = new Screenshot(Guid.NewGuid(), session.Id, @"C:\2.png", Noon.AddMinutes(30), 1, 1);
        await _screenshots.AddAsync(first, CancellationToken.None);
        await _screenshots.AddAsync(second, CancellationToken.None);

        var between = await _screenshots.GetCapturedBetweenAsync(Noon, Noon.AddMinutes(10), CancellationToken.None);
        var before = await _screenshots.GetCapturedBeforeAsync(Noon.AddMinutes(10), CancellationToken.None);
        await _screenshots.DeleteAsync([first.Id], CancellationToken.None);
        var remaining = await _screenshots.GetBySessionAsync(session.Id, CancellationToken.None);

        Assert.Equal(first.Id, Assert.Single(between).Id);
        Assert.Equal(first.Id, Assert.Single(before).Id);
        Assert.Equal(second.Id, Assert.Single(remaining).Id);
    }

    /// <summary>Metadata upsert updates the existing application instead of duplicating it.</summary>
    [Fact]
    public async Task UpsertAsync_UpdatesExistingApplication()
    {
        await _metadata.UpsertAsync(new ApplicationMetadata(Guid.NewGuid(), "code.exe", "Code", null), CancellationToken.None);
        await _metadata.UpsertAsync(new ApplicationMetadata(Guid.NewGuid(), "code.exe", "Visual Studio Code", "Work"), CancellationToken.None);

        var all = await _metadata.GetAllAsync(CancellationToken.None);

        var item = Assert.Single(all);
        Assert.Equal("Visual Studio Code", item.DisplayName);
        Assert.Equal("Work", item.Category);
    }
}
