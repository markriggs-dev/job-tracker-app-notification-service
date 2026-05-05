using Microsoft.Extensions.Logging;
using NotificationService.Core.Interfaces;
using NotificationService.Core.Models;

namespace NotificationService.Core.Services;

public class NotificationService
{
    private readonly IEmailSender _email;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IEmailSender email, ILogger<NotificationService> logger)
    {
        _email = email;
        _logger = logger;
    }

    public async Task HandleStatusChangedAsync(JobStatusChangedEvent evt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(evt.UserEmail))
        {
            _logger.LogWarning("No email address for user {UserId} — skipping status change notification", evt.UserId);
            return;
        }

        var subject = $"Application Update — {evt.RoleTitle} at {evt.CompanyName}";
        var body = $"""
            Your job application status has been updated.

            Role:     {evt.RoleTitle}
            Company:  {evt.CompanyName}
            Previous: {FormatStatus(evt.PreviousStatus)}
            New:      {FormatStatus(evt.NewStatus)}
            Updated:  {evt.OccurredAt:f}

            Log in to Job Tracker to view your application.
            """;

        await _email.SendAsync(evt.UserEmail, subject, body, ct);
        _logger.LogInformation("Sent status change notification to {Email} for job {JobReqId}", evt.UserEmail, evt.JobReqId);
    }

    public async Task HandleApplicationSubmittedAsync(ApplicationSubmittedEvent evt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(evt.UserEmail))
        {
            _logger.LogWarning("No email address for user {UserId} — skipping submission notification", evt.UserId);
            return;
        }

        var subject = $"Application Submitted — {evt.RoleTitle} at {evt.CompanyName}";
        var body = $"""
            Your application has been submitted successfully.

            Role:      {evt.RoleTitle}
            Company:   {evt.CompanyName}
            Submitted: {evt.OccurredAt:f}

            Log in to Job Tracker to track your application progress.
            """;

        await _email.SendAsync(evt.UserEmail, subject, body, ct);
        _logger.LogInformation("Sent submission notification to {Email} for job {JobReqId}", evt.UserEmail, evt.JobReqId);
    }

    private static string FormatStatus(string status) =>
        System.Text.RegularExpressions.Regex.Replace(status, "([A-Z])", " $1").Trim();
}
