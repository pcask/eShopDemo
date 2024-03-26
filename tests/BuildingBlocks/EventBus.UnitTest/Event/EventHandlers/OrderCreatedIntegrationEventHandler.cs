using EventBus.Base.Abstraction;
using EventBus.UnitTest.Event.Events;
using System.Diagnostics;

namespace EventBus.UnitTest.Event.EventHandlers;

public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public Task Handle(OrderCreatedIntegrationEvent @event)
    {
        Debug.WriteLine($"The event with {@event.TestId} Test Id, has been consumed.");
        return Task.CompletedTask;
    }
}