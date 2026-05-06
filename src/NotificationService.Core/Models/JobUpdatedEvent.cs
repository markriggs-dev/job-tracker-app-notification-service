namespace NotificationService.Core.Models;

public class JobUpdatedEvent
{
    public Guid JobReqId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public string? JobDescription { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
