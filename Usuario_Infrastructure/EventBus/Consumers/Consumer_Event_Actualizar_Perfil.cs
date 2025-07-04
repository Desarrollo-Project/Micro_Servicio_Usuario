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
/// Servicio en background que consume eventos de actualización de perfil desde RabbitMQ
/// y ejecuta la actualización correspondiente en MongoDB.
/// </summary>
public class Consumer_Event_Actualizar_Perfil : BackgroundService
{
    private readonly IEventConsumerConnection _eventConsumerConnection;
    private readonly IServiceProvider _serviceProvider;

    private const string QueueName = "perfil_actualizado_queue";
    private const string ExchangeName = "usuarios_exchange";

    public Consumer_Event_Actualizar_Perfil(
        IEventConsumerConnection eventConsumerConnection,
        IServiceProvider serviceProvider)
    {
        _eventConsumerConnection = eventConsumerConnection;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Inicia el consumidor para eventos de perfil actualizado.
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => { /* Cancelación recibida */ });

        try
        {
            _eventConsumerConnection.StartConsuming(QueueName, ExchangeName, HandleMessageAsync);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Consumidor_Rabbit("Error al iniciar el consumidor de actualización de perfil.", ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Maneja los mensajes entrantes desde RabbitMQ.
    /// </summary>
    /// <param name="ea">Evento de entrega básico con los datos del mensaje.</param>
    /// <returns>True si el evento fue manejado correctamente; false si es ignorado.</returns>
    internal async Task<bool> HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var eventType = ea.BasicProperties?.Type;

        if (eventType != null && eventType.Equals(nameof(Event_Actualizar_Perfil), StringComparison.OrdinalIgnoreCase))
        {
            return await ProcessActualizarPerfil(message);
        }

        return true;
    }

    /// <summary>
    /// Procesa la lógica del evento de perfil actualizado y realiza la persistencia en MongoDB.
    /// </summary>
    /// <param name="message">Mensaje serializado como JSON.</param>
    /// <returns>True si la operación se realiza correctamente.</returns>
    /// <exception cref="Excepcion_Consumidor_Rabbit">Si falla la deserialización del mensaje.</exception>
    /// <exception cref="Excepcion_Conexion_Mongo">Si hay un error al persistir en la base de datos.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Para cualquier otro error inesperado.</exception>
    internal async Task<bool> ProcessActualizarPerfil(string message)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<Event_Actualizar_Perfil>(message);

            if (evento == null)
                throw new Excepcion_Consumidor_Rabbit("El mensaje recibido no contiene un evento de perfil válido.");

            using var scope = _serviceProvider.CreateScope();
            var mongo = scope.ServiceProvider.GetRequiredService<Mongo_Actualizar_Perfil>();

            await mongo.ActualizarAsync(evento);

            return true;
        }
        catch (JsonException ex)
        {
            throw new Excepcion_Consumidor_Rabbit("Error al deserializar el evento de perfil actualizado.", ex);
        }
        catch (MongoDB.Driver.MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo("Error al actualizar el perfil del usuario en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al procesar el evento de actualización de perfil.", ex);
        }
    }

    public override void Dispose()
    {
        _eventConsumerConnection.Dispose();
        base.Dispose();
    }
}