using Microsoft.Extensions.Logging;

namespace MyCompany.Logging.ServiceBusSink;

/// <summary>
/// Configuration options for the Service Bus logger sink.
/// </summary>
public class ServiceBusSinkOptions
{
    /// <summary>
    /// Gets or sets the Azure Service Bus connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the queue name that will receive the log entries.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum log level to forward to Service Bus.
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException("ConnectionString must be provided for the Service Bus sink.");
        }

        if (string.IsNullOrWhiteSpace(QueueName))
        {
            throw new InvalidOperationException("QueueName must be provided for the Service Bus sink.");
        }
    }
}
