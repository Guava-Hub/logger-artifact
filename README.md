# MyCompany.Logging.ServiceBusSink

`MyCompany.Logging.ServiceBusSink` provides a lightweight `ILogger` provider that forwards structured log messages to an Azure Service Bus queue. The sink can be composed alongside any other provider, which means logs continue to flow to Serilog, NLog, log4net, or any framework that integrates with `Microsoft.Extensions.Logging`.

## Installation

Add the NuGet package reference to your project:

```bash
dotnet add package MyCompany.Logging.ServiceBusSink
```

## Registering the sink

Use `AddServiceBusSink` to register the sink when configuring logging:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCompany.Logging.ServiceBusSink;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddServiceBusSink(options =>
    {
        options.ConnectionString = "<your-service-bus-connection-string>";
        options.QueueName = "logs-queue";
        options.MinimumLogLevel = LogLevel.Debug;
    });
});
```

### Example `Program.cs` configuration

```csharp
services.AddLogging(builder =>
{
    builder.AddServiceBusSink(options =>
    {
        options.ConnectionString = "<your-service-bus-connection-string>";
        options.QueueName = "logs-queue";
        options.MinimumLogLevel = LogLevel.Debug;
    });
});
```

## Using with Serilog

Because the sink plugs into the `ILogger` pipeline it can live side-by-side with Serilog (or any other provider). Logs reach every configured provider including Service Bus.

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCompany.Logging.ServiceBusSink;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddLogging(logging =>
{
    logging.AddServiceBusSink(options =>
    {
        options.ConnectionString = "<your-service-bus-connection-string>";
        options.QueueName = "logs-queue";
        options.MinimumLogLevel = LogLevel.Information;
    });
});

var app = builder.Build();

app.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Demo")
    .LogInformation("This message is written to Serilog sinks and Azure Service Bus");

await app.RunAsync();
```

## Log payload format

Each log entry is serialized to JSON before being sent to Service Bus. A representative payload is shown below.

```json
{
  "category": "Demo",
  "logLevel": "Information",
  "message": "This is a demo log entry",
  "exception": null,
  "timestamp": "2024-01-01T12:00:00+00:00"
}
```

## Configuration options

| Option | Description |
| --- | --- |
| `ConnectionString` | Service Bus connection string with send permissions for the queue. |
| `QueueName` | The queue that will receive log messages from the sink. |
| `MinimumLogLevel` | Logs at or above this level are forwarded. Defaults to `Information`. |

## How it works

`ServiceBusLoggerProvider` manages a shared `ServiceBusSender` and exposes category-specific `ServiceBusLogger` instances. The logger serializes a `ServiceBusLogEntry` into JSON and sends the payload asynchronously to Service Bus. Exceptions are swallowed (after being written to `System.Diagnostics.Trace`) so that logging failures do not interrupt your application.

The sink uses the standard `ILogger` abstractions, so it remains compatible with other logging providers registered in the same application.
