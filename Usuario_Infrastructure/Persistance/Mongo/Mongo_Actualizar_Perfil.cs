using MongoDB.Driver;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Exceptions;

namespace Usuario_Infrastructure.Persistance.Mongo;

/// <summary>
/// Servicio encargado de actualizar el perfil de un usuario en MongoDB.
/// </summary>
public class Mongo_Actualizar_Perfil
{
    private readonly IMongoCollection<Usuario_Mongo> _coleccion;

    /// <summary>
    /// Inicializa la colección de usuarios desde el cliente de MongoDB.
    /// </summary>
    /// <param name="mongoClient">Cliente de MongoDB inyectado por DI.</param>
    public Mongo_Actualizar_Perfil(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("usuarios_db");
        _coleccion = database.GetCollection<Usuario_Mongo>("usuarios");
    }

    /// <summary>
    /// Actualiza los datos del perfil de un usuario en la base de datos.
    /// </summary>
    /// <param name="evento">Evento con los datos actualizados del perfil.</param>
    /// <returns>Una tarea asincrónica.</returns>
    /// <exception cref="Excepcion_Conexion_Mongo">Si no se actualiza ningún documento.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Si ocurre un error inesperado.</exception>
    public async Task ActualizarAsync(Event_Actualizar_Perfil evento)
    {
        try
        {
            var filter = Builders<Usuario_Mongo>.Filter.Eq(u => u.Id, evento.UsuarioId);

            var update = Builders<Usuario_Mongo>.Update
                .Set(u => u.Nombre, evento.Nombre)
                .Set(u => u.Apellido, evento.Apellido)
                .Set(u => u.Correo, evento.Correo)
                .Set(u => u.Telefono, evento.Telefono)
                .Set(u => u.Direccion, evento.Direccion);

            var result = await _coleccion.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Excepcion_Conexion_Mongo($"No se actualizó ningún documento para el usuario ID {evento.UsuarioId}.");
            }
        }
        catch (MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo("Error al actualizar el perfil del usuario en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al actualizar el perfil del usuario.", ex);
        }
    }
}