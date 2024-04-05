using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using EventBus.UnitTest.Event.EventHandlers;
using EventBus.UnitTest.Event.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventBus.UnitTest;

[TestClass]
public class EventBusTests
{
    private readonly ServiceCollection _services;

    public EventBusTests()
    {
        _services = new ServiceCollection();
        _services.AddLogging(configure => configure.AddConsole());
    }

    [TestMethod]
    public void Subscribe_To_An_Event_With_RabbitMQ_Successfully()
    {
        _services.AddSingleton<IEventBus>(sp =>
        {
            return EventBusFactory.Create(GetRabbitMQConfig(), sp);
        });

        var sp = _services.BuildServiceProvider();

        // Singleton olarak kaydettiğimiz EventBusRabbitMQ nesnesini ele alalım;
        var eventBus = sp.GetRequiredService<IEventBus>();

        eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
    }

    // [TestMethod] Şimdilik AzureServiceBus hizmetini kullanmadığım için test edilmesine gerek yok
    public void Subscribe_To_An_Event_With_AzureServiceBus_Successfully()
    {
        _services.AddSingleton<IEventBus>(sp =>
        {
            return EventBusFactory.Create(GetAzureServiceBusConfig(), sp);
        });

        var sp = _services.BuildServiceProvider();

        // Singleton olarak kaydettiğimiz EventBusRabbitMQ nesnesini ele alalım;
        var eventBus = sp.GetRequiredService<IEventBus>();

        eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
    }

    [TestMethod]
    public void Send_A_Message_To_RabbitMQ_Successfully()
    {
        _services.AddSingleton<IEventBus>(sp =>
        {
            return EventBusFactory.Create(GetRabbitMQConfig(), sp);
        });

        var sp = _services.BuildServiceProvider();

        var eventBus = sp.GetRequiredService<IEventBus>();

        eventBus.Publish(new OrderCreatedIntegrationEvent(99));
    }

    [TestMethod]
    public void Consume_OrderCreatedIntegrationEvent_Messages_On_RabbitMQ_Successfully()
    {
        _services.AddTransient<OrderCreatedIntegrationEventHandler>();

        _services.AddSingleton<IEventBus>(sp =>
        {
            return EventBusFactory.Create(GetRabbitMQConfig(), sp);
        });

        var sp = _services.BuildServiceProvider();

        // Singleton olarak kaydettiğimiz EventBusRabbitMQ nesnesini ele alalım;
        var eventBus = sp.GetRequiredService<IEventBus>();

        eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
    }

    private EventBusConfig GetRabbitMQConfig()
    {
        return new()
        {
            ConnectionRetryCount = 5,
            SubscriberClientAppName = "EventBus.UnitTest",
            DefaultTopicName = "eShopDemoUnitTestTopic", // RabbitMQ'daki Exchange Name
            EventBusType = EventBusType.RabbitMQ,
            EventNameSuffix = "IntegrationEvent", // Event'lerimizin sonunda bulunan "IntegrationEvent" suffix'sinin silinerek RabbitMQ tarafında oluşturulması için.

            // RabbitMQ için default ConnectionFactory ayarları aşağıdaki gibidir, bir değişiklik yapılmayacaksa göndermeye gerek yok.
            //Connection = new ConnectionFactory()
            //{
            //    HostName = "localhost",
            //    Port = 5672,
            //    UserName = "guest",
            //    Password = "guest"
            //}
        };
    }
    private EventBusConfig GetAzureServiceBusConfig()
    {
        return new()
        {
            ConnectionRetryCount = 5,
            SubscriberClientAppName = "EventBus.UnitTest",
            DefaultTopicName = "eShopDemoUnitTestTopic",
            EventBusType = EventBusType.AzureServiceBus,
            EventNameSuffix = "IntegrationEvent",
            EventBusConnectionString = "Azure connection string should be here"
        };
    }
}