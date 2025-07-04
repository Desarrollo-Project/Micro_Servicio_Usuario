using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;

namespace Usuario_Infrastructure.EventBus.Events;

/// <summary>
/// Publicador de eventos en RabbitMQ utilizando exchange tipo Fanout.
/// </summary>
public class RabbitMQEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel _channel;

    /// <summary>
    /// Constructor principal para entorno real de ejecución.
    /// </summary>
    /// <param name="host">Host del servidor RabbitMQ.</param>
    /// <param name="username">Usuario de conexión.</param>
    /// <param name="password">Contraseña de conexión.</param>
    /// <exception cref="Excepcion_Publicador_RabbitMQ">Cuando no se puede establecer la conexión o el canal.</exception>
    public RabbitMQEventPublisher(string host, string username, string password)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = username,
                Password = password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        catch (Exception ex)
        {
            throw new Excepcion_Publicador_RabbitMQ("No se pudo establecer conexión con RabbitMQ.", ex);
        }
    }

    /// <summary>
    /// Constructor para pruebas unitarias usando un canal ya mockeado.
    /// </summary>
    /// <param name="channel">Instancia simulada de IModel (RabbitMQ).</param>
    public RabbitMQEventPublisher(IModel channel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _connection = null;
    }

    /// <summary>
    /// Publica un evento al exchange especificado.
    /// </summary>
    /// <typeparam name="T">Tipo del evento.</typeparam>
    /// <param name="message">Instancia del evento serializable.</param>
    /// <param name="exchangeName">Nombre del exchange de RabbitMQ.</param>
    /// <param name="routingKey">RoutingKey (se ignora en Fanout pero requerido por API).</param>
    /// <exception cref="Excepcion_Publicador_RabbitMQ">Si ocurre un error durante la publicación.</exception>
    public void Publish<T>(T message, string exchangeName, string routingKey)
    {
        try
        {
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: true);

            var eventMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(eventMessage);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Type = typeof(T).Name;

            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );
        }
        catch (Exception ex)
        {
            throw new Excepcion_Publicador_RabbitMQ("Error al publicar el evento en RabbitMQ.", ex);
        }
    }

    /// <summary>
    /// Libera la conexión y el canal de RabbitMQ.
    /// </summary>
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}