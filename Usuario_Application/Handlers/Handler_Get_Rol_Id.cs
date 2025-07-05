using MediatR;
using MongoDB.Driver;
using Usuario_Application.Queries;
using Usuario_Domain.Entities;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Handlers;

public class Handler_Get_Rol_Id : IRequestHandler<Query_Get_Rol_Id, Rol_Mongo>
{
    private readonly IMongoRepository<Rol_Mongo> _mongo_repositorio;
    public Handler_Get_Rol_Id(IMongoRepository<Rol_Mongo> mongo_repositorio)
    {
        _mongo_repositorio = mongo_repositorio;
    }

    public async Task<Rol_Mongo> Handle(Query_Get_Rol_Id request, CancellationToken cancellationToken)
    {
        var filtro = Builders<Rol_Mongo>.Filter.Eq(r => r.Id, request.rol_id);
        var rol = await _mongo_repositorio.FindAsync(filtro);
        return rol.FirstOrDefault() ?? throw new KeyNotFoundException($"Rol con ID {request.rol_id} no encontrado.");
    }
}