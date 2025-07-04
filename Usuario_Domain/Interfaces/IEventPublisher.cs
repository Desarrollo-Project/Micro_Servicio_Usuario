using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface IEventPublisher
{
    void Publish<T>(T message, string exchangeName, string routingKey);
}