namespace Usuario_Application.DTOs;

public record Dto_Registrar_Usuario(
    string Nombre,
    string Apellido,
    string Username,
    string Password,
    string Correo,
    string Telefono,
    string Direccion,
    int Rol_id
);