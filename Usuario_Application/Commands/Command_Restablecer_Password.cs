using MediatR;

namespace Usuario_Application.Commands;

public record Command_Restablecer_Password(
    string Token,
    string NuevaPassword
) : IRequest<bool>;