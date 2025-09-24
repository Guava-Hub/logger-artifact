using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace MyCompany.Logging.ServiceBusSink;

/// <summary>
/// Logger implementation that forwards log entries to Azure Service Bus.
/// </summary>
internal sealed class ServiceBusLogger : ILogger
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private readonly string _categoryName;
    private readonly ServiceBusSender _sender;
    private readonly LogLevel _minimumLogLevel;

    public ServiceBusLogger(string categoryName, ServiceBusSender sender, ServiceBusSinkOptions options)
    {
        _categoryName = categoryName;
        _sender = sender;
        _minimumLogLevel = options.MinimumLogLevel;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
        => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= _minimumLogLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter is null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        string message = formatter(state, exception);

        if (string.IsNullOrEmpty(message) && exception is null)
        {
            return;
        }

        var entry = new ServiceBusLogEntry
        {
            Category = _categoryName,
            LogLevel = logLevel,
            Message = message,
            Exception = exception,
            Timestamp = DateTimeOffset.UtcNow
        };

        _ = SendAsync(entry);
    }

    private async Task SendAsync(ServiceBusLogEntry entry)
    {
        try
        {
            string payload = JsonSerializer.Serialize(entry, SerializerOptions);
            var message = new ServiceBusMessage(payload)
            {
                ContentType = "application/json"
            };

            await _sender.SendMessageAsync(message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Trace.TraceError("ServiceBusLogger failed to send log entry: {0}", ex);
        }
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
