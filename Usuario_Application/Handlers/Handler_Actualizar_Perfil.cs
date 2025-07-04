using MediatR;
using Usuario_Application.Commands;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Actualizar_Perfil: IRequestHandler<Command_Actualizar_Perfil, bool>
{
    private readonly IUsuario_Repository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IActividad_Repository _actividadRepository;
    private readonly IKeycloak_Servicio _keycloak_Servicio;

    public Handler_Actualizar_Perfil(
        IUsuario_Repository repository,
        IEventPublisher eventPublisher,
        IActividad_Repository actividadRepository,
        IKeycloak_Servicio keycloak_Servicio)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _actividadRepository = actividadRepository;
        _keycloak_Servicio = keycloak_Servicio;
    }

    public async Task<bool> Handle(Command_Actualizar_Perfil request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByIdAsync(request.UsuarioId);

        if (usuario == null)
        {
            return false;
        }

        usuario.ActualizarPerfil(
            request.Nombre,
            request.Apellido,
            request.Correo,
            request.Telefono,
            request.Direccion
        );

        // ACTUALIZAR EN KEYCLOAK
        await _keycloak_Servicio.Actualizar_Usuario_Keycloak(usuario);

        await _repository.UpdateAsync(usuario);

        // PUBLICAR EVENTO EN MONGO
        try
        {
            _eventPublisher.Publish(
                new Event_Actualizar_Perfil(
                    request.UsuarioId,
                    request.Nombre,
                    request.Apellido,
                    request.Correo,
                    request.Telefono,
                    request.Direccion
                ),
                exchangeName: "usuarios_exchange",
                routingKey: "perfil.actualizado"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al publicar evento: {ex.Message}");
        }


        // REGISTRAR ACTIVIDAD
        var actividad = new Actividad(
            request.UsuarioId,
            "Perfil Actualizado",
            "El usuario ha actualizado su perfil."
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