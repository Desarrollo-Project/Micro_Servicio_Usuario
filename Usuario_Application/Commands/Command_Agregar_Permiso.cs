using MediatR;
using Usuario_Application.DTOs;

namespace Usuario_Application.Commands;

public record Command_Agregar_Permiso(Dto_Actualizar_Permiso Payload) : IRequest<bool>;