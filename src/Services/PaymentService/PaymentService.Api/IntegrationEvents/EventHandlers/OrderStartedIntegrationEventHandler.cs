using EventBus.Base.Abstraction;
using EventBus.Base.Events;
using PaymentService.Api.IntegrationEvents.Events;

namespace PaymentService.Api.IntegrationEvents.EventHandlers;

public class OrderStartedIntegrationEventHandler(IConfiguration configuration, IEventBus eventBus, ILogger<OrderStartedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderStartedIntegrationEvent>
{

    public Task Handle(OrderStartedIntegrationEvent @event)
    {
        // Fake payment process
        string keyword = "PaymentSuccess";

        // Configuration'dan "PaymentSuccess" değeri okunarak ödemenin başarılı olup olmadığı değerini simule ediyoruz.
        bool paymentSuccessFlag = configuration.GetValue<bool>(keyword);

        // Ödeme sonucuna göre başarılı veya hatalı bir Event oluşturulacaktır;
        IntegrationEvent paymentEvent = paymentSuccessFlag
            ? new OrderPaymentSuccessIntegrationEvent(@event.OrderId)
            : new OrderPaymentFailedIntegrationEvent(@event.OrderId, "This is a fake error message");

        logger.LogInformation($"OrderStartedIntegrationEventHandler in PaymentService is fired with PaymentSuccess: {paymentSuccessFlag}, OrderId: {@event.OrderId}");

        // Oluşturulan Event eventBus'a push'lanıyor ki diğer service'lerde bu durumdan haberdar olsun ve gerekli aksiyonu alabilsin.
        eventBus.Publish(paymentEvent);

        return Task.CompletedTask;
    }
}
