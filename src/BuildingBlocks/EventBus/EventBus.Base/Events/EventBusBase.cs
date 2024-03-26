using EventBus.Base.Abstraction;
using EventBus.Base.SubManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EventBus.Base.Events;

public abstract class EventBusBase : IEventBus
{
    public readonly IServiceProvider ServiceProvider;
    public readonly IEventBusSubscriptionManager SubsManager;

    public EventBusConfig config { get; private set; }

    public EventBusBase(EventBusConfig config, IServiceProvider serviceProvider)
    {
        this.config = config;
        ServiceProvider = serviceProvider;
        SubsManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
    }

    public virtual string ProcessEventName(string eventName)
    {
        if (config.DeleteEventPrefix)
            eventName = eventName.TrimStart([.. config.EventNamePrefix]);

        if (config.DeleteEventSuffix)
            eventName = eventName.TrimEnd([.. config.EventNameSuffix]);

        return eventName;
    }

    // RabbitMQ için QueueName, AzureServiceBus için SubscriptionName e.g: BasketService.OrderCreated, PaymentService.OrderCreated
    public virtual string GetSubName(string eventName)
    {
        return $"{config.SubscriberClientAppName}.{ProcessEventName(eventName)}";
    }

    public virtual void Dispose()
    {
        config = null;
        SubsManager.Clear();
    }

    public async Task<bool> ProcessEvent(string eventName, string message)
    {
        var processed = false;

        if (SubsManager.HasSubscriptionsForEvent(eventName))
        {
            var subscriptions = SubsManager.GetHandlersForEvent(eventName);

            using (var scope = ServiceProvider.CreateScope())
            {
                // Event'e ait olan tüm Handler'lar işletiliyor;
                foreach (var subscription in subscriptions)
                {
                    var handler = ServiceProvider.GetService(subscription.HandlerType);
                    if (handler == null) continue;

                    var eventType = SubsManager.GetEventTypeByName(eventName);
                    var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                    await (Task)handler.GetType().GetMethod("Handle")!.Invoke(handler, [integrationEvent])!;

                    //var handlerInterface = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                    //await (Task)handlerInterface.GetMethod("Handle").Invoke(handler, [integrationEvent]);
                }
            }
            
            processed = true;
        }

        return processed;
    }

    public abstract void Publish(IntegrationEvent @event);

    public abstract void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;

    public abstract void UnSubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
}
