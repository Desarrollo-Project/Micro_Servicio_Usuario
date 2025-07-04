using MongoDB.Driver;
using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface IMongoRepository<TDocument>
{
    Task<TDocument> GetByIdAsync(string id);
    Task<IEnumerable<TDocument>> FindAsync(FilterDefinition<TDocument> filter);

}