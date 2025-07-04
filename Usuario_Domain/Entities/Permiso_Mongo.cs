using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Usuario_Domain.Entities;

public class Permiso_Mongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public string Descripcion { get; set; }
}