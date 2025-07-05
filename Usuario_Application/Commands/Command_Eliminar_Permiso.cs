using MediatR;
using Usuario_Application.DTOs;

namespace Usuario_Application.Commands;

public record Command_Eliminar_Permiso(Dto_Actualizar_Permiso Payload) : IRequest<bool>;