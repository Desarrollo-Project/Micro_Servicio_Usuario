namespace Usuario_Domain.Events;

public class Event_Asignar_Rol
{
    public Guid UsuarioId { get; }
    public int RolId { get; }
    public Event_Asignar_Rol(Guid usuarioId, int rolId)
    {
        UsuarioId = usuarioId;
        RolId = rolId;
    }

}