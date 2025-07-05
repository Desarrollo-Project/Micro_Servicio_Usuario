using MediatR;

namespace Usuario_Application.Commands;

public record Command_Generar_Token_Recuperacion(string Correo) : IRequest<bool>;