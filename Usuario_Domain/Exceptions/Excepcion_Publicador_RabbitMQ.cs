namespace Usuario_Domain.Exceptions;

/// <summary>
/// Representa un error ocurrido durante el consumo de eventos desde RabbitMQ.
/// </summary>
public class Excepcion_Publicador_RabbitMQ : Exception
{
    public Excepcion_Publicador_RabbitMQ(string mensaje) : base(mensaje) { }

    public Excepcion_Publicador_RabbitMQ(string mensaje, Exception innerException)
        : base(mensaje, innerException) { }
}