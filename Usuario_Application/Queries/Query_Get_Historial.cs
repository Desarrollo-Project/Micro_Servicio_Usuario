using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Usuario_Domain.Entities;

namespace Usuario_Application.Queries;

public record Query_Get_Historial(
    Guid UsuarioId,
    string? TipoAccion,
    DateTime? Desde,
    DateTime? Hasta
) : IRequest<IEnumerable<Actividad_Mongo>>;
