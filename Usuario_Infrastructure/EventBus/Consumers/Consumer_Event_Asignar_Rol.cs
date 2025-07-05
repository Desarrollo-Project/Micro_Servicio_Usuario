using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using MongoDB.Driver;
using Usuario_Domain.Events;
using Usuario_Domain.Entities;
using Usuario_Domain.Interfaces;
using Usuario_Domain.Exceptions;

/// <summary>
/// Servicio de background que consume eventos de asignación de rol desde RabbitMQ
/// y actualiza el usuario correspondiente en MongoDB.
/// </summary>
public class Consumer_Event_Asignar_Rol : BackgroundService
{
    private readonly IEventConsumerConnection _eventConsumerConnection;
    private readonly IServiceProvider _serviceProvider;

    private const string QueueName = "rol_asignado_queue";
    private const string ExchangeName = "usuarios_exchange";

    public Consumer_Event_Asignar_Rol(
        IEventConsumerConnection eventConsumerConnection,
        IServiceProvider serviceProvider)
    {
        _eventConsumerConnection = eventConsumerConnection;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Inicia el consumidor de eventos para asignar roles.
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
            throw new Excepcion_Consumidor_Rabbit("Error al iniciar el consumidor de asignación de rol.", ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Maneja el mensaje recibido desde RabbitMQ.
    /// </summary>
    /// <param name="ea">Mensaje recibido con metadata.</param>
    /// <returns>True si el evento fue procesado, false si fue ignorado.</returns>
    internal async Task<bool> HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var eventType = ea.BasicProperties?.Type;

        if (eventType != null && eventType.Equals(nameof(Event_Asignar_Rol), StringComparison.OrdinalIgnoreCase))
        {
            return await ProcessRolAsignado(message);
        }

        return true;
    }

    /// <summary>
    /// Procesa el evento de asignación de rol actualizando el usuario en MongoDB.
    /// </summary>
    /// <param name="message">Evento serializado como JSON.</param>
    /// <returns>True si el rol fue asignado correctamente.</returns>
    /// <exception cref="Excepcion_Consumidor_Rabbit">Si hay errores de deserialización.</exception>
    /// <exception cref="Excepcion_Conexion_Mongo">Si falla la persistencia en la base de datos.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Si ocurre un error inesperado.</exception>
    internal async Task<bool> ProcessRolAsignado(string message)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<Event_Asignar_Rol>(message);

            if (evento == null)
                throw new Excepcion_Consumidor_Rabbit("El evento recibido de asignación de rol no es válido.");

            using var scope = _serviceProvider.CreateScope();
            var collection = scope.ServiceProvider
                .GetRequiredService<IMongoClient>()
                .GetDatabase("usuarios_db")
                .GetCollection<Usuario_Mongo>("usuarios");

            var filter = Builders<Usuario_Mongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
            var update = Builders<Usuario_Mongo>.Update.Set(u => u.Rol_id, evento.RolId);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
                throw new Excepcion_Conexion_Mongo($"No se pudo actualizar el rol para el usuario con ID {evento.UsuarioId}.");

            return true;
        }
        catch (JsonException ex)
        {
            throw new Excepcion_Consumidor_Rabbit("Error al deserializar el evento de asignación de rol.", ex);
        }
        catch (MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo("Error al actualizar el rol en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al procesar el evento de asignación de rol.", ex);
        }
    }

    public override void Dispose()
    {
        _eventConsumerConnection.Dispose();
        base.Dispose();
    }
}