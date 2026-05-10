using NotificationService.Core.Interfaces;
using NotificationService.Core.Services;
using NotificationService.Core.Settings;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// Feedback
builder.Services.Configure<FeedbackSettings>(builder.Configuration.GetSection("Feedback"));
builder.Services.AddSingleton<IFeedbackPublisher, KafkaFeedbackPublisher>();

// Notification service
builder.Services.AddScoped<NotificationService.Core.Services.NotificationService>();

// Kafka consumer (background service)
builder.Services.AddHostedService<KafkaNotificationConsumer>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

app.Run();
