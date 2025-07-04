namespace Usuario_Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Password { get; set; }
    public string Correo { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public bool Verificado { get; set; }
    public string CodigoConfirmacion { get; private set; }
    public DateTime FechaExpiracionCodigo { get; private set; }
    public string? TokenRecuperacion { get; private set; }
    public DateTime? ExpiracionTokenRecuperacion { get; private set; }
    public int Rol_id { get; set; }
    public string? KeycloakId { get; set; }


    // Constructor
    public Usuario(string nombre, string apellido, string password, string correo, string telefono,
        string direccion, int rol_id)
    {
        Id = Guid.NewGuid();
        Nombre = nombre;
        Apellido = apellido;
        Password = password;
        Correo = correo;
        Telefono = telefono;
        Direccion = direccion;
        Verificado = false;
        Rol_id = rol_id;

        GenerarCodigo();
    }

    private void GenerarCodigo()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        CodigoConfirmacion = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        FechaExpiracionCodigo = DateTime.UtcNow.AddHours(24);
    }

    public void VerificarCuenta()
    {
        Verificado = true;

    }

    public void ActualizarPerfil(string nombre, string apellido, string correo, string telefono, string direccion)
    {
        Nombre = nombre;
        Apellido = apellido;
        Correo = correo;
        Telefono = telefono;
        Direccion = direccion;
    }

    public void GenerarTokenRecuperacion(TimeSpan tiempoExpiracion)
    {
        TokenRecuperacion = Guid.NewGuid().ToString("N");
        ExpiracionTokenRecuperacion = DateTime.UtcNow.Add(tiempoExpiracion);
    }

    public void LimpiarTokenRecuperacion()
    {
        TokenRecuperacion = null;
        ExpiracionTokenRecuperacion = null;
    }

    public void ActualizarPassword(string nuevaPassword)
    {
        Password = nuevaPassword;
        LimpiarTokenRecuperacion();

    }

    public void AsignarRol(int rolId) => Rol_id = rolId;

    public void Asignar_Keycloak_Id(string id) => KeycloakId = id;

}