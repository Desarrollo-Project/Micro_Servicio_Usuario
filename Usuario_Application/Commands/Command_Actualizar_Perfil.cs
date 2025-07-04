using MediatR;

namespace Usuario_Application.Commands;

public record Command_Actualizar_Perfil(
    Guid UsuarioId,
    string Nombre,
    string Apellido,
    string Correo,
    string Telefono,
    string Direccion
) : IRequest<bool>;