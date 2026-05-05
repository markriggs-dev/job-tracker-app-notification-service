using NotificationService.Core.Interfaces;
using NotificationService.Core.Services;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// Notification service
builder.Services.AddScoped<NotificationService.Core.Services.NotificationService>();

// Kafka consumer (background service)
builder.Services.AddHostedService<KafkaNotificationConsumer>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

app.Run();
