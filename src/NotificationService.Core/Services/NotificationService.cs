using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Core.Interfaces;
using NotificationService.Core.Models;
using NotificationService.Core.Settings;

namespace NotificationService.Core.Services;

public class NotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly FeedbackSettings _feedbackSettings;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailSender emailSender,
        IOptions<FeedbackSettings> feedbackSettings,
        ILogger<NotificationService> logger)
    {
        _emailSender = emailSender;
        _feedbackSettings = feedbackSettings.Value;
        _logger = logger;
    }

    public Task HandleJobCreatedAsync(JobCreatedEvent evt, CancellationToken ct = default)
    {
        _logger.LogInformation("Job created event received for job {JobReqId}", evt.JobReqId);
        return Task.CompletedTask;
    }

    public Task HandleJobUpdatedAsync(JobUpdatedEvent evt, CancellationToken ct = default)
    {
        _logger.LogInformation("Job updated event received for job {JobReqId}", evt.JobReqId);
        return Task.CompletedTask;
    }

    public async Task HandleFeedbackSubmittedAsync(FeedbackSubmittedEvent evt, CancellationToken ct = default)
    {
        _logger.LogInformation("Feedback received from user {UserId}", evt.UserId);

        if (string.IsNullOrWhiteSpace(_feedbackSettings.RecipientEmail))
        {
            _logger.LogWarning("Feedback:RecipientEmail is not configured — skipping email");
            return;
        }

        var from = string.IsNullOrWhiteSpace(evt.UserName)
            ? evt.UserEmail ?? "Unknown"
            : $"{evt.UserName} ({evt.UserEmail})";

        var subject = "Job Tracker — User Feedback";
        var body = $"From: {from}\nSubmitted: {evt.OccurredAt:u}\n\n{evt.Content}";

        await _emailSender.SendAsync(_feedbackSettings.RecipientEmail, subject, body, ct);
    }
}
