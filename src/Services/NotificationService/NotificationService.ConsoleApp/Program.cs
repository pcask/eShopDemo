
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.ConsoleApp.IntegrationEvents.EventHandlers;
using NotificationService.ConsoleApp.IntegrationEvents.Events;

ServiceCollection services = new();

ConfigureServices(services);

var sp = services.BuildServiceProvider();

IEventBus eventBus = sp.GetRequiredService<IEventBus>();

eventBus.Subscribe<OrderPaymentSuccessIntegrationEvent, OrderPaymentSuccessIntegrationEventHandler>();
eventBus.Subscribe<OrderPaymentFailedIntegrationEvent, OrderPaymentFailedIntegrationEventHandler>();

Console.WriteLine("Notification Service is running...");

Console.ReadLine();

void ConfigureServices(ServiceCollection services)
{
    services.AddLogging(configure =>
    {
        configure.AddConsole();
    });

    services.AddTransient<OrderPaymentFailedIntegrationEventHandler>();
    services.AddTransient<OrderPaymentSuccessIntegrationEventHandler>();

    services.AddSingleton<IEventBus>(sp =>
    {
        EventBusConfig config = new()
        {
            SubscriberClientAppName = "NotificationService",
            EventBusType = EventBusType.RabbitMQ,
            EventNameSuffix = "IntegrationEvent",
            ConnectionRetryCount = 5
        };

        return EventBusFactory.Create(config, sp);
    });
}