using MediatR;

namespace Usuario_Application.Commands;

public record Command_Crear_Usuario(
    string Nombre,
    string Apellido,
    string Username,
    string Password,
    string Correo,
    string Telefono,
    string Direccion,
    int rol_id
) : IRequest<Guid>;