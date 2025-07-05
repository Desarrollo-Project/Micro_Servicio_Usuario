using MediatR;
using Usuario_Application.Commands;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Restablecer_Password: IRequestHandler<Command_Restablecer_Password, bool>
{
    private readonly IUsuario_Repository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IActividad_Repository _actividadRepository;
    private readonly IKeycloak_Servicio _keycloak_Servicio;

    public Handler_Restablecer_Password(
        IUsuario_Repository repository, IEventPublisher eventPublisher,
        IActividad_Repository actividadRepository, IKeycloak_Servicio keycloak_Servicio)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _actividadRepository = actividadRepository;
        _keycloak_Servicio = keycloak_Servicio;
    }

    public async Task<bool> Handle(Command_Restablecer_Password request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByTokenRecuperacion(request.Token);

        if (usuario == null || usuario.ExpiracionTokenRecuperacion == null || usuario.ExpiracionTokenRecuperacion < DateTime.UtcNow)
        {
            return false;
        }


        usuario.ActualizarPassword(request.NuevaPassword);

        await _repository.UpdateAsync(usuario);

        // ACTUALIZAR USUARIO EN KEYCLOAK
        await _keycloak_Servicio.Actualizar_Usuario_Keycloak(usuario);

        // PUBLICAR EVENTO EN MONGO
        _eventPublisher.Publish(
            new Event_Cambiar_Password(usuario.Id, usuario.Password), 
            exchangeName: "usuarios_exchange", 
            routingKey: "usuario.password.cambiado"
            );


        // REGISTRAR ACTIVIDAD
        var actividad = new Actividad(
            usuario.Id,
            "Restablecimiento de Contraseña",
            "El usuario ha restablecido su contraseña exitosamente."
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