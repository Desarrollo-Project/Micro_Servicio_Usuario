using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface IPermiso_Repositorio
{
    Task<Permiso> GetByIdAsync(int id);
}