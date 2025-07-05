namespace Usuario_Application.DTOs;

public record Dto_Cambiar_Password(
    Guid UsuarioId,
    string PasswordActual,
    string NuevoPassword,
    string ConfirmarPassword
);