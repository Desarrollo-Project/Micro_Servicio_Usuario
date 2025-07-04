namespace Usuario_Domain.Exceptions;

/// <summary>
/// Indica un fallo al intentar interactuar con la base de datos MongoDB.
/// </summary>
public class Excepcion_Correo_Fallido : Exception
{
    public Excepcion_Correo_Fallido(string mensaje) : base(mensaje) { }

    public Excepcion_Correo_Fallido(string mensaje, Exception innerException)
        : base(mensaje, innerException) { }
}