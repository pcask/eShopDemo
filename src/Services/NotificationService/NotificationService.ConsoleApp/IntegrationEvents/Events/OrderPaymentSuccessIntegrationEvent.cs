using EventBus.Base.Events;

namespace NotificationService.ConsoleApp.IntegrationEvents.Events;


public class OrderPaymentSuccessIntegrationEvent(int orderId) : IntegrationEvent
{
    public int OrderId { get; } = orderId;
}
