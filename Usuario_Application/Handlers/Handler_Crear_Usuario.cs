using MediatR;
using Usuario_Application.Commands;
using Usuario_Application.Services;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Exceptions;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Crear_Usuario : IRequestHandler<Command_Crear_Usuario, Guid>
{
    private readonly IUsuario_Repository _repository;
    private readonly IUsuarioFactory _factory;
    private readonly IEventPublisher _eventPublisher;
    private readonly IActividad_Repository _actividadRepository;
    private readonly IKeycloak_Servicio _keycloak_Servicio;
    private readonly INotificaciones_Cliente _notifServicio;

    public Handler_Crear_Usuario(
        IUsuario_Repository repository,
        IUsuarioFactory factory,
        IEventPublisher eventPublisher,
        IActividad_Repository actividadRepository,
        IKeycloak_Servicio keycloak_Servicio,
        INotificaciones_Cliente notifServicio)
    {
        _repository = repository;
        _factory = factory;
        _eventPublisher = eventPublisher;
        _actividadRepository = actividadRepository;
        _keycloak_Servicio = keycloak_Servicio;
        _notifServicio = notifServicio;
    }

    public async Task<Guid> Handle(Command_Crear_Usuario request, CancellationToken cancellationToken)
    {
        // Usar Factory para crear el usuario
        var usuario = _factory.CrearUsuario(
            request.Nombre,
            request.Apellido,
            request.Password,
            request.Correo,
            request.Telefono,
            request.Direccion,
            request.rol_id
        );

        if (usuario == null)
        {
            throw new InvalidOperationException("Error al crear usuario");
        }


        // ROLMAP PARA LOS ROLES
        var rolMap = new Dictionary<int, string>
        {
            { 1, "Administrador" },
            { 2, "Subastador" },
            { 3, "Postor" },
            { 4, "Soporte Tecnico" }
        };

        if (!rolMap.TryGetValue(usuario.Rol_id, out var nombreRol))
        {
            throw new Exception($"Rol_id '{usuario.Rol_id}' no reconocido.");
        }

        // CREAR USUARIO EN KEYCLOAK
        string keycloakId;
        try
        {
            keycloakId = await _keycloak_Servicio.Crear_Usuario_Keycloak(usuario);
            await _keycloak_Servicio.Asignar_Rol_Usuario_Keycloak(keycloakId, nombreRol);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear usuario en Keycloak: {ex.Message}");
            throw new InvalidOperationException("No se pudo crear el usuario en Keycloak");
        }

        // GUARDA ID DE KEYCLOAK
        usuario.KeycloakId = keycloakId;

        await _repository.AddAsync(usuario);

        // Publicar evento
        var evento = new Event_Usuario_Creado(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Password,
            usuario.Telefono,
            usuario.Correo,
            usuario.Direccion,
            usuario.CodigoConfirmacion,
            usuario.Rol_id
        );

        _eventPublisher.Publish(
            evento,
            exchangeName: "usuarios_exchange",
            routingKey: "usuario.creado"
        );

        try
        {
            await _notifServicio.EnviarCorreoConfirmacionAsync(
                usuario.Correo,
                usuario.Nombre,
                usuario.CodigoConfirmacion
            );
        }
        catch (Exception ex)
        {
            throw new Excepcion_Correo_Fallido($"Error al enviar correo: {ex.Message}");
        }

        // REGISTRAR ACTIVIDAD
        var actividad = new Actividad(
            usuario.Id,
            "Usuario Registrado",
            "El usuario se registró exitosamente en el sistema."
        );

        await _actividadRepository.RegistrarActividad(actividad);

        // EVENTO DE REGISTRO PARA MONGO
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

        return usuario.Id;
    }
}