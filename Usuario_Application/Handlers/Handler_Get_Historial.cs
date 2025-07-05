using MediatR;
using MongoDB.Driver;
using Usuario_Application.Queries;
using Usuario_Domain.Entities;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Get_Historial: IRequestHandler<Query_Get_Historial, IEnumerable<Actividad_Mongo>>
{
    private readonly IMongoRepository<Actividad_Mongo> _repository;

    public Handler_Get_Historial(IMongoRepository<Actividad_Mongo> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Actividad_Mongo>> Handle(Query_Get_Historial request, CancellationToken cancellationToken)
    {
        var filter = Builders<Actividad_Mongo>.Filter.Eq(a => a.UsuarioId, request.UsuarioId);

        if (!string.IsNullOrEmpty(request.TipoAccion))
            filter &= Builders<Actividad_Mongo>.Filter.Eq(a => a.TipoAccion, request.TipoAccion);

        if (request.Desde.HasValue)     
            filter &= Builders<Actividad_Mongo>.Filter.Gte(a => a.Fecha, request.Desde);

        if (request.Hasta.HasValue)
            filter &= Builders<Actividad_Mongo>.Filter.Lte(a => a.Fecha, request.Hasta);

        var resultados = await _repository.FindAsync(filter);
        return resultados.OrderByDescending(a => a.Fecha);
    }
}