namespace Usuario_Domain.Events;

public class Event_Usuario_Confirmado
{
    public Guid UsuarioId { get; }
    public bool Confirmado { get; }

    public Event_Usuario_Confirmado(Guid usuarioId, bool confirmado)
    {
        UsuarioId = usuarioId;
        Confirmado = confirmado;
    }
}
