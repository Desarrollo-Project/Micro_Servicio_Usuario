using System.Security;
using MediatR;
using Usuario_Application.Commands;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Cambiar_Password: IRequestHandler<Command_Cambiar_Password, bool>
{
    private readonly IUsuario_Repository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IActividad_Repository _actividadRepository;
    private readonly IKeycloak_Servicio _keycloak_Servicio;
    private readonly INotificaciones_Cliente _notificacionServicio;

    public Handler_Cambiar_Password(
        IUsuario_Repository repository,
        IEventPublisher eventPublisher,
        IActividad_Repository actividadRepository,
        IKeycloak_Servicio keycloak_Servicio,
        INotificaciones_Cliente notificacionServicio)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _actividadRepository = actividadRepository;
        _keycloak_Servicio = keycloak_Servicio;
        _notificacionServicio = notificacionServicio;
    }

    public async Task<bool> Handle(Command_Cambiar_Password request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByIdAsync(request.UsuarioId);

        if (usuario == null)
        {
            return false;
        }

        // VALIDACION DE PASSWORD
        if (usuario.Password != request.PasswordActual)
            throw new SecurityException("Contraseña actual incorrecta");
                // Excepcion Personalizada

        usuario.Password = request.NuevoPassword;


        // CAMBIAR PASSWORD EN KEYCLOAK
        await _keycloak_Servicio.Actualizar_Usuario_Keycloak(usuario);

        await _repository.UpdateAsync(usuario);

        // NOTIFICACION
        try
        {
            await _notificacionServicio.EnviarNotificacionCambioClaveAsync(
                usuario.Correo,
                usuario.Nombre
            );
        }
        catch (Exception ex)
        {
            throw new Excepcion_Correo_Fallido($"Error al enviar correo: {ex.Message}");
        }


        // PUBLICAR EVENTO EN MONGO
        _eventPublisher.Publish(
            new Event_Cambiar_Password(usuario.Id, usuario.Password),
            exchangeName: "usuarios_exchange",
            routingKey: "usuario.password.cambiado"
        );

        // REGISTRAR ACTIVIDAD
        var actividad = new Actividad(
            usuario.Id,
            "Cambio de Contraseña",
            "El usuario ha cambiado su contraseña."
        );

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