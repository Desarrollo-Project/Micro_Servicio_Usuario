using MediatR;

namespace Usuario_Application.Commands;

public record Command_Cambiar_Password(
    Guid UsuarioId,
    string PasswordActual,
    string NuevoPassword
) : IRequest<bool>;