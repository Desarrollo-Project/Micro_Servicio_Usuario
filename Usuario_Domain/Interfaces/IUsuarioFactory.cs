using Usuario_Domain.Entities;

namespace Usuario_Domain.Interfaces;

public interface IUsuarioFactory
{
    Usuario CrearUsuario(string nombre, string apellido, string password, string correo, string telefono, string direccion, int rol_id);

}