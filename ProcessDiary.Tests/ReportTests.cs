using ProcessDiary.Application.Reports;
using ProcessDiary.Domain.Abstractions;
using ProcessDiary.Domain.Entities;
using ProcessDiary.Domain.Reports;

namespace ProcessDiary.Tests;

/// <summary>Tests for report filters and report aggregation.</summary>
public class ReportTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 7, 12, 0, 0, TimeSpan.Zero); // Wednesday
    private static readonly ReportContext Ctx = new(Now, Now.AddDays(-30), Now.AddHours(-2));

    [Fact]
    public void Day_ResolvesLocalDay()
    {
        var r = ReportFilter.Day(new DateOnly(2026, 1, 7)).Resolve(Ctx);
        Assert.Equal(new DateTimeOffset(2026, 1, 7, 0, 0, 0, TimeSpan.Zero), r.From);
        Assert.Equal(r.From.AddDays(1), r.To);
    }

    [Fact]
    public void Week_StartsOnMonday()
    {
        var r = ReportFilter.Week(new DateOnly(2026, 1, 7)).Resolve(Ctx);
        Assert.Equal(new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero), r.From);
        Assert.Equal(r.From.AddDays(7), r.To);
    }

    [Fact]
    public void Week_SundayBelongsToPreviousMonday()
    {
        var r = ReportFilter.Week(new DateOnly(2026, 1, 11)).Resolve(Ctx);
        Assert.Equal(new DateOnly(2026, 1, 5), DateOnly.FromDateTime(r.From.DateTime));
    }

    [Fact]
    public void RelativeFilters_Resolve()
    {
        Assert.Equal(Ctx.FirstRecordedAt, ReportFilter.SinceStartOfRecording().Resolve(Ctx).From);
        Assert.Equal(Ctx.CurrentRecordingStartedAt, ReportFilter.SinceStartOfProcessDiaryRecording().Resolve(Ctx).From);
        Assert.Equal(Now.AddHours(-24), ReportFilter.Last24Hours().Resolve(Ctx).From);
    }

    [Fact]
    public void Between_RejectsReversedRange() =>
        Assert.Throws<ArgumentException>(() => ReportFilter.Between(Now, Now.AddHours(-1)).Resolve(Ctx));

    [Fact]
    public async Task TimePerApplication_ClipsAndAggregates()
    {
        var day = ReportFilter.Day(new DateOnly(2026, 1, 7));
        var sessions = new List<ProcessSession>
        {
            // Started yesterday, only 1h falls inside the day.
            new(Guid.NewGuid(), "code.exe", "", "", Now.AddHours(-13), Now.AddHours(-11)),
            new(Guid.NewGuid(), "code.exe", "", "", Now.AddHours(-3), Now.AddHours(-2)),
            new(Guid.NewGuid(), "chrome.exe", "", "", Now.AddHours(-1), null)
        };
        var service = new ReportService(new FakeSessions(sessions), new FakeMetadata());

        var report = await service.GetTimePerApplicationAsync(day, Ctx, CancellationToken.None);

        Assert.Equal(TimeSpan.FromHours(3), report.Total);
        Assert.Equal("code.exe", report.Entries[0].Key);
        Assert.Equal(TimeSpan.FromHours(2), report.Entries[0].Duration);
        Assert.Equal(200d / 3d, report.Entries[0].Percentage, 3);
    }

    [Fact]
    public async Task TimePerCategory_GroupsAndFallsBackToUncategorized()
    {
        var sessions = new List<ProcessSession>
        {
            new(Guid.NewGuid(), "code.exe", "", "", Now.AddHours(-4), Now.AddHours(-3)),
            new(Guid.NewGuid(), "rider.exe", "", "", Now.AddHours(-3), Now.AddHours(-1)),
            new(Guid.NewGuid(), "game.exe", "", "", Now.AddHours(-1), Now)
        };
        var meta = new FakeMetadata(
            new ApplicationMetadata(Guid.NewGuid(), "code.exe", "VS Code", "Development"),
            new ApplicationMetadata(Guid.NewGuid(), "rider.exe", "Rider", "Development"));
        var service = new ReportService(new FakeSessions(sessions), meta);

        var report = await service.GetTimePerCategoryAsync(ReportFilter.Last24Hours(), Ctx, CancellationToken.None);

        Assert.Equal("Development", report.Entries[0].Key);
        Assert.Equal(TimeSpan.FromHours(3), report.Entries[0].Duration);
        Assert.Equal(ReportService.UncategorizedKey, report.Entries[1].Key);
    }

    [Fact]
    public async Task EmptyRange_ReturnsZeroTotal()
    {
        var service = new ReportService(new FakeSessions([]), new FakeMetadata());
        var report = await service.GetTimePerApplicationAsync(ReportFilter.Last24Hours(), Ctx, CancellationToken.None);
        Assert.Equal(TimeSpan.Zero, report.Total);
        Assert.Empty(report.Entries);
    }

    /// <summary>In-memory session repository; only the query used by reports is implemented.</summary>
    private sealed class FakeSessions(List<ProcessSession> data) : IProcessSessionRepository
    {
        public Task<IReadOnlyList<ProcessSession>> GetOverlappingAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ProcessSession>>(
                data.Where(s => s.StartedAt < to && (s.EndedAt ?? DateTimeOffset.MaxValue) > from).ToList());

        public Task AddAsync(ProcessSession session, CancellationToken ct) => throw new NotSupportedException();
        public Task UpdateAsync(ProcessSession session, CancellationToken ct) => throw new NotSupportedException();
        public Task<DateTimeOffset?> GetEarliestStartAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct) => throw new NotSupportedException();
        public Task DeleteEndedBeforeAsync(DateTimeOffset cutoff, CancellationToken ct) => throw new NotSupportedException();
        public Task DeleteAllAsync(CancellationToken ct) => throw new NotSupportedException();
    }

    /// <summary>In-memory metadata repository; only reading is implemented.</summary>
    private sealed class FakeMetadata(params ApplicationMetadata[] items) : IApplicationMetadataRepository
    {
        public Task<IReadOnlyList<ApplicationMetadata>> GetAllAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ApplicationMetadata>>(items);
        public Task UpsertAsync(ApplicationMetadata metadata, CancellationToken ct) => throw new NotSupportedException();
    }
}
