namespace Usuario_Domain.Exceptions;

/// <summary>
/// Indica un fallo al intentar interactuar con la base de datos MongoDB.
/// </summary>
public class Excepcion_Conexion_Mongo : Exception
{
    public Excepcion_Conexion_Mongo(string mensaje) : base(mensaje) { }

    public Excepcion_Conexion_Mongo(string mensaje, Exception innerException)
        : base(mensaje, innerException) { }
}