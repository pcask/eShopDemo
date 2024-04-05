using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using NotificationService.ConsoleApp.IntegrationEvents.Events;

namespace NotificationService.ConsoleApp.IntegrationEvents.EventHandlers;

public class OrderPaymentSuccessIntegrationEventHandler(ILogger<OrderPaymentSuccessIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderPaymentSuccessIntegrationEvent>
{

    public Task Handle(OrderPaymentSuccessIntegrationEvent @event)
    {
        // Send success notification with Email, Sms, Push etc.

        logger.LogInformation($"Order Payment success with OrderId: {@event.OrderId}");

        return Task.CompletedTask;
    }
}