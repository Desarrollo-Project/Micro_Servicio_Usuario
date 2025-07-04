using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;

namespace Producto.Infrastructure.EventBus.Consumer
{
    /// <summary>
    /// Encapsula la conexión y canal de RabbitMQ para consumir eventos de forma asíncrona.
    /// </summary>
    public class RabbitMQEventConsumerConnection : IEventConsumerConnection, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        /// <summary>
        /// Inicializa una nueva instancia estableciendo conexión y canal con RabbitMQ.
        /// </summary>
        /// <param name="host">Hostname del servidor RabbitMQ.</param>
        /// <param name="user">Usuario de autenticación.</param>
        /// <param name="pass">Contraseña de autenticación.</param>
        /// <exception cref="Excepcion_Conexion_RabbitMQ">Si no se puede establecer la conexión.</exception>
        public RabbitMQEventConsumerConnection(string host, string user, string pass)
        {
            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = pass,
                DispatchConsumersAsync = true
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (Exception ex)
            {
                throw new Excepcion_Conexion_RabbitMQ("No se pudo establecer la conexión con RabbitMQ.", ex);
            }
        }

        /// <summary>
        /// Configura la cola y comienza a consumir mensajes de forma asíncrona.
        /// </summary>
        /// <param name="queueName">Nombre de la cola.</param>
        /// <param name="exchangeName">Nombre del exchange.</param>
        /// <param name="handleMessageCallback">Callback que procesa el mensaje.</param>
        /// <exception cref="Excepcion_Tecnica_General">Si falla la configuración del consumidor.</exception>
        public void StartConsuming(
            string queueName,
            string exchangeName,
            Func<BasicDeliverEventArgs, Task<bool>> handleMessageCallback)
        {
            try
            {
                _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);
                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "");
                _channel.BasicQos(0, 1, false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (sender, ea) =>
                {
                    bool processedSuccessfully = false;
                    try
                    {
                        processedSuccessfully = await handleMessageCallback(ea);
                    }
                    catch (Exception ex)
                    {
                        // Aquí puedes loguear internamente si tienes sistema de logging centralizado
                        processedSuccessfully = false;
                    }
                    finally
                    {
                        if (processedSuccessfully)
                        {
                            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        else
                        {
                            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                        }
                    }
                };

                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            }
            catch (Exception ex)
            {
                throw new Excepcion_Tecnica_General($"Error al iniciar el consumo desde la cola '{queueName}'.", ex);
            }
        }

        /// <summary>
        /// Libera los recursos de conexión y canal de RabbitMQ.
        /// </summary>
        public void Dispose()
        {
            try { _channel?.Close(); } catch { /* Opcional: Log interno */ }
            _channel?.Dispose();

            try { _connection?.Close(); } catch { /* Opcional: Log interno */ }
            _connection?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}