using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface IUsuario_Repository
{
    Task AddAsync(Usuario usuario);
    Task UpdateAsync(Usuario usuario);
    Task<Usuario> GetByIdAsync(Guid id);
    Task<Usuario> GetByEmail(string email);
    Task<Usuario> GetByTokenRecuperacion(string token);
}