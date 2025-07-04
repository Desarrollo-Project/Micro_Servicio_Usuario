using MongoDB.Driver;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Exceptions;

namespace Usuario_Infrastructure.Persistance.Mongo;

/// <summary>
/// Servicio de persistencia que crea un nuevo documento de usuario en MongoDB.
/// </summary>
public class Mongo_Crear_Usuario
{
    private readonly IMongoCollection<Usuario_Mongo> _usuariosCollection;

    /// <summary>
    /// Inicializa la colección de usuarios utilizando el contexto de MongoDB.
    /// </summary>
    /// <param name="mongoDatabase">Instancia válida de la base de datos.</param>
    public Mongo_Crear_Usuario(IMongoDatabase mongoDatabase)
    {
        if (mongoDatabase == null)
            throw new ArgumentNullException(nameof(mongoDatabase));

        _usuariosCollection = mongoDatabase.GetCollection<Usuario_Mongo>("usuarios");
    }

    /// <summary>
    /// Crea un nuevo usuario en la colección Mongo a partir del evento recibido.
    /// </summary>
    /// <param name="evento">Evento con los datos del usuario a insertar.</param>
    /// <returns>Una tarea asincrónica que representa la operación.</returns>
    /// <exception cref="ArgumentNullException">Si el evento es null.</exception>
    /// <exception cref="Excepcion_Conexion_Mongo">Si ocurre un error en la inserción con MongoDB.</exception>
    /// <exception cref="Excepcion_Tecnica_General">Si ocurre un error no controlado.</exception>
    public virtual async Task CrearAsync(Event_Usuario_Creado evento)
    {
        if (evento == null)
            throw new ArgumentNullException(nameof(evento), "El evento de creación de usuario no puede ser null.");

        try
        {
            var usuarioMongo = new Usuario_Mongo
            {
                Id = evento.Id,
                Nombre = evento.Nombre,
                Apellido = evento.Apellido,
                Password = evento.Password,
                Correo = evento.Correo,
                Telefono = evento.Telefono,
                Direccion = evento.Direccion,
                Rol_id = evento.Rol_Id,
                Verificado = false
            };

            await _usuariosCollection.InsertOneAsync(usuarioMongo);
        }
        catch (MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo($"Error al insertar el usuario ID {evento.Id} en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General($"Error inesperado al crear el usuario ID {evento.Id}.", ex);
        }
    }
}