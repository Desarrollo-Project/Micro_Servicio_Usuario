using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;
using MongoDB.Driver;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Interfaces;
using Usuario_Infrastructure.Persistance.Mongo;
using Usuario_Domain.Exceptions;

/// <summary>
/// Servicio de background que consume eventos de creación de usuario desde RabbitMQ y los persiste en MongoDB.
/// </summary>
public class Consumer_Event_Usuario_Creado : BackgroundService
{
    private readonly IEventConsumerConnection _eventConsumerConnection;
    private readonly IServiceProvider _serviceProvider;

    private const string QueueName = "usuario_creado_queue";
    private const string ExchangeName = "usuarios_exchange";

    public Consumer_Event_Usuario_Creado(
        IEventConsumerConnection eventConsumerConnection,
        IServiceProvider serviceProvider)
    {
        _eventConsumerConnection = eventConsumerConnection;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Inicia el consumo de eventos desde la cola de RabbitMQ.
    /// </summary>
    /// <param name="stoppingToken">Token de cancelación.</param>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() =>
        {
            // Puedes loguear el cierre si lo deseas
        });

        try
        {
            _eventConsumerConnection.StartConsuming(QueueName, ExchangeName, HandleMessageAsync);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General($"Error al iniciar el consumidor de eventos: {ex.Message}", ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Maneja el mensaje recibido desde RabbitMQ.
    /// </summary>
    /// <param name="ea">Argumentos del evento recibido.</param>
    /// <returns>True si el evento fue manejado correctamente, false si fue ignorado.</returns>
    internal async Task<bool> HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var eventType = ea.BasicProperties?.Type;

        if (eventType != null && eventType.Equals(nameof(Event_Usuario_Creado), StringComparison.OrdinalIgnoreCase))
        {
            return await ProcessUsuarioCreadoEventLogic(message);
        }

        // Si el tipo no nos interesa, retornamos true para ACK
        return true;
    }

    /// <summary>
    /// Lógica de procesamiento del evento de creación de usuario.
    /// </summary>
    /// <param name="message">Mensaje serializado en JSON.</param>
    /// <returns>True si se procesó correctamente.</returns>
    /// <exception cref="ErrorDeDeserializacionRabbitException">Si el mensaje no puede deserializarse.</exception>
    /// <exception cref="ErrorDeConexionMongoException">Si ocurre un problema al persistir el usuario.</exception>
    internal async Task<bool> ProcessUsuarioCreadoEventLogic(string message)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<Event_Usuario_Creado>(message);

            if (evento == null)
                throw new Excepcion_Consumidor_Rabbit("El mensaje recibido no pudo deserializarse correctamente.");

            using var scope = _serviceProvider.CreateScope();
            var mongoCreator = scope.ServiceProvider.GetRequiredService<Mongo_Crear_Usuario>();

            await mongoCreator.CrearAsync(evento);
            return true;
        }
        catch (JsonException ex)
        {
            throw new Excepcion_Consumidor_Rabbit("Error al deserializar el evento de RabbitMQ.", ex);
        }
        catch (MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo("Error al persistir el usuario en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al procesar el evento de usuario.", ex);
        }
    }

    public override void Dispose()
    {
        _eventConsumerConnection.Dispose();
        base.Dispose();
    }
}
