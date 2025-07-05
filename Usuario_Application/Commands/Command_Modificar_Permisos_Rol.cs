using MediatR;
using Usuario_Application.DTOs;

namespace Usuario_Application.Commands;

public record Command_Modificar_Permisos_Rol(Dto_Modificar_Permisos_Rol Payload) : IRequest<bool>;