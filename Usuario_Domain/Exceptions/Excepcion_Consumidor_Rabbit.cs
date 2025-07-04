namespace Usuario_Domain.Exceptions;

/// <summary>
/// Representa un error ocurrido durante el consumo de eventos desde RabbitMQ.
/// </summary>
public class Excepcion_Consumidor_Rabbit : Exception
{
    public Excepcion_Consumidor_Rabbit(string mensaje) : base(mensaje) { }

    public Excepcion_Consumidor_Rabbit(string mensaje, Exception innerException)
        : base(mensaje, innerException) { }
}