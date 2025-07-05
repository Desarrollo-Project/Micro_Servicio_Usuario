using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Usuario_Domain.Entities;

namespace Usuario_Application.Queries;

public record Query_Get_Usuario_Id(Guid UsuarioId) : IRequest<Usuario_Mongo>;
