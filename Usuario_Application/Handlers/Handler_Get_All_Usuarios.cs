using MediatR;
using MongoDB.Driver;
using Usuario_Application.Queries;
using Usuario_Domain.Entities;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Get_All_Usuarios: IRequestHandler<Query_Get_All_Usuarios, IEnumerable<Usuario_Mongo>>
{
    private readonly IMongoRepository<Usuario_Mongo> _repository;

    public Handler_Get_All_Usuarios(IMongoRepository<Usuario_Mongo> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Usuario_Mongo>> Handle(Query_Get_All_Usuarios request, CancellationToken cancellationToken)
    {
        var filter = Builders<Usuario_Mongo>.Filter.Empty;
        return await _repository.FindAsync(filter);

    }

}

