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
/// Servicio en background que consume eventos de cambio de contraseña desde RabbitMQ
/// y actualiza la información del usuario en MongoDB.
/// </summary>
public class Consumer_Event_Cambiar_Password : BackgroundService
{
    private readonly IEventConsumerConnection _eventConsumerConnection;
    private readonly IServiceProvider _serviceProvider;

    private const string QueueName = "usuario_password_cambiado_queue";
    private const string ExchangeName = "usuarios_exchange";

    public Consumer_Event_Cambiar_Password(
        IEventConsumerConnection eventConsumerConnection,
        IServiceProvider serviceProvider)
    {
        _eventConsumerConnection = eventConsumerConnection;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Inicia el consumo del evento de cambio de contraseña.
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
            throw new Excepcion_Consumidor_Rabbit("Error al iniciar el consumidor de cambio de contraseña.", ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Maneja el mensaje recibido desde RabbitMQ.
    /// </summary>
    /// <param name="ea">Mensaje recibido con los metadatos del evento.</param>
    /// <returns>True si el mensaje fue procesado, false si es ignorado.</returns>
    internal async Task<bool> HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var eventType = ea.BasicProperties?.Type;

        if (eventType != null && eventType.Equals(nameof(Event_Cambiar_Password), StringComparison.OrdinalIgnoreCase))
        {
            return await ProcessCambioPassword(message);
        }

        return true; // ACK para eventos ignorados
    }

    /// <summary>
    /// Procesa el evento de cambio de contraseña actualizando el registro correspondiente.
    /// </summary>
    /// <param name="message">Mensaje serializado en JSON.</param>
    /// <returns>True si se actualizó exitosamente.</returns>
    /// <exception cref="Excepcion_Consumidor_Rabbit">Cuando el mensaje no puede deserializarse.</exception>
    /// <exception cref="Excepcion_Conexion_Mongo">Cuando no se puede persistir el cambio.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Para cualquier otro error inesperado.</exception>
    internal async Task<bool> ProcessCambioPassword(string message)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<Event_Cambiar_Password>(message);

            if (evento == null)
                throw new Excepcion_Consumidor_Rabbit("El evento de cambio de contraseña está mal formado o es null.");

            using var scope = _serviceProvider.CreateScope();
            var collection = scope.ServiceProvider
                .GetRequiredService<IMongoClient>()
                .GetDatabase("usuarios_db")
                .GetCollection<Usuario_Mongo>("usuarios");

            var filter = Builders<Usuario_Mongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
            var update = Builders<Usuario_Mongo>.Update.Set(u => u.Password, evento.Password);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
                throw new Excepcion_Conexion_Mongo($"No se encontró el usuario con ID {evento.UsuarioId} o no se pudo actualizar la contraseña.");

            return true;
        }
        catch (JsonException ex)
        {
            throw new Excepcion_Consumidor_Rabbit("Error al deserializar el evento de cambio de contraseña.", ex);
        }
        catch (MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo("Fallo al acceder a MongoDB durante la actualización de contraseña.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al procesar el cambio de contraseña.", ex);
        }
    }

    public override void Dispose()
    {
        _eventConsumerConnection.Dispose();
        base.Dispose();
    }
}