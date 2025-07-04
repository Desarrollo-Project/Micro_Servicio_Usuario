using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface IRol_Repositorio
{
    Task<Rol> GetByIdAsync(int id);
}