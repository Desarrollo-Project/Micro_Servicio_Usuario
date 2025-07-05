using MediatR;
using Usuario_Application.Queries;
using Usuario_Domain.Entities;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Get_Usuario_Id: IRequestHandler<Query_Get_Usuario_Id, Usuario_Mongo>
{
    private readonly IMongoRepository<Usuario_Mongo> _mongoRepository;

    public Handler_Get_Usuario_Id(IMongoRepository<Usuario_Mongo> mongoRepository)
    {
        _mongoRepository = mongoRepository;
    }

    public async Task<Usuario_Mongo> Handle(Query_Get_Usuario_Id request, CancellationToken cancellationToken)
    {
        return await _mongoRepository.GetByIdAsync(request.UsuarioId.ToString());
    }
}