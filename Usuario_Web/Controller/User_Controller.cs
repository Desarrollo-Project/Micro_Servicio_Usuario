using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Usuario_Application.Commands;
using Usuario_Application.DTOs;
using Usuario_Domain.Exceptions;

namespace Usuario_Web.Controller;

/// <summary>
/// Controlador principal para operaciones relacionadas a usuarios: gestión, autenticación y autorización.
/// </summary>
[ApiController]
[Route("api/Usuarios")]
public class User_Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public User_Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Endpoints públicos

    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CrearUsuario([FromBody] Dto_Registrar_Usuario dto)
    {
        var command = new Command_Crear_Usuario(
            dto.Nombre,
            dto.Apellido,
            dto.Username,
            dto.Password,
            dto.Correo,
            dto.Telefono,
            dto.Direccion,
            dto.Rol_id);
        var usuarioId = await _mediator.Send(command);
        return CreatedAtAction(nameof(CrearUsuario), new { id = usuarioId });
    }

    /// <summary>
    /// Solicita un token de recuperación de contraseña.
    /// </summary>
    [HttpPost("solicitar-recuperacion")]
    public async Task<IActionResult> SolicitarRecuperacion([FromBody] Dto_Solicitar_Recuperacion dto)
    {
        var result = await _mediator.Send(new Command_Generar_Token_Recuperacion(dto.Correo));
        return result ? Ok() : BadRequest("Correo no registrado");
    }

    /// <summary>
    /// Restablece la contraseña usando un token.
    /// </summary>
    [HttpPatch("restablecer-password")]
    public async Task<IActionResult> RestablecerPassword([FromBody] Dto_Restablecer_Password dto)
    {
        var command = new Command_Restablecer_Password(dto.Token, dto.NuevaPassword);
        var result = await _mediator.Send(command);
        return result ? Ok("Contraseña actualizada") : BadRequest("Token inválido o expirado");
    }

    #endregion

    #region Endpoints autenticados

    /// <summary>
    /// Cambia la contraseña del usuario autenticado.
    /// </summary>
    [Authorize]
    [HttpPatch("cambiar-password")]
    public async Task<IActionResult> CambiarPassword([FromBody] Dto_Cambiar_Password dto)
    {
        var command = new Command_Cambiar_Password(dto.UsuarioId, dto.PasswordActual, dto.NuevoPassword);
        var result = await _mediator.Send(command);
        return result ? Ok("Contraseña cambiada") : BadRequest("Error al cambiar la contraseña");
    }

    /// <summary>
    /// Actualiza el perfil del usuario.
    /// </summary>
    [Authorize]
    [HttpPut("actualizar-perfil")]
    public async Task<IActionResult> ActualizarPerfil([FromBody] Dto_Actualizar_Perfil dto)
    {
        var command = new Command_Actualizar_Perfil(
            dto.UsuarioId,
            dto.Nombre,
            dto.Apellido,
            dto.Correo,
            dto.Telefono,
            dto.Direccion);
        var result = await _mediator.Send(command);
        return result ? Ok("Perfil actualizado") : BadRequest("Error al actualizar el perfil");
    }

    #endregion
}