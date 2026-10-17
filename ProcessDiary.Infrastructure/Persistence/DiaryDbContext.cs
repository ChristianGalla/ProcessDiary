using Microsoft.EntityFrameworkCore;

namespace ProcessDiary.Infrastructure.Persistence;

/// <summary>Entity Framework model of the diary database.</summary>
internal sealed class DiaryDbContext : DbContext
{
    /// <summary>Initializes the context.</summary>
    /// <param name="options">The context options.</param>
    public DiaryDbContext(DbContextOptions<DiaryDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets the applications.</summary>
    public DbSet<ApplicationRow> Applications => Set<ApplicationRow>();

    /// <summary>Gets the sessions.</summary>
    public DbSet<SessionRow> Sessions => Set<SessionRow>();

    /// <summary>Gets the screenshots.</summary>
    public DbSet<ScreenshotRow> Screenshots => Set<ScreenshotRow>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationRow>(e =>
        {
            e.ToTable("Applications");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProcessName).IsRequired().UseCollation("NOCASE");
            e.HasIndex(x => x.ProcessName).IsUnique();
        });

        modelBuilder.Entity<SessionRow>(e =>
        {
            e.ToTable("Sessions");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Application).WithMany(x => x.Sessions)
                .HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.StartedAtUtcTicks);
            e.HasIndex(x => x.EndedAtUtcTicks);
        });

        modelBuilder.Entity<ScreenshotRow>(e =>
        {
            e.ToTable("Screenshots");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Session).WithMany(x => x.Screenshots)
                .HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.CapturedAtUtcTicks);
        });
    }
}
