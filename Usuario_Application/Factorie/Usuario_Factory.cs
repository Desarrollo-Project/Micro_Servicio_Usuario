using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuario_Domain.Entities;
using Usuario_Domain.Interfaces;

namespace Usuario_Application.Factorie;

public class Usuario_Factory : IUsuarioFactory
{
    public Usuario CrearUsuario(string nombre, string apellido, string password, string correo, string telefono, string direccion, int rol_id)
    {
        return new Usuario(nombre, apellido, password, correo, telefono, direccion, rol_id);
    }
}
