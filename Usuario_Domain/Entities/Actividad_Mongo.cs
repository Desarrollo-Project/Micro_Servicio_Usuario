using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Usuario_Domain.Entities;

public class Actividad_Mongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    [BsonRepresentation(BsonType.String)]
    public Guid UsuarioId { get; set; }
    public string TipoAccion { get; set; }
    public string Detalles { get; set; }
    public DateTime Fecha { get; set; }
}