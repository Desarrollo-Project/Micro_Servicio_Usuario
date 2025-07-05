using MediatR;
using Usuario_Application.Commands;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Modificar_Permisos_Rol : IRequestHandler<Command_Modificar_Permisos_Rol, bool>
{
    private readonly IKeycloak_Servicio _keycloak;

    public Handler_Modificar_Permisos_Rol(IKeycloak_Servicio keycloak)
    {
        _keycloak = keycloak;
    }

    public async Task<bool> Handle(Command_Modificar_Permisos_Rol request, CancellationToken cancellationToken)
    {
        await _keycloak.Modificar_Permisos_Rol(request.Payload.RolPrincipal, request.Payload.Permisos);
        return true;
    }
}
