using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface INotificaciones_Cliente
{
    Task EnviarNotificacionCambioClaveAsync(string email, string nombre);
    Task EnviarCorreoConfirmacionAsync(string email, string nombre, string codigo);
    Task EnviarTokenRecuperacionAsync(string email, string nombre, string token);

}