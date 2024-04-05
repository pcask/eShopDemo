using EventBus.Base.Events;

namespace PaymentService.Api.IntegrationEvents.Events;

public class OrderPaymentSuccessIntegrationEvent(int orderId) : IntegrationEvent
{
    public int OrderId { get; } = orderId;
}
