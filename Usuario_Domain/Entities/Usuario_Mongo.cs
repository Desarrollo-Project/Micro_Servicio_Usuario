using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Usuario_Domain.Entities;

public class Usuario_Mongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Password { get; set; }
    public string Correo { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public bool Verificado { get; set; }
    public int Rol_id { get; set; }
}