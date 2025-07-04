using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface IActividad_Repository
{
    Task RegistrarActividad(Actividad actividad);
}