using EventBus.Base.Events;

namespace EventBus.Base.Abstraction;

// in keyword'ü bize contravariant type kullanımı sağlar, yani method parametrelerine base type'dan nesneler atanabilir.
public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent @event);
}

// For marking only
public interface IIntegrationEventHandler
{
}