namespace Usuario_Domain.Exceptions;

/// <summary>
/// Señala un error relacionado con operaciones de persistencia o consulta en PostgreSQL.
/// </summary>
public class Excepcion_Datos_Postgre : Exception
{
    public Excepcion_Datos_Postgre(string mensaje) : base(mensaje) { }

    public Excepcion_Datos_Postgre(string mensaje, Exception innerException)
        : base(mensaje, innerException) { }
}