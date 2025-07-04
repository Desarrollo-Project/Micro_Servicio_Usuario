using MongoDB.Bson.Serialization.Attributes;

namespace Usuario_Domain.Entities;

public class Rol_Permiso_Mongo
{
    [BsonId]
    public int Id { get; set; }
    public int RolId { get; set; }
    public int PermisoId { get; set; }
}