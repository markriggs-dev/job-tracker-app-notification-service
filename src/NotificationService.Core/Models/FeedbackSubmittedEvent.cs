namespace NotificationService.Core.Models;

public class FeedbackSubmittedEvent
{
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}
