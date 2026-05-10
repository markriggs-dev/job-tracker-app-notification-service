using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Core.Models;
using NotificationService.Core.Services;

namespace NotificationService.Infrastructure.Messaging;

public class KafkaNotificationConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaNotificationConsumer> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public KafkaNotificationConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<KafkaNotificationConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Yield immediately so Kestrel can finish starting before this loop blocks on Consume()
        await Task.Yield();

        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "notification-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SecurityProtocol = SecurityProtocol.Plaintext
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(["job.application.created", "job.application.updated", "feedback.submitted"]);

        _logger.LogInformation("Kafka consumer started — subscribed to job.application.created, job.application.updated, feedback.submitted");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(ct);
                if (result?.Message is null) continue;

                _logger.LogInformation("Received message from topic {Topic}", result.Topic);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<NotificationService.Core.Services.NotificationService>();

                if (result.Topic == "job.application.created")
                {
                    var evt = JsonSerializer.Deserialize<JobCreatedEvent>(result.Message.Value, JsonOptions);
                    if (evt is not null)
                        await service.HandleJobCreatedAsync(evt, ct);
                }
                else if (result.Topic == "job.application.updated")
                {
                    var evt = JsonSerializer.Deserialize<JobUpdatedEvent>(result.Message.Value, JsonOptions);
                    if (evt is not null)
                        await service.HandleJobUpdatedAsync(evt, ct);
                }
                else if (result.Topic == "feedback.submitted")
                {
                    var evt = JsonSerializer.Deserialize<FeedbackSubmittedEvent>(result.Message.Value, JsonOptions);
                    if (evt is not null)
                        await service.HandleFeedbackSubmittedAsync(evt, ct);
                }

                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message");
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }

        consumer.Close();
        _logger.LogInformation("Kafka consumer stopped");
    }
}
