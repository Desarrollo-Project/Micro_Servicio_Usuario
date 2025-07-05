using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Usuario_Application.Commands;
using Usuario_Application.DTOs;
using Usuario_Application.Queries;
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

    /// <summary>
    /// Confirma la cuenta de usuario con un código recibido por correo.
    /// </summary>
    [HttpPatch("confirmar")]
    public async Task<IActionResult> ConfirmarCuenta([FromBody] Dto_Confirmar_Usuario dto)
    {
        var command = new Command_Confirmar_Usuario(dto.Email, dto.Codigo);
        var result = await _mediator.Send(command);
        return result ? Ok(new { message = "Cuenta confirmada" }) : BadRequest(new { message = "Código inválido o expirado" });

    }

    /// <summary>
    /// Obtiene un usuario por correo electrónico.
    /// </summary>
    [HttpGet("email/{email}")]
    public async Task<IActionResult> Get_Usuario_Correo([FromRoute] string email)
    {
        var usuario = await _mediator.Send(new Query_Get_Usuario_Correo(email));
        return usuario != null ? Ok(usuario) : NotFound();
    }

    /// <summary>
    /// Obtiene un usuario por ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUsuario(Guid id)
    {
        var usuario = await _mediator.Send(new Query_Get_Usuario_Id(id));
        return usuario != null ? Ok(usuario) : NotFound();
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

    // <summary>
    /// Obtiene el historial de actividades de un usuario con filtros opcionales.
    /// </summary>
    [Authorize]
    [HttpGet("{usuarioId}/historial")]
    public async Task<IActionResult> ObtenerHistorial(
        Guid usuarioId,
        [FromQuery] string? tipoAccion,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var actividades = await _mediator.Send(new Query_Get_Historial(
            usuarioId,
            tipoAccion,
            desde,
            hasta
        ));

        return Ok(actividades);
    }


    #endregion

    #region Endpoints administrativos

    /// <summary>
    /// Obtiene todas las actividades del sistema.
    /// </summary>
    [Authorize]
    [Permiso_Requerido("gestionar usuarios")]
    [HttpGet("actividades")]
    public async Task<IActionResult> ObtenerTodasLasActividades()
    {
        var actividades = await _mediator.Send(new Query_Get_All_Actividades());
        return Ok(actividades);
    }

    /// <summary>
    /// Obtiene la lista completa de usuarios.
    /// </summary>
    [Authorize]
    [Permiso_Requerido("gestionar usuarios")]
    [HttpGet("usuarios_all")]
    public async Task<IActionResult> ObtenerTodosLosUsuarios()
    {
        var usuarios = await _mediator.Send(new Query_Get_All_Usuarios());
        return Ok(usuarios);
    }

    /// <summary>
    /// Asigna un rol específico a un usuario.
    /// </summary>
    [Authorize]
    [Permiso_Requerido("gestionar roles y permisos")]
    [HttpPatch("roles/{usuarioId}")]
    public async Task<IActionResult> AsignarRol(Guid usuarioId, [FromBody] Dto_Asignar_Rol dto)
    {
        var command = new Command_Asignar_Rol(usuarioId, dto.Rolid);
        await _mediator.Send(command);
        return Ok("Rol asignado");
    }

    /// <summary>
    /// Reemplaza los permisos asignados a un rol.
    /// </summary>
    [Authorize]
    [Permiso_Requerido("gestionar roles y permisos")]
    [HttpPut("roles/permisos")]
    public async Task<IActionResult> ModificarPermisosDeRol([FromBody] Dto_Modificar_Permisos_Rol dto)
    {
        await _mediator.Send(new Command_Modificar_Permisos_Rol(dto));
        return Ok("Permisos del rol modificados correctamente");
    }

    /// <summary>
    /// Agrega un permiso a un rol específico.
    /// </summary>
    [Authorize]
    [Permiso_Requerido("gestionar roles y permisos")]
    [HttpPost("roles/permisos")]
    public async Task<IActionResult> AgregarPermiso([FromBody] Dto_Actualizar_Permiso dto)
    {
        var exito = await _mediator.Send(new Command_Agregar_Permiso(dto));
        return exito ? Ok("Permiso agregado") : BadRequest("Permiso no encontrado o error al asignar");
    }

    /// <summary>
    /// Elimina un permiso de un rol.
    /// </summary>
    [Authorize]
    [Permiso_Requerido("gestionar roles y permisos")]
    [HttpDelete("roles/permisos")]
    public async Task<IActionResult> EliminarPermiso([FromBody] Dto_Actualizar_Permiso dto)
    {
        var exito = await _mediator.Send(new Command_Eliminar_Permiso(dto));
        return exito ? Ok("Permiso eliminado") : BadRequest("Permiso no encontrado o error al eliminar");
    }

    #endregion

}