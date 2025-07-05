namespace Usuario_Application.DTOs;

public record Dto_Restablecer_Password(
    string Token,
    string NuevaPassword,
    string ConfirmarPassword
);