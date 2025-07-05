namespace Usuario_Domain.Events;

public class Event_Cambiar_Password
{
    public Guid UsuarioId { get; }
    public string Password { get; }


    public Event_Cambiar_Password(Guid usuarioId, string password)
    {
        UsuarioId = usuarioId;
        Password = password;
    }
}