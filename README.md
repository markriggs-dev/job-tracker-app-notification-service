# job-tracker-app-notification-service

Processes Kafka events and sends configurable email reminders to users.

## Technology
- .NET 8 Web API
- C#
- PostgreSQL
- Docker

## Getting started

```bash
dotnet restore
dotnet build
dotnet run --project src/NotificationService.Api
```

## Running with Docker

```bash
docker build -t job-tracker-app-notification-service .
docker run -p 5005:5005 job-tracker-app-notification-service
```

## Environment variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Auth0__Domain` | Auth0 domain |
| `Auth0__Audience` | Auth0 API audience |
| `Kafka__BootstrapServers` | Kafka broker address |

## Project structure

```
src/
  NotificationService.Api/          # Web API entry point, controllers, middleware
  NotificationService.Core/         # Domain models, interfaces, business logic
  NotificationService.Infrastructure/ # Data access, Kafka, external integrations
tests/
  NotificationService.UnitTests/
  NotificationService.IntegrationTests/
```
