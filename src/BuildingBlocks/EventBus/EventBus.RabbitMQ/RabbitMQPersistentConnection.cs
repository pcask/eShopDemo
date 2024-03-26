using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace EventBus.RabbitMQ;

// RabbitMQ' a kalıcı bir bağlantı sağlanması için;
public class RabbitMQPersistentConnection(IConnectionFactory connectionFactory, byte retryCount = 5) : IDisposable
{
    private IConnection connection;
    private readonly object lockObject = new();
    private bool isDisposed;

    public void Dispose()
    {
        isDisposed = true;
        connection.Dispose();
    }

    public bool IsConnected => connection != null && connection.IsOpen;

    // RabbitMQ üzerinde Channel oluşturabilmek için;
    public IModel CreateChannel()
    {
        return connection.CreateModel();
    }

    public bool TryConnect()
    {
        lock (lockObject)
        {
            var policy = Policy.Handle<SocketException>()
                               .Or<BrokerUnreachableException>()
                               .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, time) =>
                               {
                                   //Her yeniden denemeden önce gerekli loglama vs. operasyonları burada gerçekleştirilir.
                               });

            // Aşağıdaki işlem yapıldığında SocketException veya BrokerUnreachableException türünden bir exception alındığında retry edilecektir.
            policy.Execute(() =>
            {
                connection = connectionFactory.CreateConnection();
            });

            if (IsConnected)
            {
                connection.ConnectionShutdown += Connection_ConnectionShutdown;
                connection.CallbackException += Connection_CallbackException;
                connection.ConnectionBlocked += Connection_ConnectionBlocked;

                //log Connected

                return true;
            }

            return false;
        }
    }
    private void Connection_ConnectionBlocked(object? sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
    {
        // log Connection_ConnectionBlocked
        if (isDisposed)
            return;

        TryConnect();
    }
    private void Connection_CallbackException(object? sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
    {
        // log Connection_CallbackException
        if (isDisposed)
            return;

        TryConnect();
    }
    private void Connection_ConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        // log Connection_ConnectionShutdown
        if (isDisposed)
            return;

        TryConnect();
    }
}
