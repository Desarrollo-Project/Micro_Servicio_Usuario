using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Usuario_Domain.Entities;

namespace Usuario_Application.Queries;

public class Query_Get_All_Actividades : IRequest<IEnumerable<Actividad_Mongo>> { }
