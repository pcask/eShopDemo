using EventBus.Base.Events;
using System.Collections.Generic;

namespace EventBus.Base.Abstraction;

public interface IEventBusSubscriptionManager
{
    // Herhangi bir event'i dinleyip, dinlemediğimizin kontrolü
    bool IsEmpty { get; }

    // Event silindiğinde tetiklenecek, e.g: UnSubscribe işleminde tetiklenecek.
    event EventHandler<string> OnEventRemoved;

    void AddSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
    void RemoveSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;

    // Gönderilen event'e ait Sub olup-olmadığı kontrolü için;
    bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;
    bool HasSubscriptionsForEvent(string eventName);

    // Gönderilen event isminden event'in Handler type'i döndürülecek
    Type GetEventTypeByName(string eventName);

    // Tüm Sub listesini silmek için;
    void Clear();

    // Gönderilen event'e ait bütün Subs ları döndürecek.
    IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
    IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

    string GetEventKey<T>() where T : IntegrationEvent;
}
