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
        consumer.Subscribe(["job.status.changed", "job.application.submitted"]);

        _logger.LogInformation("Kafka consumer started — subscribed to job.status.changed, job.application.submitted");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(ct);
                if (result?.Message is null) continue;

                _logger.LogInformation("Received message from topic {Topic}", result.Topic);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<NotificationService.Core.Services.NotificationService>();

                if (result.Topic == "job.status.changed")
                {
                    var evt = JsonSerializer.Deserialize<JobStatusChangedEvent>(result.Message.Value, JsonOptions);
                    if (evt is not null)
                        await service.HandleStatusChangedAsync(evt, ct);
                }
                else if (result.Topic == "job.application.submitted")
                {
                    var evt = JsonSerializer.Deserialize<ApplicationSubmittedEvent>(result.Message.Value, JsonOptions);
                    if (evt is not null)
                        await service.HandleApplicationSubmittedAsync(evt, ct);
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
