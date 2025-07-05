using MediatR;
using MongoDB.Driver;
using Usuario_Application.Queries;
using Usuario_Domain.Entities;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;


public class Handler_Get_All_Actividades : IRequestHandler<Query_Get_All_Actividades, IEnumerable<Actividad_Mongo>>
{
    private readonly IMongoRepository<Actividad_Mongo> _repository;

    public Handler_Get_All_Actividades(IMongoRepository<Actividad_Mongo> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Actividad_Mongo>> Handle(Query_Get_All_Actividades request, CancellationToken cancellationToken)
    {
        var filter = Builders<Actividad_Mongo>.Filter.Empty;
        return await _repository.FindAsync(filter);

    }
}