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

    #endregion
}