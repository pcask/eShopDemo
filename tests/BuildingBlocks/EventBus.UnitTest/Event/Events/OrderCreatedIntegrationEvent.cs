using EventBus.Base.Events;

namespace EventBus.UnitTest.Event.Events;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public int TestId;
    public OrderCreatedIntegrationEvent(int testId)
    {
        TestId = testId;
    }
}