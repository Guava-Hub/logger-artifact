using System;
using Microsoft.Extensions.Logging;

namespace MyCompany.Logging.ServiceBusSink;

/// <summary>
/// Represents a log entry to be forwarded to Azure Service Bus.
/// </summary>
public class ServiceBusLogEntry
{
    public string Category { get; set; } = string.Empty;

    public LogLevel LogLevel { get; set; }

    public string Message { get; set; } = string.Empty;

    public Exception? Exception { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}
