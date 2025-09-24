using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace MyCompany.Logging.ServiceBusSink;

/// <summary>
/// Provides <see cref="ServiceBusLogger"/> instances for the logging infrastructure.
/// </summary>
internal sealed class ServiceBusLoggerProvider : ILoggerProvider
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusSinkOptions _options;
    private readonly ConcurrentDictionary<string, ServiceBusLogger> _loggers = new();
    private bool _disposed;

    public ServiceBusLoggerProvider(ServiceBusSinkOptions options, ServiceBusClient client)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _sender = _client.CreateSender(_options.QueueName);
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (categoryName is null)
        {
            throw new ArgumentNullException(nameof(categoryName));
        }

        return _loggers.GetOrAdd(categoryName, name => new ServiceBusLogger(name, _sender, _options));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var loggerName in _loggers.Keys)
        {
            _loggers.TryRemove(loggerName, out _);
        }

        try
        {
            _sender.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Trace.TraceError("Failed to dispose ServiceBusSender: {0}", ex);
        }

        try
        {
            _client.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Trace.TraceError("Failed to dispose ServiceBusClient: {0}", ex);
        }
    }
}
