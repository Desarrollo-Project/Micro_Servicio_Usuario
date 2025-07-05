using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Usuario_Application.DTOs;
using Usuario_Domain.Entities;

namespace Usuario_Application.Queries;

public class Query_Obtener_Roles_y_Permisos : IRequest<List<Dto_Rol_Con_Permisos>> { }
