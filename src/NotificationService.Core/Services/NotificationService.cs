using Microsoft.Extensions.Logging;
using NotificationService.Core.Models;

namespace NotificationService.Core.Services;

public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task HandleJobCreatedAsync(JobCreatedEvent evt, CancellationToken ct = default)
    {
        _logger.LogInformation("Job created event received for job {JobReqId} — queued for processing by consumer", evt.JobReqId);
        return Task.CompletedTask;
    }

    public Task HandleJobUpdatedAsync(JobUpdatedEvent evt, CancellationToken ct = default)
    {
        _logger.LogInformation("Job updated event received for job {JobReqId}", evt.JobReqId);
        return Task.CompletedTask;
    }
}
