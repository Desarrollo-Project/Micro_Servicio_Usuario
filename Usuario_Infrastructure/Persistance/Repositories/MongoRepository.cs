using MongoDB.Driver;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;

namespace Usuario_Infrastructure.Persistance.Repositories;

/// <summary>
/// Repositorio genérico para operaciones básicas sobre colecciones MongoDB.
/// </summary>
/// <typeparam name="TDocument">Tipo de documento gestionado.</typeparam>
public class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : class
{
    private readonly IMongoCollection<TDocument> _collection;

    /// <summary>
    /// Inicializa la colección especificada desde la base de datos proporcionada.
    /// </summary>
    /// <param name="mongoClient">Cliente Mongo inyectado vía DI.</param>
    /// <param name="databaseName">Nombre de la base de datos.</param>
    /// <param name="collectionName">Nombre de la colección.</param>
    public MongoRepository(IMongoClient mongoClient, string databaseName, string collectionName)
    {
        var database = mongoClient.GetDatabase(databaseName);
        _collection = database.GetCollection<TDocument>(collectionName);
    }

    /// <summary>
    /// Obtiene un documento por su ID.
    /// </summary>
    /// <param name="id">Identificador del documento.</param>
    /// <returns>Instancia del documento o null.</returns>
    /// <exception cref="Excepcion_Conexion_Mongo">Si ocurre un fallo en la consulta.</exception>
    public async Task<TDocument> GetByIdAsync(string id)
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Eq("_id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo($"Error al obtener el documento con ID '{id}' en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al buscar un documento por ID.", ex);
        }
    }

    /// <summary>
    /// Realiza una consulta personalizada usando un filtro de MongoDB.
    /// </summary>
    /// <param name="filter">Filtro para la búsqueda.</param>
    /// <returns>Lista de documentos que cumplen la condición.</returns>
    /// <exception cref="Excepcion_Conexion_Mongo">Si falla la búsqueda en MongoDB.</exception>
    public async Task<IEnumerable<TDocument>> FindAsync(FilterDefinition<TDocument> filter)
    {
        try
        {
            return await _collection.Find(filter).ToListAsync();
        }
        catch (MongoException ex)
        {
            throw new Excepcion_Conexion_Mongo("Error al ejecutar la consulta en MongoDB.", ex);
        }
        catch (Exception ex)
        {
            throw new Excepcion_Tecnica_General("Error inesperado al ejecutar la consulta en MongoDB.", ex);
        }
    }
}