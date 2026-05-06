# job-tracker-app-notification-service

Kafka consumer service that processes job application events asynchronously. Logs all events and has MailKit SMTP infrastructure in place for future email delivery. Runs as a background service with no user-facing HTTP endpoints beyond a health check.

## Technology
- .NET 8
- Apache Kafka (Confluent.Kafka consumer, BackgroundService)
- MailKit SMTP
- Docker

## Kafka Topics Consumed

| Topic | Action |
|-------|--------|
| `job.application.created` | Logs the event (email delivery reserved for future implementation) |
| `job.application.updated` | Logs the event |

## Design

Kafka is used as an async work queue. The job service publishes create/edit payloads immediately and returns a response to the user. This service consumes those events independently, decoupling email delivery from the request path. At scale, additional consumers (AI service, analytics) can subscribe to the same topics without any changes to the job service.

## Getting started

```bash
dotnet restore
dotnet build
dotnet run --project src/NotificationService.Api
```

## Environment variables

| Variable | Description |
|----------|-------------|
| `Kafka__BootstrapServers` | Kafka broker address (default: localhost:9092) |
| `Email__SmtpHost` | SMTP server hostname |
| `Email__SmtpPort` | SMTP port (587 for TLS) |
| `Email__Username` | SMTP account username |
| `Email__Password` | SMTP app password (not account password) |
| `Email__FromAddress` | Sender email address |
| `Email__FromName` | Sender display name |
| `Email__UseSsl` | Enable TLS (true/false) |

## Project structure

```
src/
  NotificationService.Api/            # Startup, DI registration, health check
  NotificationService.Core/           # Event models, notification service, interfaces
  NotificationService.Infrastructure/ # Kafka consumer, SMTP email sender
tests/
  NotificationService.UnitTests/
  NotificationService.IntegrationTests/
```
