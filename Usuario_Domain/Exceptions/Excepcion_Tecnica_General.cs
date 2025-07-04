namespace Usuario_Domain.Exceptions;

/// <summary>
/// Excepción genérica para representar errores técnicos no categorizados.
/// </summary>
public class Excepcion_Tecnica_General : Exception
{
    public Excepcion_Tecnica_General(string mensaje) : base(mensaje) { }

    public Excepcion_Tecnica_General(string mensaje, Exception innerException)
        : base(mensaje, innerException) { }
}