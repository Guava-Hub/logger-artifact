using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyCompany.Logging.ServiceBusSink;

/// <summary>
/// Extensions to register the Service Bus logging sink.
/// </summary>
public static class ServiceBusLoggerExtensions
{
    public static ILoggingBuilder AddServiceBusSink(this ILoggingBuilder builder, Action<ServiceBusSinkOptions> configure)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.Services.AddSingleton(provider =>
        {
            var options = new ServiceBusSinkOptions();
            configure(options);
            options.Validate();
            return options;
        });

        builder.Services.AddSingleton<ILoggerProvider>(sp =>
        {
            var options = sp.GetRequiredService<ServiceBusSinkOptions>();
            var client = new ServiceBusClient(options.ConnectionString);
            return new ServiceBusLoggerProvider(options, client);
        });

        return builder;
    }
}
