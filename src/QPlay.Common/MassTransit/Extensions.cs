using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QPlay.Common.Settings;
using System;
using System.Reflection;

namespace QPlay.Common.MassTransit;

/// <summary>
/// This class provides extension methods to register and configure MassTransit services with RabbitMQ into IServiceCollection.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds and configures MassTransit with RabbitMQ related settings and services to the IServiceCollection.
    /// It registers MassTransit consumers from the entry assembly and sets up retry policies.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="configureRetries">Optional action to configure retry policies.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, Action<IRetryConfigurator> configureRetries = null)
    {
        services.AddMassTransit(configure =>
        {
            configure.AddConsumers(Assembly.GetEntryAssembly());
            configure.ConfigureRabbitMq(configureRetries);
        });

        services.AddMassTransitHostedService();
        return services;
    }

    /// <summary>
    /// Configures RabbitMQ as the message broker for MassTransit.
    /// </summary>
    /// <param name="configure">The IServiceCollectionBusConfigurator to configure the services.</param>
    /// <param name="configureRetries">Optional action to configure retry policies.</param>
    public static void ConfigureRabbitMq(this IServiceCollectionBusConfigurator configure, Action<IRetryConfigurator> configureRetries = null)
    {
        configure.UsingRabbitMq((context, configurator) =>
        {
            IConfiguration configuration = context.GetService<IConfiguration>();
            ServiceSettings serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            RabbitMQSettings rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
            configurator.Host(rabbitMQSettings.Host);
            configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
            configureRetries ??= (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            configurator.UseMessageRetry(configureRetries);
        });
    }
}
