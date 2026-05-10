using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Core.Interfaces;
using NotificationService.Core.Models;

namespace NotificationService.Infrastructure.Messaging;

public class KafkaFeedbackPublisher : IFeedbackPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaFeedbackPublisher> _logger;

    public KafkaFeedbackPublisher(IConfiguration configuration, ILogger<KafkaFeedbackPublisher> logger)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _producer = new ProducerBuilder<string, string>(
            new ProducerConfig { BootstrapServers = bootstrapServers }
        ).Build();
        _logger = logger;
    }

    public async Task PublishAsync(FeedbackSubmittedEvent evt, CancellationToken ct = default)
    {
        var message = new Message<string, string>
        {
            Key = evt.UserId,
            Value = JsonSerializer.Serialize(evt)
        };

        await _producer.ProduceAsync("feedback.submitted", message, ct);
        _logger.LogInformation("Published feedback.submitted for user {UserId}", evt.UserId);
    }

    public void Dispose() => _producer.Dispose();
}
