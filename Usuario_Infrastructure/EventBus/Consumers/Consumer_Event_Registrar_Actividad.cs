using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Usuario_Domain.Events;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;
using Usuario_Infrastructure.Persistance.Mongo;

/// <summary>
/// Servicio de background que consume eventos de actividad registrada desde RabbitMQ
/// y persiste la información en MongoDB.
/// </summary>
public class Consumer_Event_Registrar_Actividad : BackgroundService
{
    private readonly IEventConsumerConnection _eventConsumerConnection;
    private readonly IServiceProvider _serviceProvider;

    private const string QueueName = "actividad_registrada_queue";
    private const string ExchangeName = "usuarios_exchange";

    public Consumer_Event_Registrar_Actividad(
        IEventConsumerConnection eventConsumerConnection,
        IServiceProvider serviceProvider)
    {
        _eventConsumerConnection = eventConsumerConnection;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Inicia el consumo del evento desde la cola de RabbitMQ.
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => { /* Cancelación del servicio */ });

        try
        {
            _eventConsumerConnection.StartConsuming(QueueName, ExchangeName, HandleMessageAsync);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Consumidor_Rabbit("Error al iniciar el consumidor de actividad registrada.", ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Procesa el mensaje recibido desde RabbitMQ.
    /// </summary>
    /// <param name="ea">Argumentos del mensaje recibido.</param>
    /// <returns>True si el mensaje fue manejado correctamente, false si fue ignorado.</returns>
    internal async Task<bool> HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var eventType = ea.BasicProperties?.Type;

        if (eventType != null && eventType.Equals(nameof(Event_Registrar_Actividad), StringComparison.OrdinalIgnoreCase))
        {
            return await ProcessActividadEventLogic(message);
        }

        return true; // Evento ignorado pero ACKed
    }

    /// <summary>
    /// Lógica de procesamiento del evento de registro de actividad.
    /// </summary>
    /// <param name="message">Mensaje serializado como JSON.</param>
    /// <returns>True si el evento fue procesado y almacenado correctamente.</returns>
    /// <exception cref="Excepcion_Consumidor_Rabbit">Cuando hay problemas de deserialización.</exception>
    /// <exception cref="Excepcion_Conexion_Mongo">Cuando no se puede persistir la actividad en Mongo.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Cuando ocurre un error inesperado.</exception>
    internal async Task<bool> ProcessActividadEventLogic(string message)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<Event_Registrar_Actividad>(message);

            if (evento == null)
                throw new Excepcion_Consumidor_Rabbit("El mensaje recibido no contiene una actividad válida.");

            using var scope = _serviceProvider.CreateScope();
            var mongoRepo = scope.ServiceProvider.GetRequiredService<Mongo_Registrar_Actividad>();

            await mongoRepo.RegistrarAsync(evento);

            return true;
        }
        catch (JsonException ex)
        {
            throw new Excepcion_Consumidor_Rabbit("Error al deserializar el evento de actividad registrada.", ex);
        }
        catch (MongoDB.Driver.MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo("Error al guardar la actividad en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al procesar el evento de actividad.", ex);
        }
    }

    public override void Dispose()
    {
        _eventConsumerConnection.Dispose();
        base.Dispose();
    }
}