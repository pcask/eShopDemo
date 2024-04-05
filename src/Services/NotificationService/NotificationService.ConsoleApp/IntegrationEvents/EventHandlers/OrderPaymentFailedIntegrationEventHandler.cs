using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using NotificationService.ConsoleApp.IntegrationEvents.Events;

namespace NotificationService.ConsoleApp.IntegrationEvents.EventHandlers;

public class OrderPaymentFailedIntegrationEventHandler(ILogger<OrderPaymentFailedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
{

    public Task Handle(OrderPaymentFailedIntegrationEvent @event)
    {
        // Send fail notification with Email, Sms, Push etc.

        logger.LogInformation($"Order Payment failed with OrderId: {@event.OrderId}, ErrorMessage: {@event.ErrorMessage}");

        return Task.CompletedTask;
    }
}
