using MediatR;
using Usuario_Application.Commands;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Asignar_Rol : IRequestHandler<Command_Asignar_Rol, Unit>
{
    private readonly IRol_Repositorio _rol_Repositorio;
    private readonly IActividad_Repository _actividadRepositorio;
    private readonly IUsuario_Repository _usuarioRepositorio;
    private readonly IEventPublisher _eventPublisher;
    private readonly IKeycloak_Servicio _keycloakServicio;

    public Handler_Asignar_Rol(
        IRol_Repositorio rol_Repositorio,
        IActividad_Repository actividadRepositorio,
        IUsuario_Repository usuarioRepositorio,
        IEventPublisher eventPublisher,
        IKeycloak_Servicio keycloakServicio)
    {
        _rol_Repositorio = rol_Repositorio;
        _actividadRepositorio = actividadRepositorio;
        _usuarioRepositorio = usuarioRepositorio;
        _eventPublisher = eventPublisher;
        _keycloakServicio = keycloakServicio;
    }

    public async Task<Unit> Handle(Command_Asignar_Rol request, CancellationToken cancellationToken)
    {
        var rol = await _rol_Repositorio.GetByIdAsync(request.RolId);
        if (rol == null)
            throw new Exception("Rol no encontrado");

        var usuario = await _usuarioRepositorio.GetByIdAsync(request.UsuarioId);
        if (usuario == null)
            throw new Exception("Usuario no encontrado");

        usuario.AsignarRol(request.RolId);


        // ASIGNAR ROL EN KEYCLOAK
        await _keycloakServicio.Asignar_Rol_Usuario_Keycloak(usuario.KeycloakId, rol.Nombre);

        await _usuarioRepositorio.UpdateAsync(usuario);

        // PUBLICAR EVENTO EN MONGO
        try
        {
            _eventPublisher.Publish(
                new Event_Asignar_Rol(
                    request.UsuarioId,
                    request.RolId
                ),
                exchangeName: "usuarios_exchange",
                routingKey: "rol.asignado"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al publicar evento: {ex.Message}");
        }

        // REGISTRAR ACTIVIDAD
        var actividad = new Actividad(
            usuario.Id,
            "Asignación de rol",
            $" Al Usuario {usuario.Correo} se asigno el Rol {rol.Nombre}"
        );

        await _actividadRepositorio.RegistrarActividad(actividad);

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

        return Unit.Value;
    }
}