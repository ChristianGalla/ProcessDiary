using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProcessDiary.Application.Privacy;

/// <summary>Applies the retention rule shortly after start and then once per hour.</summary>
public sealed class RetentionBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IDataManagementService _data;
    private readonly TimeProvider _time;
    private readonly ILogger<RetentionBackgroundService> _logger;

    /// <summary>Initializes the job.</summary>
    /// <param name="data">Deletes the old data.</param>
    /// <param name="time">Provides the clock used for waiting.</param>
    /// <param name="logger">The logger.</param>
    public RetentionBackgroundService(IDataManagementService data, TimeProvider time, ILogger<RetentionBackgroundService> logger)
    {
        _data = data;
        _time = time;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _data.ApplyRetentionAsync(stoppingToken);
                await Task.Delay(Interval, _time, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retention could not be applied");
                try
                {
                    await Task.Delay(Interval, _time, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
