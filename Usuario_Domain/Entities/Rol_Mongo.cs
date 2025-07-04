using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Usuario_Domain.Entities;

public class Rol_Mongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public int Id { get; set; }
    public string Nombre { get; set; }
}