using NotificationService.Core.Models;

namespace NotificationService.Core.Interfaces;

public interface IFeedbackPublisher
{
    Task PublishAsync(FeedbackSubmittedEvent evt, CancellationToken ct = default);
}
