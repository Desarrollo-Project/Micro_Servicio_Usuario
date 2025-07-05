using MediatR;
using Usuario_Application.Commands;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Generar_Token_Recuperacion: IRequestHandler<Command_Generar_Token_Recuperacion, bool>
{
    private readonly IUsuario_Repository _repository;
    private readonly IActividad_Repository _actividadRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificaciones_Cliente _notificServicio;

    public Handler_Generar_Token_Recuperacion(
        IUsuario_Repository repository,
        IActividad_Repository actividadRepository,
        IEventPublisher eventPublisher,
        INotificaciones_Cliente notificServicio)
    {
        _repository = repository;
        _actividadRepository = actividadRepository;
        _eventPublisher = eventPublisher;
        _notificServicio = notificServicio;
    }

    public async Task<bool> Handle(Command_Generar_Token_Recuperacion request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByEmail(request.Correo);
        if (usuario == null) return false;

        usuario.GenerarTokenRecuperacion(TimeSpan.FromHours(24));
        await _repository.UpdateAsync(usuario);

        try
        {
            await _notificServicio.EnviarTokenRecuperacionAsync(
                usuario.Correo,
                usuario.Nombre,
                usuario.TokenRecuperacion
            );
        }
        catch (Exception ex)
        {
            throw new Excepcion_Correo_Fallido($"Error al enviar correo: {ex.Message}");
        }

        // REGISTRAR ACTIVIDAD
        var actividad = new Actividad(
            usuario.Id,
            "Generación de Token de Recuperación",
            "El usuario solicitó recuperación de contraseña");

        await _actividadRepository.RegistrarActividad(actividad);

        // PUBLICAR EVENTO EN MONGO
        _eventPublisher.Publish(
            new Event_Registrar_Actividad(
                actividad.Id,
                actividad.UsuarioId,
                actividad.TipoAccion,
                actividad.Detalles
            ),
            exchangeName: "usuarios_exchange",
            routingKey: "actividad.registrada"
        );

        return true;
    }
}