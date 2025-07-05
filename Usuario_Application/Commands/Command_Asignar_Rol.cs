using MediatR;

namespace Usuario_Application.Commands;

public record Command_Asignar_Rol(
    Guid UsuarioId,
    int RolId
) : IRequest<Unit>;