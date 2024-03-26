using EventBus.Base;
using EventBus.Base.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;

namespace EventBus.RabbitMQ;

public class RabbitMQEventBus : EventBusBase
{
    private readonly RabbitMQPersistentConnection persistentConnection;
    private readonly ConnectionFactory connectionFactory;
    private readonly EventBusConfig _config;
    private readonly IModel channel;
    public RabbitMQEventBus(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
    {
        _config = config;
        if (config.Connection != null)
        {
            // Object türünden config.Connection tüm message broker'lar için ihtiyaç duyulan connection nesnesini temsil etmektedir.
            // RabbitMQ implementasyonunda içerisinde varsa ConnectionFactory nesnesini ele alabilmek için;
            var connJson = JsonConvert.SerializeObject(config.Connection, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson) ?? new ConnectionFactory();
        }
        else
            connectionFactory = new ConnectionFactory();


        persistentConnection = new RabbitMQPersistentConnection(connectionFactory, config.ConnectionRetryCount);

        channel = CreateChannel();

        SubsManager.OnEventRemoved += SubsManager_OnEventRemoved;
    }

    public override void Publish(IntegrationEvent @event)
    {
        TryConnect();

        var policy = Policy.Handle<BrokerUnreachableException>()
                           .Or<SocketException>()
                           .WaitAndRetry(_config.ConnectionRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                           {
                               // Her yeniden denemeden önce gerekli loglama vs. operasyonları burada gerçekleştirilir.
                           });

        // Publish sırasında Exchange'in varlığından emin olmak için;
        channel.ExchangeDeclare(exchange: _config.DefaultTopicName, type: "direct");

        var messageArr = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
        var eventName = ProcessEventName(@event.GetType().Name);

        var properties = channel.CreateBasicProperties();
        properties.DeliveryMode = 2; // persistent

        policy.Execute(() =>
        {
            channel.BasicPublish(exchange: _config.DefaultTopicName,
                                 routingKey: eventName,
                                 mandatory: true,
                                 basicProperties: properties,
                                 body: messageArr);
        });
    }

    public override void Subscribe<T, TH>()
    {
        var eventName = ProcessEventName(typeof(T).Name);

        // RabbitMQ Susbcribe işlemleri
        if (!SubsManager.HasSubscriptionsForEvent(eventName))
        {
            TryConnect();

            channel.ExchangeDeclare(exchange: _config.DefaultTopicName, type: "direct");

            channel.QueueDeclare(queue: GetSubName(eventName),
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueBind(queue: GetSubName(eventName),
                              exchange: _config.DefaultTopicName,
                              routingKey: eventName,
                              arguments: null);

            // InMemory tarafında Subscribe işlemleri
            SubsManager.AddSubscription<T, TH>();
        }

        // İlgili queue dinlenmeye/consume edilmeye başlıyor;
        StartBasicConsume(eventName);
    }

    // Channel üzerinden ilgili queue'yu consume ediyoruz;
    private void StartBasicConsume(string eventName)
    {
        if (channel != null)
        {
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += Consumer_Received;

            channel.BasicConsume(queue: GetSubName(eventName),
                                 autoAck: false,
                                 consumer: consumer);
        }
    }

    // Consumer'a her yeni mesaj geldiğinde;
    private async void Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;

        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            await ProcessEvent(eventName, message); // Message'ın yani IntegrationEvent'in ilgili tüm Handler'ları yürütülüyor.
        }
        catch (Exception)
        {
            // log ProcessEvent Failure
        }

        channel.BasicAck(eventArgs.DeliveryTag,
                         multiple: true);
    }

    public override void UnSubscribe<T, TH>()
    {
        // InMemory tarafında Subscription siliniyor ve bu işlem aynı zamanda OnEventRemoved Event'ini tetikleyecektir.
        // RabbitMQ tarafında unSubscribe işlemleri SubsManager_OnEventRemoved method'unda ele alınıyor.
        SubsManager.RemoveSubscription<T, TH>();
    }

    // RabbitMQ tarafında unSubscribe işlemleri
    private void SubsManager_OnEventRemoved(object? sender, string eventName)
    {
        TryConnect();

        // İlgili queue silinmeden sadece Unbind edilecek ve exchange artık bu queue'lara message gönderemiyecektir.
        channel.QueueUnbind(queue: GetSubName(eventName),
                            exchange: _config.DefaultTopicName,
                            routingKey: eventName,
                            arguments: null);

        // Eğer Exchange ile bağlı son Queue da Unbind edildiyse channel kapatılıyor;
        if (SubsManager.IsEmpty)
            channel.Close();
    }

    // Exchange ile Queue arasında iletişimi sağlayacak olan channel oluşturuluyor;
    private IModel CreateChannel()
    {
        TryConnect();

        return persistentConnection.CreateChannel();
    }

    private void TryConnect()
    {
        if (!persistentConnection.IsConnected)
            persistentConnection.TryConnect();
    }
}
