namespace EventBus.Base;

public class EventBusConfig
{
    public byte ConnectionRetryCount { get; set; } = 5;

    public string DefaultTopicName { get; set; } = "EShopEventBus";

    public string EventBusConnectionString { get; set; } = string.Empty;

    // Bir event'i birden fazla service dinleyebilir, bunların ayrımını yapabilmek için EventName başına SubscriberClientAppName gelecektir e.g: BasketService.OrderCreated, PaymentService.OrderCreated
    public string SubscriberClientAppName { get; set; } = string.Empty;

    public string EventNamePrefix { get; set; } = string.Empty;

    public string EventNameSuffix { get; set; } = "IntegrationEvent";

    public EventBusType EventBusType { get; set; } = EventBusType.RabbitMQ;

    // Object türünden tüm message broker'lar için ihtiyaç duyulan connection nesnesini temsil etmektedir.
    public object Connection { get; set; }

    public bool DeleteEventPrefix => !string.IsNullOrEmpty(EventNamePrefix);
    public bool DeleteEventSuffix => !string.IsNullOrEmpty(EventNameSuffix);
}

public enum EventBusType
{
    RabbitMQ = 0,
    AzureServiceBus = 1
}
