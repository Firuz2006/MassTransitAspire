# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET Aspire application that demonstrates MassTransit integration with RabbitMQ messaging. The solution consists of two projects:

1. **MassTransitAspire** (AppHost) - .NET Aspire orchestration project that manages the application lifecycle and infrastructure dependencies
2. **MassTransit.Api** - ASP.NET Core Web API that integrates with MassTransit for message-based communication

## Architecture

### .NET Aspire AppHost Pattern
The AppHost project (`MassTransitAspire/AppHost.cs`) uses .NET Aspire's distributed application builder to:
- Define infrastructure resources (RabbitMQ via `AddRabbitMQ`)
- Reference and wire up service projects via `AddProject<T>`
- Automatically inject connection strings into referenced projects

The RabbitMQ connection string is automatically made available to the API project through the `WithReference(rabbitmq)` call, accessible via configuration key `"messaging"`.

### MassTransit Configuration
The API project configures MassTransit with RabbitMQ transport:
- Connection string is retrieved from configuration using key `"messaging"` (injected by Aspire)
- Uses kebab-case endpoint name formatting convention
- Automatic endpoint configuration via `ConfigureEndpoints(context)`
- Two consumers registered: `CarRegisteredConsumer` and `CarMaintenanceScheduledConsumer`

### Message-Based Architecture
The application implements a publish-subscribe pattern with two message types:
- **CarRegistered** - Published when a car is registered via `/cars/register` endpoint
- **CarMaintenanceScheduled** - Published when maintenance is scheduled via `/cars/maintenance` endpoint

Each message is handled by a corresponding consumer that logs the message details using Serilog.

## Common Commands

### Build
```bash
dotnet build
```

### Run the Application
Run the AppHost project to start the entire distributed application:
```bash
dotnet run --project MassTransitAspire
```

This will start both the RabbitMQ container and the API project with proper service orchestration.

### Run API Standalone
```bash
dotnet run --project MassTransit.Api
```
Note: Running the API standalone requires RabbitMQ to be available and the connection string to be configured separately.

### Test
```bash
dotnet test
```

## Technology Stack

- **.NET 10.0** - Target framework for all projects
- **Aspire.AppHost.Sdk 13.0.0** - Application orchestration
- **Aspire.Hosting.RabbitMQ 13.0.0** - RabbitMQ hosting integration
- **MassTransit.RabbitMQ 8.5.5** - Message bus abstraction over RabbitMQ
- **ASP.NET Core 10.0** - Web API framework
- **Serilog.AspNetCore 9.0.0** - Structured logging framework
- **Serilog.Sinks.Console 6.1.1** - Console output for Serilog

## Development Notes

### API Endpoints
The application exposes the following endpoints:

**POST /cars/register** - Register a new car
- Publishes a `CarRegistered` message to the queue
- Example request body:
```json
{
  "carId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "make": "Toyota",
  "model": "Camry",
  "year": 2024,
  "vin": "1HGBH41JXMN109186",
  "registeredAt": "2024-11-21T10:00:00Z"
}
```

**POST /cars/maintenance** - Schedule car maintenance
- Publishes a `CarMaintenanceScheduled` message to the queue
- Example request body:
```json
{
  "maintenanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "carId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "serviceType": "Oil Change",
  "scheduledDate": "2024-12-01T10:00:00Z",
  "description": "Regular maintenance oil change",
  "estimatedCost": 75.00
}
```

### Adding Message Consumers
When adding MassTransit consumers:
1. Create consumer classes implementing `IConsumer<TMessage>` in the `Consumers` folder
2. Register consumers in `Program.cs` using `x.AddConsumer<TConsumer>()` within the MassTransit configuration
3. Consumers will be automatically configured with endpoints via `ConfigureEndpoints(context)`
4. Use ILogger for logging within consumers

### Connection String Resolution
The API project expects a connection string named `"messaging"`. In the Aspire-orchestrated setup, this is automatically injected. For local development outside Aspire, configure it in `appsettings.Development.json` or user secrets.
