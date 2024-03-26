using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBus.AzureServiceBus;

public class AzureEventBus : EventBusBase
{
    private ITopicClient topicClient;
    private ManagementClient managementClient;
    private readonly EventBusConfig config;
    private readonly ILogger logger;

    public AzureEventBus(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
    {
        this.config = config;
        managementClient = new ManagementClient(config.EventBusConnectionString);
        topicClient = CreateTopicClient();
        logger = serviceProvider.GetService<ILogger<EventBusBase>>();
    }

    private ITopicClient CreateTopicClient()
    {
        if (topicClient == null || topicClient.IsClosedOrClosing)
            topicClient = new TopicClient(config.EventBusConnectionString, config.DefaultTopicName, retryPolicy: RetryPolicy.Default);

        // Management tarafında da ilgili Topic'in olup olmadığını kontrol edip, yoksa oluşturalım.
        if (!managementClient.TopicExistsAsync(config.DefaultTopicName).GetAwaiter().GetResult())
            managementClient.CreateTopicAsync(config.DefaultTopicName).GetAwaiter().GetResult();

        return topicClient;
    }

    public override void Publish(IntegrationEvent @event)
    {
        var eventName = ProcessEventName(@event.GetType().Name); // "OrderCreatedIntegrationEvent" to "OrderCreated"

        var message = new Message()
        {
            MessageId = Guid.NewGuid().ToString(),
            Label = eventName, // Label'ı RabbitMQ TopicExchange türü için Message içindeki RoutingKey olarak düşünebiliriz.
            Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event))
        };

        topicClient.SendAsync(message).GetAwaiter().GetResult();
    }

    public override void Subscribe<T, TH>()
    {
        var eventName = ProcessEventName(typeof(T).Name);

        if (!SubsManager.HasSubscriptionsForEvent(eventName))
        {
            var subClient = CreateSubscriptionClientIfNotExist(eventName);

            RegisterMessageHandlerToSubscriptionClient(subClient);
        }

        logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

        SubsManager.AddSubscription<T, TH>();
    }

    // Burada Subscription silinmiyor sadece ona ait olan Rule silinerek Sub boşa çıkarılmış oluyor.
    public override void UnSubscribe<T, TH>()
    {
        var eventName = ProcessEventName(typeof(T).Name);

        // İlgili client üzerinden Rule'ı silebilmek için;
        var subClient = CreateSubscriptionClient(eventName);

        try
        {
            subClient.RemoveRuleAsync(eventName).GetAwaiter().GetResult();
        }
        catch (MessagingEntityNotFoundException)
        {
            logger.LogWarning("The messaging entity {EventName} could not be found.", eventName);
        }

        logger.LogInformation("Unsubscribing from event {EventName}", eventName);

        SubsManager.RemoveSubscription<T, TH>();
    }

    private void RegisterMessageHandlerToSubscriptionClient(ISubscriptionClient subscriptionClient)
    {
        subscriptionClient.RegisterMessageHandler(async (message, token) =>
        {
            var eventName = message.Label;
            var messageData = Encoding.UTF8.GetString(message.Body);

            // İlgili message'ın tüm handler'ları işleniyor.
            if (await ProcessEvent(eventName, messageData))
                await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken); // Complete işlemi yapılmalı ki message tekrardan işlenmesin!.

        }, new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = 10, AutoComplete = false });
    }

    private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs eventArgs)
    {
        var ex = eventArgs.Exception;
        var context = eventArgs.ExceptionReceivedContext;

        logger.LogError("Error handling message: {ExceptionMessage} - Context: {ExceptionContext}", ex.Message, context);

        return Task.CompletedTask;
    }

    private ISubscriptionClient CreateSubscriptionClientIfNotExist(string eventName)
    {
        var subClient = CreateSubscriptionClient(eventName);

        // AzureServiceBus tarafında Subscription yoksa oluşturuluyor;
        if (!managementClient.SubscriptionExistsAsync(config.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult())
        {
            managementClient.CreateSubscriptionAsync(config.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();

            RemoveDefaultRule(subClient); // DefaultRule siliniyor
        }

        CreateRuleIfNotExist(eventName, subClient); // Yoksa publish edilen message ile aynı label'a sahip yeni bir rule oluşturuluyor.

        return subClient;
    }

    private SubscriptionClient CreateSubscriptionClient(string eventName)
    {
        return new SubscriptionClient(config.EventBusConnectionString, config.DefaultTopicName, GetSubName(eventName));
    }

    private void RemoveDefaultRule(SubscriptionClient subscriptionClient)
    {
        try
        {
            subscriptionClient.RemoveRuleAsync(RuleDescription.DefaultRuleName).GetAwaiter().GetResult();
        }
        catch (MessagingEntityNotFoundException)
        {
            logger.LogWarning("The message entity {DefaultRuleName}, could not be found.", RuleDescription.DefaultRuleName);
        }
    }

    private void CreateRuleIfNotExist(string eventName, ISubscriptionClient subscriptionClient)
    {
        bool ruleExits;

        try
        {
            var rule = managementClient.GetRuleAsync(config.DefaultTopicName, GetSubName(eventName), eventName).GetAwaiter().GetResult();
            ruleExits = rule != null;
        }
        catch (MessagingEntityNotFoundException)
        {
            ruleExits = false;
        }

        if (!ruleExits)
        {
            subscriptionClient.AddRuleAsync(new()
            {
                Filter = new CorrelationFilter { Label = eventName },
                Name = eventName
            }).GetAwaiter().GetResult();
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        managementClient.CloseAsync().GetAwaiter().GetResult();
        topicClient.CloseAsync().GetAwaiter().GetResult();
        managementClient = null;
        topicClient = null;

    }
}
