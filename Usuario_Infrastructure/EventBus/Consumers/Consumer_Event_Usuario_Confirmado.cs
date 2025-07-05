using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;

/// <summary>
/// Servicio de background que consume eventos de confirmación de usuarios desde RabbitMQ
/// y actualiza su estado de verificación en MongoDB.
/// </summary>
public class Consumer_Event_Usuario_Confirmado : BackgroundService
{
    private readonly IEventConsumerConnection _eventConsumerConnection;
    private readonly IServiceProvider _serviceProvider;

    private const string QueueName = "usuario_confirmado_queue";
    private const string ExchangeName = "usuarios_exchange";

    public Consumer_Event_Usuario_Confirmado(
        IEventConsumerConnection eventConsumerConnection,
        IServiceProvider serviceProvider)
    {
        _eventConsumerConnection = eventConsumerConnection;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Método principal que inicia el consumo del evento.
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
            throw new Excepcion_Consumidor_Rabbit("Error al iniciar el consumidor de confirmación de usuarios.", ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Maneja el mensaje recibido desde RabbitMQ y delega el procesamiento del evento.
    /// </summary>
    /// <param name="ea">Argumentos del mensaje recibido.</param>
    /// <returns>True si se maneja correctamente, false si es ignorado.</returns>
    internal async Task<bool> HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var eventType = ea.BasicProperties?.Type;

        if (eventType != null && eventType.Equals(nameof(Event_Usuario_Confirmado), StringComparison.OrdinalIgnoreCase))
        {
            return await ProcessUsuarioConfirmado(message);
        }

        return true; // Ignorado pero ACK
    }

    /// <summary>
    /// Lógica de actualización de verificación del usuario en MongoDB.
    /// </summary>
    /// <param name="message">Mensaje serializado en JSON.</param>
    /// <returns>True si se actualizó correctamente.</returns>
    /// <exception cref="Excepcion_Consumidor_Rabbit">Cuando ocurre un error de deserialización.</exception>
    /// <exception cref="Excepcion_Conexion_Mongo">Cuando falla la actualización en la base de datos.</exception>
    internal async Task<bool> ProcessUsuarioConfirmado(string message)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<Event_Usuario_Confirmado>(message);

            if (evento == null)
                throw new Excepcion_Consumidor_Rabbit("El mensaje recibido no contiene un evento válido.");

            using var scope = _serviceProvider.CreateScope();

            var collection = scope.ServiceProvider
                .GetRequiredService<IMongoClient>()
                .GetDatabase("usuarios_db")
                .GetCollection<Usuario_Mongo>("usuarios");

            var filter = Builders<Usuario_Mongo>.Filter.Eq(u => u.Id, evento.UsuarioId);
            var update = Builders<Usuario_Mongo>.Update
                .Set(u => u.Verificado, evento.Confirmado);

            var result = await collection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
                throw new Excepcion_Conexion_Mongo($"No se encontró el usuario con ID {evento.UsuarioId} o no se pudo actualizar.");

            return true;
        }
        catch (JsonException ex)
        {
            throw new Excepcion_Consumidor_Rabbit("Error al deserializar el evento de confirmación de usuario.", ex);
        }
        catch (MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo("Error al actualizar el estado del usuario en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al procesar el evento de confirmación.", ex);
        }
    }

    public override void Dispose()
    {
        _eventConsumerConnection.Dispose();
        base.Dispose();
    }
}