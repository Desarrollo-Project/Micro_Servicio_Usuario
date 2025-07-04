using System.Text.Json.Serialization;

namespace Usuario_Domain.Events;

public class Event_Registrar_Actividad
{
    public Guid ActividadId { get; }
    public Guid UsuarioId { get; }

    public string TipoAccion { get; }

    public string Detalles { get; }

    public DateTime Fecha { get; }

    public Event_Registrar_Actividad(Guid actividadId, Guid usuarioId, string tipoAccion, string detalles)
    {
        ActividadId = actividadId;
        UsuarioId = usuarioId;
        TipoAccion = tipoAccion;
        Detalles = detalles;
        Fecha = DateTime.UtcNow;
    }
}