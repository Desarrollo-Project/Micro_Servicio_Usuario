using MediatR;
using MongoDB.Driver;
using Usuario_Application.Queries;
using Usuario_Domain.Entities;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Get_Usuario_Correo: IRequestHandler<Query_Get_Usuario_Correo, Usuario_Mongo>
{
    private readonly IMongoRepository<Usuario_Mongo> _mongoRepositorio;

    public Handler_Get_Usuario_Correo(IMongoRepository<Usuario_Mongo> mongoRepositorio)
    {
        _mongoRepositorio = mongoRepositorio;
    }

    public async Task<Usuario_Mongo> Handle(Query_Get_Usuario_Correo request, CancellationToken cancellationToken)
    {
        var filtro = Builders<Usuario_Mongo>.Filter.Eq(u => u.Correo, request.correo);
        var usuario = await _mongoRepositorio.FindAsync(filtro);
        return usuario.FirstOrDefault();

    }
}