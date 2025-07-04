using RabbitMQ.Client.Events;
using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface IEventConsumerConnection : IDisposable
{
    void StartConsuming(
        string queueName,
        string exchangeName,
        Func<BasicDeliverEventArgs, Task<bool>> handleMessageCallback);

}