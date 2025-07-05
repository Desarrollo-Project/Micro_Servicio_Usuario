using MediatR;
using Usuario_Application.Commands;
using Usuario_Application.DTOs;
using Usuario_Application.Queries;
using Usuario_Domain.Entities;
using Usuario_Domain.Events;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Obtener_Roles_y_Permisos : IRequestHandler<Query_Obtener_Roles_y_Permisos, List<Dto_Rol_Con_Permisos>>
{
    private readonly IKeycloak_Servicio _keycloak;

    public Handler_Obtener_Roles_y_Permisos(IKeycloak_Servicio keycloak)
    {
        _keycloak = keycloak;
    }

    public async Task<List<Dto_Rol_Con_Permisos>> Handle(Query_Obtener_Roles_y_Permisos request, CancellationToken cancellationToken)
    {
        var roles = await _keycloak.ObtenerRolesCompuestos();

        return roles.Select(r => new Dto_Rol_Con_Permisos
        {
            Nombre = r.name,
            Permisos = r.compositeRoles.Select(p => p.name).ToList()
        }).ToList();
    }
}