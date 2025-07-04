using MongoDB.Driver;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Exceptions;

namespace Usuario_Infrastructure.Persistance.Mongo;

/// <summary>
/// Servicio responsable de registrar actividades de usuarios en MongoDB.
/// </summary>
public class Mongo_Registrar_Actividad
{
    private readonly IMongoCollection<Actividad_Mongo> _coleccion;

    /// <summary>
    /// Inicializa la colección de actividades desde el cliente de MongoDB.
    /// </summary>
    /// <param name="mongoClient">Instancia del cliente de Mongo.</param>
    public Mongo_Registrar_Actividad(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("usuarios_db");
        _coleccion = database.GetCollection<Actividad_Mongo>("actividades");
    }

    /// <summary>
    /// Inserta un nuevo documento de actividad en la colección.
    /// </summary>
    /// <param name="evento">Evento que contiene los datos de la actividad.</param>
    /// <returns>Una tarea asincrónica.</returns>
    /// <exception cref="Excepcion_Conexion_Mongo">Si ocurre un error durante la inserción.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Si ocurre un error inesperado.</exception>
    public async Task RegistrarAsync(Event_Registrar_Actividad evento)
    {
        try
        {
            var actividad = new Actividad_Mongo
            {
                Id = evento.ActividadId,
                UsuarioId = evento.UsuarioId,
                TipoAccion = evento.TipoAccion,
                Detalles = evento.Detalles,
                Fecha = evento.Fecha
            };

            await _coleccion.InsertOneAsync(actividad);
        }
        catch (MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo("Error al insertar la actividad en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al registrar la actividad.", ex);
        }
    }
}