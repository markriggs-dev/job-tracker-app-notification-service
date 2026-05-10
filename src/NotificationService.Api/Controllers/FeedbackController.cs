using Microsoft.AspNetCore.Mvc;
using NotificationService.Core.Interfaces;
using NotificationService.Core.Models;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/feedback")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackPublisher _publisher;

    public FeedbackController(IFeedbackPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] FeedbackRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { error = "Feedback content is required." });

        var evt = new FeedbackSubmittedEvent
        {
            UserId = request.UserId ?? "anonymous",
            UserEmail = request.UserEmail,
            UserName = request.UserName,
            Content = request.Content.Trim(),
            OccurredAt = DateTimeOffset.UtcNow
        };

        await _publisher.PublishAsync(evt, ct);
        return Accepted();
    }
}

public record FeedbackRequest(
    string Content,
    string? UserId,
    string? UserEmail,
    string? UserName
);
