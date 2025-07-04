namespace Usuario_Domain.Events;

public class Event_Usuario_Creado
{
    public Guid Id { get; }
    public string Nombre { get; }
    public string Apellido { get; }
    public string Password { get; }
    public string Telefono { get; }
    public string Correo { get; }
    public string Direccion { get; }
    public string CodigoConfirmacion { get; }
    public int Rol_Id { get; }

    public Event_Usuario_Creado(Guid id, string nombre, string apellido, string password, string telefono, string correo, string direccion, string codigoConfirmacion, int rol_Id)
    {
        Id = id;
        Nombre = nombre;
        Apellido = apellido;
        Password = password;
        Telefono = telefono;
        Correo = correo;
        Direccion = direccion;
        CodigoConfirmacion = codigoConfirmacion;
        Rol_Id = rol_Id;
    }
}