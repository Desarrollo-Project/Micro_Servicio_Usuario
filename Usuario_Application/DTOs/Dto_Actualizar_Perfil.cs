namespace Usuario_Application.DTOs;

public record Dto_Actualizar_Perfil(
    Guid UsuarioId,
    string Nombre,
    string Apellido,
    string Correo,
    string Telefono,
    string Direccion
);