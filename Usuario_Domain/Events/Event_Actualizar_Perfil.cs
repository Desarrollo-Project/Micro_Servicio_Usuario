namespace Usuario_Domain.Events;

public class Event_Actualizar_Perfil
{
    public Guid UsuarioId { get; }
    public string Nombre { get; }
    public string Apellido { get; }
    public string Correo { get; }
    public string Telefono { get; }
    public string Direccion { get; }

    public Event_Actualizar_Perfil(Guid usuarioId, string nombre, string apellido, string correo, string telefono, string direccion)
    {
        UsuarioId = usuarioId;
        Nombre = nombre;
        Apellido = apellido;
        Correo = correo;
        Telefono = telefono;
        Direccion = direccion;
    }

}