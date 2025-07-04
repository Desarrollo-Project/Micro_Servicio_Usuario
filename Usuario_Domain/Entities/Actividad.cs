namespace Usuario_Domain.Entities;

public class Actividad
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string TipoAccion { get; private set; }
    public string Detalles { get; private set; }
    public DateTime Fecha { get; private set; }

    public Actividad(Guid usuarioId, string tipoAccion, string detalles)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        TipoAccion = tipoAccion;
        Detalles = detalles;
        Fecha = DateTime.UtcNow;
    }
}