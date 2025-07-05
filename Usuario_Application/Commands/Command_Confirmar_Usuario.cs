using MediatR;

namespace Usuario_Application.Commands;

public record Command_Confirmar_Usuario(string Email, string Codigo) : IRequest<bool>;