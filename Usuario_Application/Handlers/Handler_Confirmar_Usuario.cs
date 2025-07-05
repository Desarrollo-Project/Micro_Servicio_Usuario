using MediatR;
using Usuario_Application.Commands;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Confirmar_Usuario: IRequestHandler<Command_Confirmar_Usuario, bool>
{
    private readonly IUsuario_Repository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IActividad_Repository _actividadRepository;

    public Handler_Confirmar_Usuario(
        IUsuario_Repository repository,
        IEventPublisher eventPublisher,
        IActividad_Repository actividadRepository)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _actividadRepository = actividadRepository;
    }

    public async Task<bool> Handle(Command_Confirmar_Usuario request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByEmail(request.Email);

        if (usuario == null ||
            usuario.CodigoConfirmacion != request.Codigo ||
            usuario.FechaExpiracionCodigo < DateTime.UtcNow)
        {
            return false;
        }

        usuario.VerificarCuenta();
        await _repository.UpdateAsync(usuario);

        // PUBLICAR EVENTO EN MONGO
        _eventPublisher.Publish(
            new Event_Usuario_Confirmado(usuario.Id, usuario.Verificado),
            exchangeName: "usuarios_exchange",
            routingKey: "usuario.confirmado"
        );

        // REGISTRAR ACTIVIDAD
        var actividad = new Actividad(
            usuario.Id,
            "Cuenta Confirmada",
            "El usuario ha confirmado su cuenta exitosamente."
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