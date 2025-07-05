using MediatR;
using Usuario_Application.Commands;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Agregar_Permiso : IRequestHandler<Command_Agregar_Permiso, bool>
{
    private readonly IKeycloak_Servicio _keycloak;

    public Handler_Agregar_Permiso(IKeycloak_Servicio keycloak)
    {
        _keycloak = keycloak;
    }

    public async Task<bool> Handle(Command_Agregar_Permiso request, CancellationToken cancellationToken)
    {
        return await _keycloak.AgregarSubrolAsync(request.Payload.RolPrincipal, request.Payload.Permiso);
    }
}
